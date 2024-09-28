using UnityEngine;
using System.Collections.Generic;

public class HeatMap : MonoBehaviour
{
    private int gridSizeX;
    private int gridSizeZ;
    private float[,] heatValues;
    private Texture2D heatMapTexture;
    public GameObject heatMapPlane;
    private Gradient heatGradient;


    // Initialize the HeatMap by passing the grid size values from Grid.cs
    public void InitializeHeatMap(int gridX, int gridZ, float stepSize)
    {
        gridSizeX = gridX / (int)stepSize;
        gridSizeZ = gridZ / (int)stepSize;

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

        InitializeGradient();
    }

    // Update the heat map with buildings and their contributions
    public void UpdateHeatMap(List<Transform> allBuildings)
    {
        // Reset heat values before recalculating
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int z = 0; z < gridSizeZ; z++)
            {
                heatValues[x, z] = 0f;
            }
        }

        // Calculate heat contributions from buildings
        int rescaleVal = 10;
        foreach (Transform building in allBuildings)
        {
            SimulationStats buildingStats = building.GetComponent<SimulationStats>();

            if (buildingStats != null)
            {
                int gridX = Mathf.RoundToInt(building.position.x / rescaleVal);
                int gridZ = Mathf.RoundToInt(building.position.z / rescaleVal);

                if (gridX >= 0 && gridX < gridSizeX && gridZ >= 0 && gridZ < gridSizeZ)
                {

                    int heatContribution = buildingStats.heatContribution;
                    heatValues[gridX, gridZ] += heatContribution;
                }
            }
        }

        // Generate the texture to represent the heat map
        GenerateHeatMapTexture();
        DisplayHeatMap();
    }


    private void GenerateHeatMapTexture()
    {
        print("GenerateHeatMapTexture || " + "X:" + gridSizeX + "   Z:" + gridSizeZ);
        heatMapTexture = new Texture2D(gridSizeX, gridSizeZ);
        float heatMin = 0f;
        float heatMax = 100f;
        float alphaMin = 0.4f;
        float alphaMax = 0.85f;

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int z = 0; z < gridSizeZ; z++)
            {
                // Normalize the heat value to a range of 0 to 1
                float normalizedHeat = Mathf.InverseLerp(heatMin, heatMax, heatValues[x, z]);
                float normalizedAlpha = NumbersUtils.Remap(heatMin, heatMax + 1f, alphaMin, alphaMax, heatValues[x, z]);

                // Use the gradient to get the color at the normalized heat value
                Color heatColor = heatGradient.Evaluate(normalizedHeat);
                heatColor.a = normalizedHeat == 0f ? 0.1f : normalizedAlpha;

                // Optionally set alpha based on normalized heat
                heatColor.a = 0.55f; // You can adjust this for transparency

                // Set the pixel color in the texture
                heatMapTexture.SetPixel(x, z, heatColor);
            }
        }

        // Apply the changes to the texture
        heatMapTexture.Apply();

        // Assign the texture to the heatmap plane
        heatMapTexture = ApplyBlur(heatMapTexture, 0);
        heatMapPlane.GetComponent<Renderer>().material.mainTexture = heatMapTexture;
    }


    private void InitializeGradient()
    {
        // Create a new Gradient
        heatGradient = new Gradient();

        // Define the color keys and alpha keys
        GradientColorKey[] colorKeys = new GradientColorKey[5];
        // colorKeys[0].color = Color.blue;   // 0% heat
        colorKeys[0].color = Color.cyan;   // 0% heat
        colorKeys[0].time = 0.0f;
        colorKeys[1].color = Color.green;  // 25% heat
        colorKeys[1].time = 0.25f;
        colorKeys[2].color = Color.yellow; // 50% heat
        colorKeys[2].time = 0.5f;
        colorKeys[3].color = new Color(1f, 0.65f, 0f); // Orange (75% heat)
        colorKeys[3].time = 0.75f;
        colorKeys[4].color = Color.red;    // 100% heat
        colorKeys[4].time = 1.0f;

        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
        alphaKeys[0].alpha = 1.0f;
        alphaKeys[0].time = 0.0f;
        alphaKeys[1].alpha = 1.0f;
        alphaKeys[1].time = 1.0f;

        // Set the color and alpha keys to the gradient
        heatGradient.SetKeys(colorKeys, alphaKeys);
    }

    // Display the heat map texture on a plane in the scene
    private void DisplayHeatMap()
    {
        Renderer renderer = heatMapPlane.GetComponent<Renderer>();
        renderer.material.mainTexture = heatMapTexture;
    }

    private Texture2D ApplyBlur(Texture2D sourceTexture, int blurSize = 1)
    {
        Texture2D blurredTexture = new Texture2D(sourceTexture.width, sourceTexture.height);
        if (blurSize == 0) return sourceTexture;
        for (int x = 0; x < sourceTexture.width; x++)
        {
            for (int z = 0; z < sourceTexture.height; z++)
            {
                Color averageColor = GetAverageColor(sourceTexture, x, z, blurSize);
                blurredTexture.SetPixel(x, z, averageColor);
            }
        }

        blurredTexture.Apply();
        return blurredTexture;
    }

    private Color GetAverageColor(Texture2D texture, int x, int z, int blurSize)
    {
        Color sum = Color.clear;
        int count = 0;

        for (int xOffset = -blurSize; xOffset <= blurSize; xOffset++)
        {
            for (int zOffset = -blurSize; zOffset <= blurSize; zOffset++)
            {
                int newX = Mathf.Clamp(x + xOffset, 0, texture.width - 1);
                int newZ = Mathf.Clamp(z + zOffset, 0, texture.height - 1);

                sum += texture.GetPixel(newX, newZ);
                count++;
            }
        }

        return sum / count;
    }
}
