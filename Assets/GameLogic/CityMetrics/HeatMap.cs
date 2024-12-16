using UnityEngine;
using System.Collections.Generic;
using System;
using System.Reflection;

/**
handles the creation and updating of a heatmap overlay in the Unity scene. 
It calculates heatmap data from city metrics (e.g., pollution, temperature) and displays it visually on a grid. 
The heatmap texture dynamically updates based on the selected metric, using building properties and a color gradient for visual representation. 
It also updates a legend to display contextual information about the metric.
**/
public class HeatMapOverlay : MonoBehaviour
{
    private int gridSizeX;
    private int gridSizeZ;
    private Texture2D heatMapTexture;
    public GameObject heatMapPlane;
    private Gradient heatGradient;
    [SerializeField] public List<Color> heatColors = new();


    [Range(0f, 1f)]
    public float heatMapAlpha = 0.55f;
    public bool heatMapInitialized = false;

    public HeatMapLegend heatMapLegend;

    private void Start()
    {
        if (!heatMapLegend) heatMapLegend = FindObjectOfType<HeatMapLegend>();

        heatGradient = HeatMapUtils.InitializeGradient(heatColors);

        Grid grid = FindObjectOfType<Grid>();
        gridSizeX = grid.gridSizeX;
        gridSizeZ = grid.gridSizeZ;

        if (grid != null) InitializeHeatMap(gridSizeX, gridSizeZ, 10);
    }

    public void Update()
    {
        if (!heatMapInitialized)
        {
            Grid grid = FindObjectOfType<Grid>();
            gridSizeX = grid.gridSizeX;
            gridSizeZ = grid.gridSizeZ;
            InitializeHeatMap(gridSizeX, gridSizeZ, 10);
        }
    }


    // Initialize the HeatMap by passing the grid size values from Grid.cs
    public void InitializeHeatMap(int gridX, int gridZ, float stepSize)
    {

        gridSizeX = gridX / (int)stepSize;
        gridSizeZ = gridZ / (int)stepSize;
        if (gridSizeX == 0 || gridSizeZ == 0) return;

        // Move the heat map above the ground on init
        Vector3 currentPosition = heatMapPlane.transform.position;
        float heatMapYPos = 20;
        heatMapPlane.transform.position = new Vector3(currentPosition.x, heatMapYPos, currentPosition.z);

        float[,] heatValues = new float[gridSizeX, gridSizeZ];
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int z = 0; z < gridSizeZ; z++)
            {
                heatValues[x, z] = float.NegativeInfinity;  // Initialize all heat values to negative infinity
            }
        }

        if (heatGradient == null) heatGradient = HeatMapUtils.InitializeGradient(heatColors);
        heatMapInitialized = true;
    }


    // Update the heat map with buildings and their given metric
    public void UpdateHeatMap(List<Transform> allBuildings, string metricName, float metricMin, float metricMax)
    {
        // Reset heat values before recalculating
        float[,] heatValues = ArrayUtils.MatrixFill(gridSizeX, gridSizeZ, float.NegativeInfinity);


        // Calculate heat contributions from buildings
        int rescaleVal = 10; // grid size is 10
        foreach (Transform building in allBuildings)
        {
            BuildingProperties buildingProps = building.GetComponent<BuildingProperties>();

            if (buildingProps != null)
            {
                int gridX = Mathf.RoundToInt(building.position.x / rescaleVal);
                int gridZ = Mathf.RoundToInt(building.position.z / rescaleVal);

                if (gridX >= 0 && gridX < gridSizeX && gridZ >= 0 && gridZ < gridSizeZ)
                {
                    // Use reflection to get the value of the metric dynamically
                    float heatmapValue = GetMetricValue(buildingProps, metricName);
                    heatValues[gridX, gridZ] = heatmapValue;
                }
            }
            else
            {
                Debug.LogWarning($"{building.name} | No building Props");
            }
        }

        BuildingMetric? metricEnum = MetricMapping.GetBuildingMetric(metricName);
        bool invertMetrics = metricEnum.HasValue
            ? MetricMapping.BuildingMetricIsInverted(metricEnum.Value)
            : false;

        // Generate the texture to represent the heat map
        GenerateHeatMapTexture(heatValues, metricMin, metricMax, invertMetrics);

        heatMapLegend.UpdateLabels(metricName, metricMin, metricMax, invertMetrics);
    }

    public void RenderCityTemperatureHeatMap(float[,] matrix, int metricMin, int metricMax)
    {
        GenerateHeatMapTexture(matrix, metricMin, metricMax, false);
        heatMapLegend.UpdateLabels("cityTemperature", metricMin, metricMax, false);
    }


    private void GenerateHeatMapTexture(float[,] heatValues, float metricMin, float metricMax, bool invertValues = false)
    {
        heatGradient ??= HeatMapUtils.InitializeGradient(heatColors);

        if (!heatMapInitialized || heatValues.Length == 0 || heatGradient == null) return;

        heatMapTexture = new Texture2D(gridSizeX, gridSizeZ);

        // Assign the texture to the heatmap plane
        heatMapTexture = HeatMapUtils.GenerateHeatMapTexture(heatValues, heatGradient, heatMapAlpha, metricMin, metricMax, invertValues);
        DisplayHeatMap();
    }



    // Display the heat map texture on a plane in the scene
    private void DisplayHeatMap()
    {
        Renderer renderer = heatMapPlane.GetComponent<Renderer>();
        renderer.material.mainTexture = heatMapTexture;
    }

    // Helper method to dynamically get the metric value using reflection
    public float GetMetricValue(BuildingProperties buildingProps, string metricName)
    {
        // Try to find the field with the given name
        FieldInfo field = buildingProps.GetType().GetField(metricName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (field != null)
        {
            return (float)field.GetValue(buildingProps);
        }

        // Try to find the property with the given name
        PropertyInfo property = buildingProps.GetType().GetProperty(metricName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (property != null && property.CanRead)
        {
            return (float)property.GetValue(buildingProps);
        }

        // If the field or property is not found, throw an exception (or handle the error)
        throw new Exception("No field or property found with the name: " + metricName);
    }
}

