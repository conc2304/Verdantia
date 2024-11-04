using System;
using System.Collections.Generic;
using System.Drawing;
using System.Security.Cryptography.X509Certificates;
using UnityEditor;
using UnityEngine;

public class CityMetricsManager : MonoBehaviour
{

    // public static CityMetricsManager Instance { get; private set; }

    // Global city metrics
    public int startingBudget = 1000000;
    public float startingTemp = 69.0f;

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

    private Dictionary<MetricTitle, float> metrics;


    public CameraController cameraController;
    public BuildingsMenuNew buildingsMenu;

    // Time-keeping variables
    public int currentMonth = 1; // January starts as 1
    public int currentYear = 2024; // Set the starting year
    public float monthDuration = 30.0f; // Duration of a "month" in seconds (for testing purposes)
    private float monthTimer = 0f;

    public event Action<int, int> OnTimeUpdated; // (month, year)
    public event Action OnMetricsUpdate;
    public event Action OnTempUpdated;

    public Grid grid;
    private int gridSizeX;
    private int gridSizeZ;
    private readonly int gridPadding = 2;
    public float[,] temps;
    public float[,] initialTemps;
    private Dictionary<string, (int min, int max)> propertyRanges;


    [Header("Heat Map Debug")]
    public bool takeStep = false;
    public bool toggleRestartTemp = false;
    private int runCount = 0;

    public bool play = false;
    public bool pause = true;

    // NOTE these values are stable
    public float heatDiffusionRate = 0.25f;
    public float heatDissipationRate = 0.999f; // todo remove 

    public float sunHeatBase = 0.06f;

    public float heatAddRange = 0.05f;

    private float minX = float.MaxValue;
    private float maxX = float.MinValue;
    private float minZ = float.MaxValue;
    private float maxZ = float.MinValue;



    void Start()
    {
        propertyRanges = buildingsMenu.GetPropertyRanges();
        cityTemperature = startingTemp;
        budget = startingBudget;
        UpdateCityMetrics();
        OnMetricsUpdate?.Invoke();
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

        gridSizeX = (grid.gridSizeX / 10) + gridPadding;
        gridSizeZ = (grid.gridSizeZ / 10) + gridPadding;

        RestartSimulation();
    }

    public float GetMetricValue(MetricTitle metricName)
    {
        return metrics.ContainsKey(metricName) ? metrics[metricName] : 0f;
    }

    public void InitializeGrid()
    {
        temps = ArrayFill(gridSizeX, gridSizeZ, startingTemp);
    }



    void Update()
    {
        // Accumulate budget monthly
        monthTimer += Time.deltaTime;

        if (monthTimer >= monthDuration)
        {
            // Every month, calculate the city's financial status
            UpdateMonthlybudget();
            AdvanceMonth();
            monthTimer = 0f;
        }

        if (pause)
        {
            cameraController.toggleRestartTemp = false;
            cameraController.playTemp = false;
            play = false;
        }
        if (play)
        {
            cameraController.toggleRestartTemp = true;
            cameraController.playTemp = true;
            pause = false;
        }


        if (toggleRestartTemp)
        {
            RestartSimulation();
            toggleRestartTemp = false;
        }
        if (takeStep)
        {
            cameraController.toggleRestartTemp = true;
            takeStep = false;
        }
    }

    [ContextMenu("Trigger My Function")]
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
        // Reset all metrics before recalculating them
        ResetMetrics();

        float tempDifference = cityTemperature - startingTemp;

        int cityArea = 0;

        foreach (Transform building in cameraController.allBuildings)
        {
            BuildingProperties buildingProps = building.GetComponent<BuildingProperties>();
            cityArea += buildingProps.additionalSpace.Length + 1;

            // Population and economic metrics
            population += buildingProps.capacity;
            // happiness += buildingProps.happinessImpact;
            budget -= buildingProps.operationalCost;
            budget += buildingProps.taxRevenue;
            greenSpace += buildingProps.greenSpaceEffect;

            revenue += buildingProps.taxContribution;
            income += buildingProps.taxRevenue;
            expenses += buildingProps.upkeep;

            // Environmental metrics
            float adjustedHeatContribution = buildingProps.heatContribution;
            float adjustedEnergyConsumption = buildingProps.energyConsumption;
            float adjustedCarbonFootprint = buildingProps.carbonFootprint;
            float adjustedHappiness = buildingProps.happinessImpact;


            // Only apply additional feedback if temperature is above base
            // Update metric value based on temperature difference
            if (tempDifference != 0)
            {
                adjustedEnergyConsumption += adjustedEnergyConsumption * tempDifference * tempSensitivity;
                adjustedCarbonFootprint += adjustedCarbonFootprint * tempDifference * tempSensitivity;
                adjustedHeatContribution += adjustedHeatContribution * tempDifference * tempSensitivity;
                adjustedHappiness += adjustedHappiness * tempDifference * tempSensitivity;
            }

            // Apply adjusted metrics
            happiness += (int)adjustedHappiness;
            urbanHeat += (int)adjustedHeatContribution;
            pollution += buildingProps.pollutionOutput - buildingProps.pollutionReduction;
            energy += (int)(buildingProps.resourceProduction - adjustedEnergyConsumption);
            carbonEmission += (int)adjustedCarbonFootprint;
        }

        // Adjust happiness to be averaged over all buildings
        happiness = cameraController.allBuildings.Count > 0 ? (happiness / cameraController.allBuildings.Count) : 0;
        happiness = (float)Math.Round(happiness);
        greenSpace = cityArea > 0 ? (float)Math.Round(greenSpace / cityArea) : 0;
        print("Happiness: " + happiness);
    }

    // Method to reset all metrics to initial state before recalculation
    private void ResetMetrics()
    {
        population = 0;
        happiness = 0;
        greenSpace = 0;
        urbanHeat = 0;
        pollution = 0;
        energy = 0;
        carbonEmission = 0;
        revenue = 0;
        income = 0;
        expenses = 0;
    }

    // Example method to manually add income to the city
    public void AddRevenue(int amount)
    {
        budget += amount;
        Debug.Log($"Revenue added: {amount}, New budget: {budget}");
    }

    // Example method to manually deduct expenses from the city
    public void DeductExpenses(int amount)
    {
        budget -= amount;
        Debug.Log($"Expenses deducted: {amount}, New budget: {budget}");
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

        List<Transform> allBuildings = cameraController.allBuildings;

        float[,] outputTemps = ArrayFill(gridSizeX, gridSizeZ, 0);

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
        for (int z = 1; z < gridSizeZ - (gridPadding / 2); z++)
        {
            temps[0, z] = temps[1, z];
            temps[gridSizeX - 1, z] = temps[gridSizeX - 2, z];
        }
        for (int x = 0; x < gridSizeX - (gridPadding / 2); x++)
        {
            temps[x, 0] = temps[x, 1];
            temps[x, gridSizeZ - 1] = temps[x, gridSizeZ - 2];
        }
        int minTemp = 50;
        int maxTemp = 100;

        // Build output temps matrix: Heat Diffusion/Dissipation Forumla
        for (int x = 1; x < gridSizeX - (gridPadding / 2); x++)
        {
            for (int z = 1; z < gridSizeZ - (gridPadding / 2); z++)
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

        float[,] newTemps = new float[gridSizeX, gridSizeZ];

        // Boundary Conditions
        for (int z = 1; z < gridSizeZ - (gridPadding / 2); z++)
        {
            // Fix left and right edges to inner cell values, effectively holding temperature steady at the boundaries
            temps[0, z] = temps[1, z];
            temps[gridSizeX - 1, z] = temps[gridSizeX - 2, z];
        }

        for (int x = 1; x < gridSizeX - (gridPadding / 2); x++)
        {
            // Fix top and bottom edges to inner cell values
            temps[x, 0] = temps[x, 1];
            temps[x, gridSizeZ - 1] = temps[x, gridSizeZ - 2];
        }


        float tempMin = 50;

        for (int i = 1; i < gridSizeX - 1; i++)
        {
            for (int j = 1; j < gridSizeZ - 1; j++)
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

    public float[,] AddHeat(float[,] tempsGrid)
    {
        int rescaleVal = 10; // grid size is 10

        minX = float.MaxValue;
        maxX = float.MinValue;
        minZ = float.MaxValue;
        maxZ = float.MinValue;

        foreach (Transform building in cameraController.allBuildings)
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
            tempsGrid[gridX, gridZ] += adjustedHeatContribution;

            // Update min and max boundaries based on the building position
            minX = Mathf.Min(minX, gridX);
            maxX = Mathf.Max(maxX, gridX);
            minZ = Mathf.Min(minZ, gridZ);
            maxZ = Mathf.Max(maxZ, gridZ);

            // Check any additional spaces associated with the building
            foreach (Transform additionalSpace in buildingProps.additionalSpace)
            {
                gridX = Mathf.RoundToInt(additionalSpace.position.x / rescaleVal);
                gridZ = Mathf.RoundToInt(additionalSpace.position.z / rescaleVal);
                tempsGrid[gridX, gridZ] += adjustedHeatContribution;

                // Update boundaries for additional space positions
                minX = Mathf.Min(minX, gridX);
                maxX = Mathf.Max(maxX, gridX);
                minZ = Mathf.Min(minZ, gridZ);
                maxZ = Mathf.Max(maxZ, gridZ);
            }
        }

        return tempsGrid;
    }

    private float GetAvgTemp(float[,] temps, int padding = 5)
    {
        // Calculate the bounds in grid coordinates based on minX, maxX, minZ, maxZ
        int minGridX = Mathf.Clamp(Mathf.RoundToInt(minX - padding), 0, gridSizeX - 1);
        int maxGridX = Mathf.Clamp(Mathf.RoundToInt(maxX + padding), 0, gridSizeX - 1);
        int minGridZ = Mathf.Clamp(Mathf.RoundToInt(minZ - padding), 0, gridSizeZ - 1);
        int maxGridZ = Mathf.Clamp(Mathf.RoundToInt(maxZ + padding), 0, gridSizeZ - 1);

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

        return (float)Math.Round(averageTemperature);
    }
}

// 0.25
// 0.999
// 0.06
// 0.05

