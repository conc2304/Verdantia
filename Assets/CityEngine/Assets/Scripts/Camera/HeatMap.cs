using UnityEngine;
using System.Collections.Generic;
using System;
using System.Reflection;

public class HeatMap : MonoBehaviour
{
    private int gridSizeX;
    private int gridSizeZ;
    private float[,] heatValues;
    private Texture2D heatMapTexture;
    public GameObject heatMapPlane;
    private Gradient heatGradient;
    [SerializeField] private List<Color> heatColors = new();


    [Range(0f, 1f)]
    public float heatMapAlpha = 0.55f;
    public bool heatMapInitialized = false;

    private void Start()
    {
        InitializeGradient();

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
                heatValues[x, z] = 0f;  // Initialize all heat values to zero
            }
        }

        if (heatGradient == null) InitializeGradient();
        heatMapInitialized = true;
    }

    private void InitializeGradient()
    {
        // Create a new Gradient
        heatGradient = new Gradient();

        List<Color> gradientPalette = new List<Color>{
            // Color.blue,
            // Color.cyan,
            Color.green,
            Color.yellow,
            new (1f, 0.65f, 0f),  // orange color
            Color.red
        };

        if (heatColors != null && heatColors.Count > 0) gradientPalette = heatColors;


        // Define the color keys and alpha keys
        GradientColorKey[] colorKeys = new GradientColorKey[gradientPalette.Count];
        float timeStep = 1f / (gradientPalette.Count - 1);

        // for (int i = 0; i < gradientPalette.Count - 1; i++)
        for (int i = 0; i < gradientPalette.Count - 1; i++)
        {
            colorKeys[i].color = gradientPalette[i];
            colorKeys[0].time = i * timeStep;
        }


        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
        alphaKeys[0].alpha = 1.0f;
        alphaKeys[0].time = 0.0f;
        alphaKeys[1].alpha = 1.0f;
        alphaKeys[1].time = 1.0f;

        // Set the color and alpha keys to the gradient
        heatGradient.SetKeys(colorKeys, alphaKeys);
    }

    // Update the heat map with buildings and their given metric
    public void UpdateHeatMap(List<Transform> allBuildings, string metricName, int metricMin, int metricMax)
    {
        if (heatValues.Length == 0) return;

        if (metricName == "cityTemperature")
        {
            Debug.LogError("Function for Heat Map not bypassed: " + metricName);
            return;
        }

        // Reset heat values before recalculating
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int z = 0; z < gridSizeZ; z++)
            {
                heatValues[x, z] = 0f;
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
                    int heatmapValue = GetMetricValue(buildingProps, metricName);
                    heatValues[gridX, gridZ] += heatmapValue;
                }
            }
        }

        // Generate the texture to represent the heat map
        GenerateHeatMapTexture(metricMin, metricMax);
        DisplayHeatMap();
    }

    public void RenderCityTemperatureHeatMap(float[,] matrix, int metricMin, int metricMax)
    {
        heatValues = matrix;
        GenerateHeatMapTexture(metricMin, metricMax);
        DisplayHeatMap();
        print("RenderCityTemperatureHeatMap");
    }


    private void GenerateHeatMapTexture(int metricMin, int metricMax)
    {
        if (heatGradient == null) InitializeGradient();
        if (!heatMapInitialized || heatValues.Length == 0 || heatGradient == null) return;

        print("GenerateHeatMapTexture");
        heatMapTexture = new Texture2D(gridSizeX, gridSizeZ);
        float heatMin = metricMin;
        float heatMax = metricMax;
        float alphaMin = 0.4f;
        float alphaMax = 0.85f;

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int z = 0; z < gridSizeZ; z++)
            {
                // Normalize the heat/alpha value to a range of 0 to 1
                float normalizedHeat = Mathf.InverseLerp(heatMin, heatMax, heatValues[x, z]); // BUG
                float normalizedAlpha = NumbersUtils.Remap(heatMin, heatMax + 1f, alphaMin, alphaMax, heatValues[x, z]);

                // print($"Normalized heat : {normalizedHeat}");
                // Use the gradient to get the color at the normalized heat value
                Color heatColor = heatGradient.Evaluate(normalizedHeat);
                // heatColor.a = normalizedHeat == 0f ? 0.1f : normalizedAlpha;

                // Set alpha based on normalized heat
                heatColor.a = heatMapAlpha;

                // Set the pixel color in the texture
                heatMapTexture.SetPixel(x, z, heatColor);
            }
        }

        // Apply the changes to the texture
        heatMapTexture.Apply();

        // Assign the texture to the heatmap plane
        // heatMapTexture = ApplyBlur(heatMapTexture, 0);
        heatMapPlane.GetComponent<Renderer>().material.mainTexture = heatMapTexture;
    }



    // Display the heat map texture on a plane in the scene
    private void DisplayHeatMap()
    {
        Renderer renderer = heatMapPlane.GetComponent<Renderer>();
        renderer.material.mainTexture = heatMapTexture;
    }

    // Helper method to dynamically get the metric value using reflection
    public int GetMetricValue(BuildingProperties buildingProps, string metricName)
    {
        // Try to find the field with the given name
        FieldInfo field = buildingProps.GetType().GetField(metricName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (field != null)
        {
            return (int)field.GetValue(buildingProps);
        }

        // Try to find the property with the given name
        PropertyInfo property = buildingProps.GetType().GetProperty(metricName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (property != null && property.CanRead)
        {
            return (int)property.GetValue(buildingProps);
        }

        // If the field or property is not found, throw an exception (or handle the error)
        throw new System.Exception("No field or property found with the name: " + metricName);
    }
}
