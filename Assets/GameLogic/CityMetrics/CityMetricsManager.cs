using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/**
Manages and updates various city-wide metrics in a simulation, such as temperature, budget, population, and happiness. 
It handles the passage of time by simulating months, updates metrics based on building data, 
adjusts metrics dynamically (e.g., due to temperature changes), and maintains a history of metric values over time. 
It also integrates with other systems, such as missions and temperature controllers, 
to ensure accurate metric updates and provides functionality for financial adjustments. 
The script emits events to notify other parts of the system when metrics or time change.
**/
public class CityMetricsManager : MonoBehaviour
{

    // Global city metrics
    public int startingBudget = 1000000;
    public float startingTemp = 67.0f;
    private float lowTemp;
    private float highTemp;
    public float tempSensitivity = 0.05f; // Sensitivity factor for how much extra heat affects energy and emissions...
    public float cityTemperature { get; private set; }
    public float population { get; private set; }
    public float happiness { get; private set; }
    public float budget { get; private set; }
    public float urbanHeat { get; private set; }
    public float pollution { get; private set; }
    public float energy { get; private set; }
    public float carbonEmission { get; private set; }
    public Dictionary<MetricTitle, List<MetricData>> metricsOverTime = new Dictionary<MetricTitle, List<MetricData>>();


    // Time-keeping variables
    public int currentMonth = 1;
    public int currentYear = 2024;
    public float monthDuration = 30.0f; // Duration of a "month" in seconds 
    private float monthTimer = 0f;
    private MissionManager missionManager;
    public int missionMonthsRemaining = 0;
    public event Action<int, int, int> OnTimeUpdated; // (month, year, monthsRemaining)
    public event Action OnMetricsUpdate;

    // Metric Setter and Getters
    private Dictionary<MetricTitle, float> metrics;
    public Dictionary<string, (float min, float max)> propertyRanges;

    public float revenue { get; private set; }
    public CameraController cameraController;
    public BuildingsMenuNew buildingsMenu;

    public HeatFactModal heatFactModal;
    private CityTemperatureController cityTemperatureController;


    void Start()
    {
        cityTemperature = startingTemp;
        budget = startingBudget;
        cityTemperatureController.startingTemp = startingTemp;


        // Initialize the dictionary for easier access
        UpdateDictionary();

        InitializeMetricsOverTime();
        UpdateCityMetrics();
    }

    public void UpdateDictionary()
    {
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
        missionManager.onMissionStarted += HandleMissionStarted;
        cityTemperatureController.OnTempUpdated += HandleUpdateTemperature;
    }

    void Update()
    {
        HandleDateChange();
    }

    private void InitializeMetricsOverTime()
    {
        metricsOverTime[MetricTitle.CityTemperature] = new List<MetricData>();
        metricsOverTime[MetricTitle.UrbanHeat] = new List<MetricData>();
        // metricsOverTime[MetricTitle.GreenSpace] = new List<MetricData>();
        metricsOverTime[MetricTitle.Budget] = new List<MetricData>();
        metricsOverTime[MetricTitle.Happiness] = new List<MetricData>();
        metricsOverTime[MetricTitle.Pollution] = new List<MetricData>();
        metricsOverTime[MetricTitle.Population] = new List<MetricData>();
        metricsOverTime[MetricTitle.CarbonEmission] = new List<MetricData>();
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
            HandleTemperatureChange();
            monthTimer = 0f;
        }
    }

    public void HandleMissionStarted(Mission mission)
    {
        budget = mission.startingBudget;
        missionMonthsRemaining = missionManager.currentMission.GetMonthsRemaining(currentMonth, currentYear);
        OnTimeUpdated?.Invoke(currentMonth, currentYear, missionMonthsRemaining);
        OnMetricsUpdate?.Invoke();
    }

    public void HandleTemperatureChange()
    {
        // Trigger heat fact if conditions are met
        if (cityTemperature != 0 && missionManager.currentMission != null && missionManager.missionInProgress)
        {
            if (cityTemperature >= startingTemp + 1 || highTemp >= startingTemp + 4)
            {
                // Trigger a heat fact pop-up
                string randomHeatFact = heatFactModal.GetRandomHeatFact();
                heatFactModal.TriggerFact(randomHeatFact);
            }
            else if (cityTemperature <= startingTemp - 1)
            {
                // Trigger a temperature drop fact pop-up
                string randomTempDropFact = heatFactModal.GetRandomTempDropFact();
                heatFactModal.TriggerFact(randomTempDropFact);
            }
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

        const float BaseWeight = 5f;        // Intrinsic weight for low population buildings

        print($"--- UpdateCityMetrics ---  {totalCityBuildings}");

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
                Debug.LogWarning("Building Props is Null for : " + building.name);
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


            // Apply additional feedback if temperature is above or below base
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

            // Apply adjusted metrics
            urbanHeat += (int)adjustedHeatContribution;
            pollution += (int)adjustedPollution;
            energy += (int)adjustedEnergyConsumption;
            carbonEmission += (int)adjustedCarbonFootprint;
        }


        // Calculate overall city happiness
        // Calculate normalization factor (adjust based on city size and population)
        float normalizationFactor = Mathf.Max(1, totalCityBuildings + population); // Prevent division by zero
        urbanHeat = urbanHeat / totalCityBuildings;

        happiness += totalHappinessImpact / normalizationFactor;
        happiness = Mathf.Clamp(happiness, 0f, 100f);

        CleanMetrics();
        UpdateDictionary();
        AddMetricsToHistory();
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

    private void AddMetricsToHistory()
    {
        float monthElapsed = (float)Math.Round(monthTimer / monthDuration, 1) * 10;
        float yearMonth = float.Parse(string.Format("{0}{1:D2}.{2}", currentYear, currentMonth, monthElapsed));


        metricsOverTime[MetricTitle.CityTemperature].Add(new MetricData { Value = cityTemperature, YearMonth = yearMonth });
        metricsOverTime[MetricTitle.UrbanHeat].Add(new MetricData { Value = urbanHeat, YearMonth = yearMonth });
        // metricsOverTime[MetricTitle.GreenSpace].Add(new MetricData { Value = greenSpace, YearMonth = yearMonth });
        metricsOverTime[MetricTitle.Budget].Add(new MetricData { Value = budget, YearMonth = yearMonth });
        metricsOverTime[MetricTitle.Happiness].Add(new MetricData { Value = happiness, YearMonth = yearMonth });
        metricsOverTime[MetricTitle.Pollution].Add(new MetricData { Value = pollution, YearMonth = yearMonth });
        metricsOverTime[MetricTitle.Population].Add(new MetricData { Value = population, YearMonth = yearMonth });
        metricsOverTime[MetricTitle.CarbonEmission].Add(new MetricData { Value = carbonEmission, YearMonth = yearMonth });
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

    public void HandleUpdateTemperature(float cityAvgTemp, float cityLowTemp, float cityHighTemp)
    {
        lowTemp = cityLowTemp;
        highTemp = cityHighTemp;
        cityTemperature = cityAvgTemp;
    }

    void OnDestroy()
    {
        cityTemperatureController.OnTempUpdated -= HandleUpdateTemperature;
        missionManager.onMissionStarted -= HandleMissionStarted;
    }

}




