using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CityMetricsManager : MonoBehaviour
{


    // Global city metrics
    public int startingBudget = 1000000;
    public float startingTemp = 69.0f;

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
    private Dictionary<string, (int min, int max)> propertyRanges;

    public float tempSensitivity = 0.05f; // Sensitivity factor for how much extra heat affects energy and emissions
    public float cityTemperature { get; private set; }
    public int population { get; private set; }
    public float happiness { get; private set; }
    public int budget { get; private set; }
    public float greenSpace { get; private set; }
    public int urbanHeat { get; private set; }
    public int pollution { get; private set; }
    public int energy { get; private set; }
    public int carbonEmission { get; private set; }
    public int revenue { get; private set; }
    public int income { get; private set; }
    public int expenses { get; private set; }
    public CameraController cameraController;
    public BuildingsMenuNew buildingsMenu;


    // City Grid Plane
    public Grid grid;
    private int gridLengthX;
    private int gridLengthZ;
    private readonly int gridPadding = 2;
    private int gridTileSize = 10;

    private float cityBoarderMinX = float.MaxValue;
    private float cityBoarderMaxX = float.MinValue;
    private float cityBoarderMinZ = float.MaxValue;
    private float cityBoarderMaxZ = float.MinValue;

    public float cityTempUpdateInterval = 6;
    public int temperatureMapTimeSteps = 5;
    private float cityTempUpdateRate;
    private float cityTempTimer = 0f;

    // City Temperature Heat Diffusion 
    public float[,] temps { get; private set; }
    // NOTE these values are stable
    public float heatDiffusionRate = 0.25f;
    public float heatDissipationRate = 0.999f; // todo remove 
    public float sunHeatBase = 0.06f;
    public float heatAddRange = 0.05f;

    [Header("Heat Map Debug")]
    public bool takeStep = false;
    public bool toggleRestartTemp = false;
    public bool playTemp = false;

    private HeatMap heatMap;

    void Start()
    {
        heatMap = FindObjectOfType<HeatMap>();

        gridTileSize = cameraController.gridSize;
        cityTemperature = startingTemp;
        budget = startingBudget;
        RestartSimulation();

        cityTempUpdateRate = monthDuration / cityTempUpdateInterval;

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
            { MetricTitle.Income, income },
            { MetricTitle.Expenses, expenses }
        };

        gridLengthX = (grid.gridSizeX / gridTileSize) + gridPadding;
        gridLengthZ = (grid.gridSizeZ / gridTileSize) + gridPadding;
    }

    public float GetMetricValue(MetricTitle metricName)
    {
        return metrics.ContainsKey(metricName) ? metrics[metricName] : 0f;
    }

    public void InitializeGrid()
    {
        temps = ArrayFill(gridLengthX, gridLengthZ, startingTemp);
    }

    void Update()
    {
        // Accumulate budget monthly
        monthTimer += Time.deltaTime;
        if (monthTimer >= monthDuration)
        {
            // Every month, calculate the city's financial status
            UpdateMonthlybudget();
            UpdateCityMetrics();
            AdvanceMonth();
            monthTimer = 0f;
        }


        // handle heat map updates on city change on fixed monthly intervals
        cityTempTimer += Time.deltaTime;

        // if (toggleRestartTemp || (playTemp && cityTempTimer >= cityTempUpdateRate))      // TODO remove this after testing
        if (cityTempTimer >= cityTempUpdateRate)
        {
            for (int i = 0; i < temperatureMapTimeSteps; i++)
            {
                GetCityTemperatures();
            }

            if (cameraController.heatmapMetric == "cityTemperature")
            {
                // Make sure we have a valid temperature matrix
                float[,] cityTemps = (temps != null && temps.Length != 0) ?
                    temps :
                    GetCityTemperatures();

                int minTemp = 50;
                int maxTemp = 85;
                // cityTemps = cityMetricsManager.RemovePaddingFromMatrix(cityTemps);
                heatMap.RenderCityTemperatureHeatMap(cityTemps, minTemp, maxTemp);  // TODO investigate if this needs to change 

                return; // city temp is computed and rendered differently
            }

            OnTempUpdated?.Invoke();
        }
        cityTempTimer = 0f;

        // toggleRestartTemp = false; // Reset the toggle if you want it to trigger only once


        // if (toggleRestartTemp)
        // {
        //     RestartSimulation();
        //     toggleRestartTemp = false;
        // }
        // if (takeStep)
        // {
        //     toggleRestartTemp = true;
        //     takeStep = false;
        // }
    }

    public void RestartSimulation()
    {
        InitializeGrid();
        GetCityTemperatures();
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
        // Calculate income from tax revenue
        int monthlyIncome = income;

        // Calculate expenses (operational cost and upkeep of all buildings)
        int monthlyExpenses = expenses;

        // Update the budget by subtracting expenses from income
        budget += monthlyIncome - monthlyExpenses;
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
        int totalPopulation = 0;

        // Constants for greenspace and intrinsic weights
        // const float GreenMultiplier = 0.5f; // Amplifies the effect of greenspace
        const float BaseWeight = 5f;        // Intrinsic weight for capacity-zero buildings

        print($"--- UpdateCityMetrics --- ");
        print($"#Buildings : {cityBuildings.Count}");

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
            population += buildingProps.capacity;
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
        Debug.Log($"total happiness : {totalHappinessImpact}");
        Debug.Log($"totalPopulationWeight : {totalWeight}");

        // Calculate overall city happiness
        // Calculate normalization factor (adjust based on city size and population)
        float normalizationFactor = Mathf.Max(1, totalCityBuildings + totalPopulation); // Prevent division by zero

        // Adjust city happiness
        happiness += totalHappinessImpact / normalizationFactor;
        print($"City Happiness: {happiness} | -2");

        // Clamp city happiness to the range [0, 100]
        happiness = (float)Math.Round(Mathf.Clamp(happiness, 0f, 100f));

        // Log the city happiness for debugging
        print($"City Happiness: {happiness} | -1");
        OnMetricsUpdate?.Invoke();
    }



    // Method to reset all metrics to initial state before recalculation
    private void ResetMetrics()
    {
        population = 0;
        happiness = 50;
        greenSpace = 0;
        urbanHeat = 0;
        pollution = 0;
        energy = 0;
        carbonEmission = 0;
        revenue = 0;
        income = 0;
        expenses = 0;
        // Budget does not get reset
    }

    public void AddRevenue(int amount)
    {
        budget += amount;
        OnMetricsUpdate?.Invoke();
    }

    public void DeductExpenses(int amount)
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

    public float[,] ArrayFill(int sizeX, int sizeY, float initialValue)
    {
        float[,] grid = new float[sizeX, sizeY];

        // Optional: Explicitly setting all values to 0 (not necessary as default for int is 0)
        for (int i = 0; i < sizeX; i++)
        {
            for (int j = 0; j < sizeY; j++)
            {
                grid[i, j] = initialValue;
            }
        }

        return grid;
    }

    public static int[,] RotateMatrix90DegreesClockwise(int[,] matrix)
    {
        int n = matrix.GetLength(0);  // Number of rows
        int m = matrix.GetLength(1);  // Number of columns

        // Create a new matrix to hold the rotated values
        int[,] rotatedMatrix = new int[m, n];

        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < m; j++)
            {
                rotatedMatrix[j, n - 1 - i] = matrix[i, j];
            }
        }

        return rotatedMatrix;
    }



    public float[,] GetCityTemperatures_OG()
    {

        bool includeSpace = false;
        List<Transform> allBuildings = cameraController.GetAllBuildings(includeSpace);

        float[,] outputTemps = ArrayFill(gridLengthX, gridLengthZ, 0);

        float epsilon = 1f; // Small constant to prevent division by zero
        // float maxCarbonEmission = propertyRanges["carbonFootprint"].max;
        float deltaTime = 1;
        float deltaX = 1;

        // float heatDiffusionRate = (float)(deltaTime / Math.Pow(deltaX, 2));             // Alpha
        // heatDiffusionRate = 0.249999999999f;             // Alpha

        // float heatDissipationRate = 1 / ((carbonEmission / maxCarbonEmission) + epsilon); // Beta

        // float b = 1 -deltaTime * heatDissipationRate - 4 * heatDiffusionRate;          // part of formula for finite differences PDE

        float clampedRate = Math.Clamp((4 * heatDiffusionRate) + heatDissipationRate, 0, 0.98f);
        float bk = 1 - clampedRate;

        // float bk = 0.5f;

        // Boundary Conditions
        for (int z = 1; z < gridLengthZ - (gridPadding / 2); z++)
        {
            temps[0, z] = temps[1, z];
            temps[gridLengthX - 1, z] = temps[gridLengthX - 2, z];
        }
        for (int x = 0; x < gridLengthX - (gridPadding / 2); x++)
        {
            temps[x, 0] = temps[x, 1];
            temps[x, gridLengthZ - 1] = temps[x, gridLengthZ - 2];
        }
        int minTemp = 50;
        int maxTemp = 100;

        // Build output temps matrix: Heat Diffusion/Dissipation Forumla
        for (int x = 1; x < gridLengthX - (gridPadding / 2); x++)
        {
            for (int z = 1; z < gridLengthZ - (gridPadding / 2); z++)
            {
                outputTemps[x, z] =
                    (bk * (temps[x, z] - minTemp))
                    + (heatDiffusionRate
                        * (
                            temps[x + 1, z]
                            + temps[x - 1, z]
                            + temps[x, z + 1]
                            + temps[x, z - 1]
                        )
                    );
            }
        }

        outputTemps = AddHeat(outputTemps);

        temps = outputTemps;
        return outputTemps;
    }

    public float[,] GetCityTemperatures()
    {
        print("GetCityTemperatures");
        // Make sure we have a valid grid
        if (gridLengthX == 0 || gridLengthZ == 0)
        {
            gridLengthX = (grid.gridSizeX / gridTileSize) + gridPadding;
            gridLengthZ = (grid.gridSizeZ / gridTileSize) + gridPadding;
        }
        if (gridLengthX == 0 || gridLengthZ == 0) return new float[0, 0];
        if (temps == null || temps.Length == 0) InitializeGrid();


        float[,] newTemps = new float[gridLengthX, gridLengthZ];

        // Boundary Conditions
        for (int z = 1; z < gridLengthZ - (gridPadding / 2); z++)
        {
            // Fix left and right edges to inner cell values, effectively holding temperature steady at the boundaries
            temps[0, z] = temps[1, z];
            temps[gridLengthX - 1, z] = temps[gridLengthX - 2, z];
        }

        for (int x = 1; x < gridLengthX - (gridPadding / 2); x++)
        {
            // Fix top and bottom edges to inner cell values
            temps[x, 0] = temps[x, 1];
            temps[x, gridLengthZ - 1] = temps[x, gridLengthZ - 2];
        }


        float tempMin = 50;

        for (int i = 1; i < gridLengthX - 1; i++)
        {
            for (int j = 1; j < gridLengthZ - 1; j++)
            {
                // Apply heat equation
                float heatDiffusion = heatDiffusionRate * (
                    temps[i + 1, j] + temps[i - 1, j] +
                    temps[i, j + 1] + temps[i, j - 1] -
                    4 * temps[i, j]
                );

                newTemps[i, j] = temps[i, j] + heatDiffusion;
                newTemps[i, j] *= heatDissipationRate; // Apply dissipation factor
                newTemps[i, j] += sunHeatBase; // Apply sun heat to prevent global freeze
                newTemps[i, j] = Math.Max(newTemps[i, j], tempMin);
            }
        }

        newTemps = AddHeat(newTemps);
        cityTemperature = GetAvgTemp(newTemps, 10);
        temps = newTemps;
        return temps;
    }

    public float[,] AddHeat_old(float[,] tempsGrid)
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

        bool includeSpace = true;
        foreach (Transform building in cameraController.GetAllBuildings(includeSpace))
        {
            BuildingProperties buildingProps = building.GetComponent<BuildingProperties>();
            if (buildingProps == null) continue;

            int gridX = Mathf.RoundToInt(building.position.x / rescaleVal);
            int gridZ = Mathf.RoundToInt(building.position.z / rescaleVal);

            float adjustedHeatContribution = NumbersUtils.Remap(
                propertyRanges["heatContribution"].min,
                propertyRanges["heatContribution"].max,
                -heatAddRange,
                heatAddRange * (propertyRanges["heatContribution"].min / propertyRanges["heatContribution"].max),
                buildingProps.heatContribution
            );

            adjustedHeatContribution = buildingProps.heatContribution * heatAddRange;

            // Apply heat to the tempsGrid at the calculated grid position
            tempsGrid[gridX, gridZ] += adjustedHeatContribution; // TODO BUGFIX


            // Update min and max boundaries based on the building position
            cityBoarderMinX = Mathf.Min(cityBoarderMinX, gridX);
            cityBoarderMaxX = Mathf.Max(cityBoarderMaxX, gridX);
            cityBoarderMinZ = Mathf.Min(cityBoarderMinZ, gridZ);
            cityBoarderMaxZ = Mathf.Max(cityBoarderMaxZ, gridZ);

            // Check any additional spaces associated with the building
            foreach (Transform additionalSpace in buildingProps.additionalSpace)
            {
                gridX = Mathf.RoundToInt(additionalSpace.position.x / rescaleVal);
                gridZ = Mathf.RoundToInt(additionalSpace.position.z / rescaleVal);
                tempsGrid[gridX, gridZ] += adjustedHeatContribution;

                // Update boundaries for additional space positions
                cityBoarderMinX = Mathf.Min(cityBoarderMinX, gridX);
                cityBoarderMaxX = Mathf.Max(cityBoarderMaxX, gridX);
                cityBoarderMinZ = Mathf.Min(cityBoarderMinZ, gridZ);
                cityBoarderMaxZ = Mathf.Max(cityBoarderMaxZ, gridZ);
            }
        }

        return tempsGrid;
    }

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

            int effectRadius = buildingProps.effectRadius;
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
                    for (int offsetX = -effectRadius; offsetX <= effectRadius; offsetX++)
                    {
                        for (int offsetZ = -effectRadius; offsetZ <= effectRadius; offsetZ++)
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

    private float GetAvgTemp(float[,] temps, int padding = 5)
    {
        // Calculate the bounds in grid coordinates based on minX, maxX, minZ, maxZ
        int minGridX = Mathf.Clamp(Mathf.RoundToInt(cityBoarderMinX - padding), 0, gridLengthX - 1);
        int maxGridX = Mathf.Clamp(Mathf.RoundToInt(cityBoarderMaxX + padding), 0, gridLengthX - 1);
        int minGridZ = Mathf.Clamp(Mathf.RoundToInt(cityBoarderMinZ - padding), 0, gridLengthZ - 1);
        int maxGridZ = Mathf.Clamp(Mathf.RoundToInt(cityBoarderMaxZ + padding), 0, gridLengthZ - 1);

        // Calculate the average temperature within city bounds
        float totalTemperature = 0f;
        int count = 0;

        for (int i = minGridX; i <= maxGridX; i++)
        {
            for (int j = minGridZ; j <= maxGridZ; j++)
            {
                totalTemperature += temps[i, j];
                count++;
            }
        }

        float averageTemperature = count > 0 ? totalTemperature / count : 0;

        averageTemperature = (float)Math.Round(averageTemperature);
        return averageTemperature;
    }

    public float[,] RemovePaddingFromMatrix(float[,] matrix)
    {
        // Calculate the new dimensions without padding
        int newGridLengthX = grid.gridSizeX / gridTileSize;
        int newGridLengthZ = grid.gridSizeZ / gridTileSize;

        // Initialize a new matrix with the adjusted size
        float[,] shiftedMatrix = new float[newGridLengthX, newGridLengthZ];

        // Fill the new matrix by skipping padding rows and columns
        for (int x = 0; x < newGridLengthX; x++)
        {
            for (int z = 0; z < newGridLengthZ; z++)
            {
                // Offset by the padding to avoid it in the original matrix
                shiftedMatrix[x, z] = matrix[x + gridPadding, z + gridPadding];
            }
        }

        return shiftedMatrix;
    }
}




