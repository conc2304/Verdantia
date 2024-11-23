using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CityMetricsManager : MonoBehaviour
{


    // Global city metrics
    public int startingBudget = 1000000;
    public float startingTemp = 67.0f;
    public float cityTempLow = float.PositiveInfinity;
    public float cityTempMax = float.NegativeInfinity;

    // Time-keeping variables
    public int currentMonth = 1; // January starts as 1
    public int currentYear = 2024; // Set the starting year
    public float monthDuration = 30.0f; // Duration of a "month" in seconds (for testing purposes)
    private float monthTimer = 0f;

    public event Action<int, int> OnTimeUpdated; // (month, year)
    public event Action OnMetricsUpdate;
    public event Action OnTempUpdated;

    // Metric Setter and Getters
    private Dictionary<MetricTitle, float> metrics;
    private Dictionary<string, (float min, float max)> propertyRanges;

    public float tempSensitivity = 0.05f; // Sensitivity factor for how much extra heat affects energy and emissions
    public float cityTemperature { get; private set; }
    public float population { get; private set; }
    public float happiness { get; private set; }
    public float budget { get; private set; }
    public float greenSpace { get; private set; }
    public float urbanHeat { get; private set; }
    public float pollution { get; private set; }
    public float energy { get; private set; }
    public float carbonEmission { get; private set; }
    public float revenue { get; private set; }
    public CameraController cameraController;
    public BuildingsMenuNew buildingsMenu;


    private int gridTileSize = 10;

    private float cityBoarderMinX = float.MaxValue;
    private float cityBoarderMaxX = float.MinValue;
    private float cityBoarderMinZ = float.MaxValue;
    private float cityBoarderMaxZ = float.MinValue;


    // City Temperature Heat Diffusion 
    // public float[,] cityTempGrid { get; private set; }

    [Header("Heat Map Debug")]

    public float heatAddRange = 0.05f;
    private HeatMap heatMap;
    private CityTemperatureController cityTemperatureController;


    void Start()
    {
        gridTileSize = cameraController.gridSize;
        cityTemperature = startingTemp;
        budget = startingBudget;

        cityTemperatureController.startingTemp = startingTemp;


        UpdateCityMetrics();
        OnTempUpdated?.Invoke();

        // Initialize the dictionary for easier access
        metrics = new Dictionary<MetricTitle, float>
        {
            { MetricTitle.CityTemperature, cityTemperature },
            { MetricTitle.UrbanHeat, urbanHeat },
            { MetricTitle.GreenSpace, greenSpace },
            { MetricTitle.Budget, budget },
            { MetricTitle.Happiness, happiness },
            { MetricTitle.Pollution, pollution },
            { MetricTitle.Population, population },
            { MetricTitle.CarbonEmission, carbonEmission },
        };
    }

    private void Awake()
    {
        cityTemperatureController = FindObjectOfType<CityTemperatureController>();
    }

    public float GetMetricValue(MetricTitle metricName)
    {
        return metrics.ContainsKey(metricName) ? metrics[metricName] : 0f;
    }


    void Update()
    {
        // handle heat map updates on city change on fixed monthly intervals

        HandleDateChange();
    }

    public void HandleDateChange()
    {
        monthTimer += Time.deltaTime;
        if (monthTimer >= monthDuration)
        {
            // Every month, calculate the city's financial status
            UpdateMonthlybudget();
            UpdateCityMetrics();
            AdvanceMonth();
            monthTimer = 0f;
        }
    }



    void AdvanceMonth()
    {
        // Advance the month
        currentMonth++;

        // Check if we've reached the end of the year
        if (currentMonth > 12)
        {
            currentMonth = 1; // Reset to January
            currentYear++; // Move to the next year
        }

        // Notify any systems 
        OnMetricsUpdate?.Invoke();
        OnTempUpdated?.Invoke();
        OnTimeUpdated?.Invoke(currentMonth, currentYear);
        propertyRanges = buildingsMenu.GetPropertyRanges();

    }

    // Update budget based on revenue and expenses (done every "month")
    private void UpdateMonthlybudget()
    {
        // Update the budget by subtracting expenses from income
        budget += revenue;
    }

    // Update city metrics based on all buildings
    public void UpdateCityMetrics()
    {
        propertyRanges = buildingsMenu.GetPropertyRanges();
        if (propertyRanges == null) return;

        // Reset all metrics before recalculating them
        ResetMetrics();

        float tempDifference = Mathf.Clamp(-10, 10, cityTemperature - startingTemp);
        tempDifference = 0f; // TODO remove this later 
        int cityArea = 0;

        // Variables for happiness calculation
        // Variables for happiness calculation
        float totalHappinessImpact = 0f;
        float totalWeight = 0f;

        // Include additional spaces in the "allBuildings" list
        bool includeSpaces = false;
        List<Transform> cityBuildings = cameraController.GetAllBuildings(includeSpaces);
        int totalCityBuildings = cityBuildings.Count;
        float totalPopulation = 0;

        // Constants for greenspace and intrinsic weights
        // const float GreenMultiplier = 0.5f; // Amplifies the effect of greenspace
        const float BaseWeight = 5f;        // Intrinsic weight for capacity-zero buildings

        print($"--- UpdateCityMetrics --- ");
        // print($"#Buildings : {cityBuildings.Count}");

        foreach (Transform building in cityBuildings)
        {
            if (!(building.CompareTag("Building") || building.CompareTag("Road")))
            {
                // Skip buildings' additionalSpaces to avoid over-counting
                continue;
            }

            building.TryGetComponent(out BuildingProperties buildingProps);
            if (!buildingProps)
            {
                Debug.LogError("Building Props is Null for : " + building.name);
                continue;
            }

            cityArea += buildingProps.additionalSpace.Count() + 1; // 1 for the building, and then count the spaces

            // Cumulative Metrics
            population += (float)Math.Round(buildingProps.capacity);
            revenue += buildingProps.cityRevenue;
            greenSpace += buildingProps.greenSpaceEffect;

            // Non standard metrics
            float adjustedHeatContribution = buildingProps.heatContribution;
            float adjustedEnergyConsumption = buildingProps.netEnergy;
            float adjustedCarbonFootprint = buildingProps.carbonFootprint;
            float adjustedHappiness = buildingProps.happinessImpact;
            float adjustedPollution = buildingProps.pollutionImpact;


            // Apply additional feedback if temperature is above base
            if (tempDifference != 0)
            {
                adjustedEnergyConsumption += adjustedEnergyConsumption * tempDifference * tempSensitivity;
                adjustedCarbonFootprint += adjustedCarbonFootprint * tempDifference * tempSensitivity;
                adjustedHeatContribution += adjustedHeatContribution * tempDifference * tempSensitivity;
                adjustedHappiness += adjustedHappiness * tempDifference * tempSensitivity;
                adjustedPollution += adjustedPollution * tempDifference * tempSensitivity;
            }

            // Calculate weight based on building type
            // Calculate dynamic GreenMultiplier based on greenspaceEffect
            float greenMultiplier = (buildingProps.greenSpaceEffect + 100f) / 200f;
            float populationWeight = 0f;

            // Calculate weight based on building type
            if (buildingProps.capacity > 0)
            {
                // Population-based weight for residential/commercial buildings
                populationWeight = buildingProps.capacity + buildingProps.effectRadius * greenMultiplier;
            }
            else
            {
                // Weight for capacity-zero buildings (parks, forests, etc.)
                populationWeight = buildingProps.effectRadius * greenMultiplier + BaseWeight;
            }

            // Add to happiness impact
            totalHappinessImpact += buildingProps.happinessImpact * populationWeight;

            // Accumulate weight and population
            totalWeight += populationWeight;
            totalPopulation += buildingProps.capacity;

            // Apply adjusted metrics
            // happiness += (int)adjustedHappiness;
            urbanHeat += (int)adjustedHeatContribution;
            pollution += (int)adjustedPollution;
            energy += (int)adjustedEnergyConsumption;
            carbonEmission += (int)adjustedCarbonFootprint;
        }
        // Debug.Log($"total happiness : {totalHappinessImpact}");
        // Debug.Log($"totalPopulationWeight : {totalWeight}");

        // Calculate overall city happiness
        // Calculate normalization factor (adjust based on city size and population)
        float normalizationFactor = Mathf.Max(1, totalCityBuildings + totalPopulation); // Prevent division by zero

        // Adjust city happiness
        happiness += totalHappinessImpact / normalizationFactor;
        // print($"City Happiness: {happiness} | -2");

        // Clamp city happiness to the range [0, 100]
        happiness = Mathf.Clamp(happiness, 0f, 100f);

        // Log the city happiness for debugging
        // print($"City Happiness: {happiness} | -1");


        CleanMetrics();
        OnMetricsUpdate?.Invoke();
    }



    // Method to reset all metrics to initial state before recalculation
    public void ResetMetrics()
    {
        population = 0;
        happiness = 50;
        greenSpace = 0;
        urbanHeat = 0;
        pollution = 0;
        energy = 0;
        carbonEmission = 0;
        revenue = 0;
        // Budget does not get reset
    }

    private void CleanMetrics()
    {
        population = (float)Math.Round(population);
        happiness = (float)Math.Round(happiness);
        greenSpace = (float)Math.Round(greenSpace);
        urbanHeat = (float)Math.Round(urbanHeat);
        pollution = (float)Math.Round(pollution);
        energy = (float)Math.Round(energy);
        carbonEmission = (float)Math.Round(carbonEmission);
        revenue = (float)Math.Round(revenue);
    }

    public void AddRevenue(float amount)
    {
        budget += amount;
        OnMetricsUpdate?.Invoke();
    }

    public void DeductExpenses(float amount)
    {
        budget -= amount;
        OnMetricsUpdate?.Invoke();
    }



    public float CalculateOverallGreenSpaceEffect()
    {
        float totalGreenSpaceEffect = 0;
        int totalBuildingsWithGreenSpaceEffect = 0;

        // Constants for normalization
        float maxGreenSpaceEffect = 60f;
        float maxEffectRadius = 6f;
        float normalizationFactor = maxGreenSpaceEffect * maxEffectRadius; // 360

        bool includeSpaces = false;
        foreach (Transform building in cameraController.GetAllBuildings(includeSpaces))
        {
            BuildingProperties buildingProps = building.GetComponent<BuildingProperties>();

            // Only consider buildings with a positive green space effect
            if (buildingProps.greenSpaceEffect != 0 && buildingProps.effectRadius > 0)
            {
                // Calculate weighted green space effect
                float weightedGreenSpaceEffect = buildingProps.greenSpaceEffect * buildingProps.effectRadius;
                float normalizedGreenSpaceContribution = weightedGreenSpaceEffect / normalizationFactor;

                // Sum up the normalized contribution
                totalGreenSpaceEffect += normalizedGreenSpaceContribution;
                totalBuildingsWithGreenSpaceEffect++;
            }
        }

        // Scale up to make it more readable (e.g., on a 0-100 scale)
        float cityGreenSpaceEffect = totalGreenSpaceEffect * 100;

        // Optional: If you want to adjust based on green space coverage of residential areas
        // float coverageScore = CalculateGreenSpaceCoverageScore(greenSpaces, residentialBuildings);
        // cityGreenSpaceEffect *= (coverageScore / 100);

        return Mathf.Round(cityGreenSpaceEffect);
    }


    //   Key: carbonFootprint, Min: -500, Max: 5000
    //   Key: heatContribution, Min: -50, Max: 100





    public float[,] AddHeat(float[,] tempsGrid)
    {
        if (propertyRanges == null)
        {
            propertyRanges = buildingsMenu.GetPropertyRanges();
            if (propertyRanges == null) return new float[0, 0];
        }

        int rescaleVal = gridTileSize;

        cityBoarderMinX = float.MaxValue;
        cityBoarderMaxX = float.MinValue;
        cityBoarderMinZ = float.MaxValue;
        cityBoarderMaxZ = float.MinValue;

        // Store occupied grid positions
        HashSet<(int, int)> occupiedGridPositions = new HashSet<(int, int)>();

        // First, determine all occupied positions
        bool includeSpaces = false;
        foreach (Transform building in cameraController.GetAllBuildings(includeSpaces))
        {
            // only do Buildings and Roads
            if (!(building.CompareTag("Building") || building.CompareTag("Road"))) continue;

            BuildingProperties buildingProps = building.GetComponent<BuildingProperties>();
            if (buildingProps == null) continue;

            // Get the footprint of the building and its additional spaces
            List<(int, int)> buildingFootprint = new List<(int, int)>();
            int baseX = Mathf.RoundToInt(building.position.x / rescaleVal);
            int baseZ = Mathf.RoundToInt(building.position.z / rescaleVal);

            // Mark the building's position as occupied
            occupiedGridPositions.Add((baseX, baseZ));
            buildingFootprint.Add((baseX, baseZ));

            float adjustedHeatContribution = NumbersUtils.Remap(
                propertyRanges["heatContribution"].min,
                propertyRanges["heatContribution"].max,
                -heatAddRange,
                heatAddRange * (propertyRanges["heatContribution"].min / propertyRanges["heatContribution"].max),
                buildingProps.heatContribution
            );

            adjustedHeatContribution = buildingProps.heatContribution * heatAddRange;

            // Check additional spaces to determine the full footprint
            foreach (Transform additionalSpace in buildingProps.additionalSpace)
            {
                int additionalX = Mathf.RoundToInt(additionalSpace.position.x / rescaleVal);
                int additionalZ = Mathf.RoundToInt(additionalSpace.position.z / rescaleVal);

                occupiedGridPositions.Add((additionalX, additionalZ));
                buildingFootprint.Add((additionalX, additionalZ));

                // Apply heat from additional spaces
                tempsGrid[additionalX, additionalZ] += adjustedHeatContribution;

                // Update boundaries for additional space positions
                cityBoarderMinX = Mathf.Min(cityBoarderMinX, additionalX);
                cityBoarderMaxX = Mathf.Max(cityBoarderMaxX, additionalX);
                cityBoarderMinZ = Mathf.Min(cityBoarderMinZ, additionalZ);
                cityBoarderMaxZ = Mathf.Max(cityBoarderMaxZ, additionalZ);
            }

            // Apply heat to the building's grid position
            tempsGrid[baseX, baseZ] += adjustedHeatContribution;

            // Update boundaries based on the building position
            cityBoarderMinX = Mathf.Min(cityBoarderMinX, baseX);
            cityBoarderMaxX = Mathf.Max(cityBoarderMaxX, baseX);
            cityBoarderMinZ = Mathf.Min(cityBoarderMinZ, baseZ);
            cityBoarderMaxZ = Mathf.Max(cityBoarderMaxZ, baseZ);

            float effectRadius = buildingProps.effectRadius;
            // buildingProps.proximityEffects
            bool applyProximityHeat = false;
            foreach (MetricBoost booster in buildingProps.proximityEffects)
            {
                if (booster.metricName == BuildingMetric.heatContribution) applyProximityHeat = true;
            }

            if (applyProximityHeat)
            {
                // Define the surrounding area based on the building footprint
                foreach (var (footprintX, footprintZ) in buildingFootprint)
                {
                    // Check the effect radius surrounding area of each footprint position
                    for (int offsetX = (int)-effectRadius; offsetX <= effectRadius; offsetX++)
                    {
                        for (int offsetZ = (int)-effectRadius; offsetZ <= effectRadius; offsetZ++)
                        {
                            // Skip the center position where the building is located
                            if (offsetX == 0 && offsetZ == 0) continue;

                            int surroundingX = footprintX + offsetX;
                            int surroundingZ = footprintZ + offsetZ;

                            // Check if the surrounding position is occupied
                            if (!occupiedGridPositions.Contains((surroundingX, surroundingZ)))
                            {
                                tempsGrid[surroundingX, surroundingZ] += adjustedHeatContribution;

                                // Update boundaries for surrounding positions
                                cityBoarderMinX = Mathf.Min(cityBoarderMinX, surroundingX);
                                cityBoarderMaxX = Mathf.Max(cityBoarderMaxX, surroundingX);
                                cityBoarderMinZ = Mathf.Min(cityBoarderMinZ, surroundingZ);
                                cityBoarderMaxZ = Mathf.Max(cityBoarderMaxZ, surroundingZ);
                            }
                        }
                    }
                }
            }
        }


        return tempsGrid;
    }




}




