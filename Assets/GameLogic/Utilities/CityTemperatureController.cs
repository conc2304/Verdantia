using System;
using System.Collections.Generic;
using UnityEngine;

/**
Simulates and manages the temperature distribution across a city grid, 
accounting for factors like sun heat, building heat contributions, and carbon emissions. 
It uses a heat diffusion model to update the temperature of each grid cell over time, 
adjusting for heat dissipation and accumulation. 
The class integrates with a heat map overlay to visualize the temperature changes and 
tracks metrics such as the average, high, and low temperatures within the city. 
By using a tri-diagonal matrix algorithm, it efficiently solves for the new temperatures 
at each grid point and updates the temperature distribution with more stability then the Euler Method.
**/
public class CityTemperatureController : MonoBehaviour
{
    // Heat Diffusion Variables
    public float[,] cityTempGrid { get; private set; }
    public float startingTemp = 67;
    public float diffusionRate = 35f;
    public float dissipationRate = 0.9999999f;
    public float dissipationConstant = 0.9999999f;
    public float something = 16000f;
    public float sunHeatBase { get; private set; } = 36f;
    public float heatAddRange = 0.05f;
    public float timeStep = 0.1f;

    public float cityTempAvg, cityTempLow, cityTempHigh;

    public CameraController cameraController;
    public BuildingsMenuNew buildingsMenu;
    private HeatMapOverlay heatMap;

    // City Boarder
    private float cityBoarderMinX = float.MaxValue;
    private float cityBoarderMaxX = float.MinValue;
    private float cityBoarderMinZ = float.MaxValue;
    private float cityBoarderMaxZ = float.MinValue;

    private float totalCarbonAccumulation = 0;

    // City Grid Variables
    public Grid grid;
    private int gridTileSize = 10;
    private int gridSizeX;
    private int gridSizeZ;


    // Heat Map Legend Vars
    public int heatMapTempMin;
    public int heatMapTempMax;
    public int tempScaleRange = 15;

    public float cityTempUpdateRate = 3f;
    private float cityTempTimer = 0f;

    public event Action<float, float, float> OnTempUpdated;
    public event Action<float[,], float, float, float, float> OnTempGridUpdated; //  (cityTempsGrid, cityBoarderMinX, cityBoarderMaxX, cityBoarderMinZ, cityBoarderMaxZ)

    [Header("Heat Map Debug")]
    public bool takeStep = false;
    public bool toggleRestartTemp = false;
    public bool playTemp = true;

    private void Start()
    {
        heatMap = FindObjectOfType<HeatMapOverlay>();


        // we are apply sun heat 2x (once for each tranposition step)
        heatMapTempMin = (int)startingTemp - tempScaleRange;
        heatMapTempMax = (int)startingTemp + tempScaleRange;

        gridTileSize = cameraController.gridSize;

        gridSizeX = Math.Max(grid.gridSizeX / gridTileSize, 100);
        gridSizeZ = Math.Max(grid.gridSizeZ / gridTileSize, 100);


        RestartSimulation();
        StepSimulation();
        UpdateTemperatureMetric();
    }

    private void Update()
    {
        HandleTempUpdate();
    }

    public void HandleTempUpdate()
    {

        if (playTemp)
        {
            toggleRestartTemp = false;
            takeStep = false;
        }

        if (toggleRestartTemp)
        {
            RestartSimulation();
            toggleRestartTemp = false;
            playTemp = false;
            return;
        }

        if (takeStep)
        {
            StepSimulation();
            takeStep = false;
            playTemp = false;
            return;
        }


        cityTempTimer += Time.deltaTime;
        if (playTemp && cityTempTimer >= cityTempUpdateRate)
        {
            StepSimulation();
            cityTempTimer = 0f;
        }
    }


    public void RestartSimulation()
    {
        InitializeGrid();
        heatMap.RenderCityTemperatureHeatMap(cityTempGrid, heatMapTempMin, heatMapTempMax);
    }

    public void StepSimulation()
    {
        // Always update the grid on regular intervals
        cityTempGrid = GetCityTempGrid(cityTempGrid);


        UpdateTemperatureMetric();

        // Only call to render if we have temps, and if heatmap is set to metric
        if (cameraController.heatmapActive && cameraController.heatmapMetric == "cityTemperature")
        {
            RenderTemperature();
        }
    }

    public void UpdateTemperatureMetric()
    {
        var (_cityTempAvg, _cityTempLow, _cityTempHigh) = GetCityTemps(cityTempGrid);

        if (float.IsNaN(_cityTempAvg) || float.IsNaN(_cityTempLow) || float.IsNaN(_cityTempHigh)) return;

        (cityTempAvg, cityTempLow, cityTempHigh) = (_cityTempAvg, _cityTempLow, _cityTempHigh);

        OnTempUpdated?.Invoke(cityTempAvg, cityTempLow, cityTempHigh);
        OnTempGridUpdated?.Invoke(
            cityTempGrid,
            cityBoarderMinX,
            cityBoarderMaxX,
            cityBoarderMinZ,
            cityBoarderMaxZ
        );
    }

    public void RenderTemperature()
    {
        if (cityTempGrid != null && cityTempGrid.Length != 0)
        {
            heatMapTempMin = (int)startingTemp - tempScaleRange;
            heatMapTempMax = (int)startingTemp + tempScaleRange;
            heatMap.RenderCityTemperatureHeatMap(cityTempGrid, heatMapTempMin, heatMapTempMax);
        }
    }


    public void InitializeGrid()
    {
        cityTempGrid = ArrayUtils.MatrixFill(gridSizeX, gridSizeZ, startingTemp);
    }

    public float[,] GetCityTempGrid(float[,] currentTemps)
    {

        if (currentTemps == null || currentTemps.Length == 0)
        {
            currentTemps = ArrayUtils.MatrixFill(gridSizeX, gridSizeZ, startingTemp);
        }

        sunHeatBase = startingTemp / 2;

        float[,] newTemps = new float[gridSizeX, gridSizeZ];
        float[,] heatContributionGrid = CalculateBuildingsHeat();

        for (int t = 1; t <= 2; t++)
        {
            // Run once for each row, and once for each column by transposing the 

            for (int i = 0; i < gridSizeZ; i++)
            {
                // For each column, calculate the new temperatures and assign them to the newTemps matrix
                float[] columnTemps = GetColumnTemperatures(currentTemps, heatContributionGrid, i, gridSizeX, gridSizeZ);
                for (int j = 0; j < gridSizeX; j++)
                {
                    newTemps[j, i] = columnTemps[j];
                }
            }

            heatContributionGrid = ArrayUtils.TransposeMatrix(heatContributionGrid);
            currentTemps = ArrayUtils.TransposeMatrix(newTemps);
        }

        return currentTemps;
    }


    public float[,] CalculateBuildingsHeat()
    {
        bool includeSpaces = true;
        List<Transform> allBuildings = cameraController.GetAllBuildings(includeSpaces);

        float[,] buildingHeatGrid = ArrayUtils.MatrixFill(gridSizeX, gridSizeZ, 0f);  // No contribution by default
        if (allBuildings.Count <= 1)
        {
            Debug.LogWarning("all buildings count is 1 or less");
            return buildingHeatGrid;
        };

        int rescaleVal = gridTileSize;

        cityBoarderMinX = float.MaxValue;
        cityBoarderMaxX = float.MinValue;
        cityBoarderMinZ = float.MaxValue;
        cityBoarderMaxZ = float.MinValue;

        totalCarbonAccumulation = 0;

        // First, determine all occupied positions

        foreach (Transform building in allBuildings)
        {

            BuildingProperties buildingProps = building.GetComponent<BuildingProperties>();
            if (buildingProps == null) continue;


            int posX = Mathf.RoundToInt(building.position.x / rescaleVal);
            int posZ = Mathf.RoundToInt(building.position.z / rescaleVal);

            float adjustedHeatContribution = buildingProps.heatContribution * heatAddRange;

            // Apply heat to the building's grid position
            buildingHeatGrid[posX, posZ] = adjustedHeatContribution;

            // Update boundaries based on the building position
            cityBoarderMinX = Mathf.Min(cityBoarderMinX, posX);
            cityBoarderMaxX = Mathf.Max(cityBoarderMaxX, posX);
            cityBoarderMinZ = Mathf.Min(cityBoarderMinZ, posZ);
            cityBoarderMaxZ = Mathf.Max(cityBoarderMaxZ, posZ);

            // Get total carbon footprint of the city
            if (!building.CompareTag("Space"))
            {
                totalCarbonAccumulation += buildingProps.carbonFootprint;
            }
        }


        return buildingHeatGrid;
    }

    public float[] GetColumnTemperatures(
        float[,] tempsGrid,
        float[,] heatContributionGrid,
        int calculateColumn,
        int gridSizeX = 100, int gridSizeZ = 100)
    {
        if (tempsGrid == null || tempsGrid.Length == 0)
        {
            tempsGrid = ArrayUtils.MatrixFill(gridSizeX, gridSizeZ, startingTemp);
        }

        // Heat Dissipation Rate is affected by the carbon emmissions of the city

        float bMin = startingTemp / heatMapTempMax;
        float bMax = startingTemp / heatMapTempMin;

        float carbonEmissionNormalized = totalCarbonAccumulation / something;

        dissipationRate = dissipationRate * (float)Math.Pow(dissipationConstant, carbonEmissionNormalized);
        dissipationRate = Math.Clamp(dissipationRate, bMin, bMax);

        float A = diffusionRate * timeStep / 2;
        float B = 1 + (2 * A) + (dissipationRate * timeStep / 4);
        float C = 2 - B;

        // 3 vectors | for the lower, upper, and diagonal for the solver
        float[] lower = ArrayUtils.Fill(gridSizeZ - 1, -A);
        lower[gridSizeZ - 2] = 2 * (-A);

        float[] diagonal = ArrayUtils.Fill(gridSizeZ, B);
        float[] upper = ArrayUtils.Fill(gridSizeZ - 1, -A);
        upper[0] = 2 * (-A);

        // Form the right hand side of the system 
        float[] rightSide = new float[gridSizeZ];
        float heatContribution;

        for (int i = 0; i < gridSizeZ; i++)
        {
            // Add heat sources and sinks
            heatContribution = heatContributionGrid[i, calculateColumn] + sunHeatBase;

            rightSide[i] = (heatContribution * timeStep) + (C * tempsGrid[i, calculateColumn]);

            if (calculateColumn == 0)
            {
                rightSide[i] += 2 * A * tempsGrid[i, 1];
            }
            else if (calculateColumn == gridSizeZ - 1)
            {
                rightSide[i] += 2 * A * tempsGrid[i, gridSizeZ - 2];
            }
            else
            {
                rightSide[i] += A * (tempsGrid[i, calculateColumn - 1] + tempsGrid[i, calculateColumn + 1]);
            }
        }

        // return new temperures at column index to calculate
        return TDMA.SolveInPlace(lower, diagonal, upper, rightSide);
    }

    private (float cityTempAvg, float cityTempLow, float cityTempHigh) GetCityTemps(float[,] temps, int padding = 5)
    {
        // Calculate the bounds in grid coordinates based on minX, maxX, minZ, maxZ
        print($"{cityBoarderMinX} {cityBoarderMaxX} {cityBoarderMinZ} {cityBoarderMaxZ}");
        int minGridX = Mathf.Clamp(Mathf.RoundToInt(cityBoarderMinX - padding), 0, gridSizeX - 1);
        int maxGridX = Mathf.Clamp(Mathf.RoundToInt(cityBoarderMaxX + padding), 0, gridSizeX - 1);
        int minGridZ = Mathf.Clamp(Mathf.RoundToInt(cityBoarderMinZ - padding), 0, gridSizeZ - 1);
        int maxGridZ = Mathf.Clamp(Mathf.RoundToInt(cityBoarderMaxZ + padding), 0, gridSizeZ - 1);

        print($"{minGridX} {maxGridX} {minGridZ} {maxGridZ}");

        // Calculate the average temperature within city bounds
        float totalTemperature = 0f;
        int count = 0;

        float cityTempLow = float.PositiveInfinity; ;
        float cityTempHigh = float.NegativeInfinity;

        for (int i = minGridX; i <= maxGridX; i++)
        {
            for (int j = minGridZ; j <= maxGridZ; j++)
            {
                totalTemperature += temps[i, j];
                cityTempLow = Math.Min(cityTempLow, temps[i, j]);
                cityTempHigh = Math.Max(cityTempHigh, temps[i, j]);

                count++;
            }
        }

        float averageTemperature = count > 0 ? totalTemperature / count : startingTemp;
        cityTempAvg = (float)Math.Round(averageTemperature);
        cityTempLow = count > 0 ? (float)Math.Round(cityTempLow) : startingTemp;
        cityTempHigh = count > 0 ? (float)Math.Round(cityTempHigh) : startingTemp;

        return (cityTempAvg, cityTempLow, cityTempHigh);
    }
}
