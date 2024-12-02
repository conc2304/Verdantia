using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CityMetricsManager : MonoBehaviour
{

    // Global city metrics
    public int startingBudget = 1000000;
    public float startingTemp = 67.0f;
    public float tempSensitivity = 0.05f; // Sensitivity factor for how much extra heat affects energy and emissions...
    public float cityTemperature { get; private set; }
    public float population { get; private set; }
    public float happiness { get; private set; }
    public float budget { get; private set; }
    public float urbanHeat { get; private set; }
    public float pollution { get; private set; }
    public float energy { get; private set; }
    public float carbonEmission { get; private set; }

    // Time-keeping variables
    public int currentMonth = 1;
    public int currentYear = 2024;
    public float monthDuration = 30.0f; // Duration of a "month" in seconds 
    private float monthTimer = 0f;
    private MissionManager missionManager;
    private int missionMonthsRemaining = 0;
    public event Action<int, int, int> OnTimeUpdated; // (month, year, monthsRemaining)
    public event Action OnMetricsUpdate;

    // Metric Setter and Getters
    private Dictionary<MetricTitle, float> metrics;
    public Dictionary<string, (float min, float max)> propertyRanges;

    public float revenue { get; private set; }
    public CameraController cameraController;
    public BuildingsMenuNew buildingsMenu;

    private CityTemperatureController cityTemperatureController;



    void Start()
    {
        cityTemperature = startingTemp;
        budget = startingBudget;
        cityTemperatureController.startingTemp = startingTemp;

        UpdateCityMetrics();

        // Initialize the dictionary for easier access
        metrics = new Dictionary<MetricTitle, float>
        {
            { MetricTitle.CityTemperature, cityTemperature },
            { MetricTitle.UrbanHeat, urbanHeat },
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
        missionManager = FindObjectOfType<MissionManager>();
        cityTemperatureController.OnTempUpdated += HandleUpdateTemperature;
    }

    void Update()
    {
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

        if (missionManager.currentMission != null && !missionManager.IsMissionFreePlay())
        {
            missionMonthsRemaining = missionManager.currentMission.GetMonthsRemaining(currentMonth, currentYear);
        }

        // Notify any systems 
        OnMetricsUpdate?.Invoke();
        OnTimeUpdated?.Invoke(currentMonth, currentYear, missionMonthsRemaining);
        propertyRanges = buildingsMenu.GetPropertyRanges();

    }

    // Update budget based on revenue and expenses (done every "month")
    private void UpdateMonthlybudget()
    {
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
        int cityArea = 0;

        // Variables for happiness calculation
        float totalHappinessImpact = 0f;
        float totalWeight = 0f;

        bool includeSpaces = false;
        List<Transform> cityBuildings = cameraController.GetAllBuildings(includeSpaces);
        int totalCityBuildings = cityBuildings.Count;
        float totalPopulation = 0;

        const float BaseWeight = 5f;        // Intrinsic weight for low population buildings

        print($"--- UpdateCityMetrics --- ");

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
            if (buildingProps.capacity > 50)
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
            totalHappinessImpact += adjustedHappiness * populationWeight;

            // Accumulate weight and population
            totalWeight += populationWeight;
            totalPopulation += buildingProps.capacity;

            // Apply adjusted metrics
            urbanHeat += (int)adjustedHeatContribution;
            pollution += (int)adjustedPollution;
            energy += (int)adjustedEnergyConsumption;
            carbonEmission += (int)adjustedCarbonFootprint;
        }


        // Calculate overall city happiness
        // Calculate normalization factor (adjust based on city size and population)
        float normalizationFactor = Mathf.Max(1, totalCityBuildings + totalPopulation); // Prevent division by zero
        happiness += totalHappinessImpact / normalizationFactor;
        happiness = Mathf.Clamp(happiness, 0f, 100f);

        CleanMetrics();
        OnMetricsUpdate?.Invoke();
    }



    // Method to reset all metrics to initial state before recalculation
    public void ResetMetrics()
    {
        population = 0;
        happiness = 50;
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


    public float GetMetricValue(MetricTitle metricName)
    {
        return metrics.ContainsKey(metricName) ? metrics[metricName] : 0f;
    }

    public void HandleUpdateTemperature(float avgTemp, float lowTemp, float hightTemp)
    {
        cityTemperature = avgTemp;
    }

    void OnDestroy()
    {
        cityTemperatureController.OnTempUpdated -= HandleUpdateTemperature;
    }

}




