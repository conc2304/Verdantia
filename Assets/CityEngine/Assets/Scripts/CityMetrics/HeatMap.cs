using UnityEngine;
using System.Collections.Generic;
using System;
using System.Reflection;

public class HeatMapOverlay : MonoBehaviour
{
    private int gridSizeX;
    private int gridSizeZ;
    private float[,] heatValues;
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

        heatValues = new float[gridSizeX, gridSizeZ];
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int z = 0; z < gridSizeZ; z++)
            {
                heatValues[x, z] = float.NegativeInfinity;  // Initialize all heat values to zero
            }
        }

        if (heatGradient == null) heatGradient = HeatMapUtils.InitializeGradient(heatColors);
        heatMapInitialized = true;
    }


    // Update the heat map with buildings and their given metric
    public void UpdateHeatMap(List<Transform> allBuildings, string metricName, float metricMin, float metricMax)
    {
        if (heatValues == null || heatValues.Length == 0) return;

        // Reset heat values before recalculating
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int z = 0; z < gridSizeZ; z++)
            {
                heatValues[x, z] = float.NegativeInfinity;
            }
        }

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
                Debug.LogError($"{building.name} | No building Props");
            }
        }

        BuildingMetric? metricEnum = MetricMapping.GetBuildingMetric(metricName);
        bool invertMetrics = metricEnum.HasValue
            ? MetricMapping.BuildingMetricIsInverted(metricEnum.Value)
            : false;


        // Generate the texture to represent the heat map
        GenerateHeatMapTexture(metricMin, metricMax, invertMetrics);

        heatMapLegend.UpdateLabels(metricName, metricMin, metricMax, invertMetrics);
    }

    public void RenderCityTemperatureHeatMap(float[,] matrix, int metricMin, int metricMax)
    {
        heatValues = matrix;
        GenerateHeatMapTexture(metricMin, metricMax, false);

        heatMapLegend.UpdateLabels("cityTemperature", metricMin, metricMax, false);

        print("RenderCityTemperatureHeatMap");
    }


    private void GenerateHeatMapTexture(float metricMin, float metricMax, bool invertValues = false)
    {
        heatGradient ??= HeatMapUtils.InitializeGradient(heatColors);

        if (!heatMapInitialized || heatValues.Length == 0 || heatGradient == null) return;

        print("GenerateHeatMapTexture");
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
        throw new System.Exception("No field or property found with the name: " + metricName);
    }
}

