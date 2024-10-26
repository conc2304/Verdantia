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
    // public int startingBudget = 500000;
    public float startingTemp = 69.0f;

    public float cityTemp { get; private set; }
    public float tempSensitivity = 0.05f; // Sensitivity factor for how much extra heat affects energy and emissions
    private string tempSuffix = "°F";
    public int population { get; private set; }
    public float happiness { get; private set; } // Store as float for calculation
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
    private int gridPadding = 2;
    public float[,] temps;
    public float[,] initialTemps;



    public bool takeStep = false;
    public bool toggleRestartTemp = false;
    private int runCount = 0;

    public bool play = false;
    public bool pause = true;

    public float heatDiffusionRate = 0.1f; // todo remove ALPHA
    public float heatDissipationRate = 0.1f; // todo remove 
    public int sizeExtra = 2;

    public float sunHeatBase = 1;
    float[,] sinkSourcesGrid;




    void Start()
    {
        cityTemp = startingTemp;
        budget = startingBudget;
        UpdateCityMetrics();
        OnMetricsUpdate?.Invoke();

        gridSizeX = (grid.gridSizeX / 10) + gridPadding;
        gridSizeZ = (grid.gridSizeZ / 10) + gridPadding;

        RestartSimulation();
    }

    public void InitializeGrid()
    {
        temps = ArrayFill(gridSizeX, gridSizeZ, startingTemp);

        // temps[gridSizeX / 2, gridSizeZ / 2] = 80f; // TODO remove later
        // for (int i = -sizeExtra; i < sizeExtra; i++)
        // {
        //     for (int j = -sizeExtra; j < sizeExtra; j++)
        //     {
        //         temps[gridSizeX / 2 + i, gridSizeZ / 2 + j] = 80f;
        //     }
        // }

    }

    public void InitSinkSourcesToGrid()
    {

        int heatMax = 100;
        int tempSize = 25;

        sinkSourcesGrid = ArrayFill(gridSizeX, gridSizeZ, sunHeatBase);
        for (int i = -sizeExtra; i < sizeExtra; i++)
        {
            for (int j = -sizeExtra; j < sizeExtra; j++)
            {
                sinkSourcesGrid[gridSizeX / 2 + i, gridSizeZ / 2 + j] += heatMax / tempSize;
            }
        }
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
            toggleRestartTemp = false; // Reset the toggle if you want it to trigger only once
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
        runCount = 0;
        InitSinkSourcesToGrid();
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
        OnTimeUpdated?.Invoke(currentMonth, currentYear);
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


        float tempDifference = cityTemp - startingTemp;


        foreach (Transform building in cameraController.allBuildings)
        {
            BuildingProperties buildingProps = building.GetComponent<BuildingProperties>();

            // Population and economic metrics
            population += buildingProps.capacity;
            happiness += buildingProps.happinessImpact;
            budget -= buildingProps.operationalCost;
            budget += buildingProps.taxRevenue;
            greenSpace += buildingProps.greenSpaceEffect;

            // Environmental metrics
            float adjustedHeatContribution = buildingProps.heatContribution;
            float adjustedEnergyConsumption = buildingProps.energyConsumption;
            float adjustedCarbonFootprint = buildingProps.carbonFootprint;

            // Only apply additional feedback if temperature is above base
            // Update metric value based on temperature difference
            if (tempDifference != 0)
            {
                adjustedEnergyConsumption += adjustedEnergyConsumption * tempDifference * tempSensitivity;
                adjustedCarbonFootprint += adjustedCarbonFootprint * tempDifference * tempSensitivity;
                adjustedHeatContribution += adjustedHeatContribution * tempDifference * tempSensitivity;
            }

            // Apply adjusted metrics
            urbanHeat += (int)adjustedHeatContribution;
            pollution += buildingProps.pollutionOutput - buildingProps.pollutionReduction;
            energy += (int)(buildingProps.resourceProduction - adjustedEnergyConsumption);
            carbonEmission += (int)adjustedCarbonFootprint;
            revenue += buildingProps.taxContribution;
            income += buildingProps.taxRevenue;
            expenses += buildingProps.upkeep;
        }

        // Adjust happiness to be averaged over all buildings
        happiness = cameraController.allBuildings.Count > 0 ? (happiness / cameraController.allBuildings.Count) : 0;
        happiness = (float)Math.Truncate((double)happiness * 100 / 100);
    }

    // Method to reset all metrics to initial state before recalculation
    private void ResetMetrics()
    {
        population = 0;
        happiness = 0;
        budget = 0;
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

    private float[,] ApplyBlur(float[,] matrix, int blurSize = 1)
    {
        // Texture2D blurredTexture = new Texture2D(matrix.width, sourceTexture.height);
        int rows = matrix.GetLength(0);    // Number of rows
        int columns = matrix.GetLength(1);  // Number of columns
        float[,] blurredMatrix = new float[rows, columns];

        if (blurSize == 0) return matrix;
        for (int x = 0; x < columns; x++)
        {
            for (int z = 0; z < rows; z++)
            {
                float avgVal = GetAverageNumber(matrix, x, z, blurSize);
                // Color averageColor = GetAverageColor(sourceTexture, x, z, blurSize);
                // blurredTexture.SetPixel(x, z, averageColor);
                blurredMatrix[x, z] = avgVal;
            }
        }

        // blurredTexture.Apply();
        return blurredMatrix;
    }

    private float GetAverageNumber(float[,] matrix, int x, int z, int blurSize)
    {
        float sum = 0f;
        int count = 0;

        for (int xOffset = -blurSize; xOffset <= blurSize; xOffset++)
        {
            for (int zOffset = -blurSize; zOffset <= blurSize; zOffset++)
            {
                int newX = Mathf.Clamp(x + xOffset, 0, matrix.GetLength(0) - 1);
                int newZ = Mathf.Clamp(z + zOffset, 0, matrix.GetLength(1) - 1);

                sum += matrix[newX, newZ];
                count++;
            }
        }

        return sum / count;
    }

    public float[,] GetCityTemperatures()
    {
        print("Run | GetCItyTemp : " + runCount);
        runCount++;

        List<Transform> allBuildings = cameraController.allBuildings;
        Dictionary<string, (int min, int max)> propertyRanges = buildingsMenu.propertyRanges;

        float[,] outputTemps = ArrayFill(gridSizeX, gridSizeZ, 0);

        float epsilon = 1f; // Small constant to prevent division by zero
        float maxCarbonEmission = propertyRanges["carbonFootprint"].max;
        float deltaTime = 1;
        float deltaX = 1;

        // float heatDiffusionRate = (float)(deltaTime / Math.Pow(deltaX, 2));             // Alpha
        // heatDiffusionRate = 0.249999999999f;             // Alpha

        // float heatDissipationRate = 1 / ((carbonEmission / maxCarbonEmission) + epsilon); // Beta

        // float b = 1 -deltaTime * heatDissipationRate - 4 * heatDiffusionRate;          // part of formula for finite differences PDE

        float clampedRate = Math.Clamp((4 * heatDiffusionRate) + heatDissipationRate, 0, 0.98f);
        print($"clampedRate {clampedRate}");
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

        // Build output temps matrix: Heat Diffusion/Dissipation Forumla
        for (int x = 1; x < gridSizeX - (gridPadding / 2); x++)
        {
            for (int z = 1; z < gridSizeZ - (gridPadding / 2); z++)
            {
                outputTemps[x, z] =
                    (bk * temps[x, z]) + sinkSourcesGrid[x, z]
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

        temps = outputTemps;
        return outputTemps;
    }
}
