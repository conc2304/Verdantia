using UnityEngine;
using System.Collections.Generic;

public class HeatMap : MonoBehaviour
{
    private int gridSizeX;
    private int gridSizeZ;
    private float[,] heatValues;
    private Texture2D heatMapTexture;
    public GameObject heatMapPlane;

    // Initialize the HeatMap by passing the grid size values from Grid.cs
    public void InitializeHeatMap(int gridX, int gridZ, float stepSize)
    {
        print("InitializeHeatMap x: " + gridX + "  z: " + gridZ);
        gridSizeX = gridX / (int)stepSize;
        gridSizeZ = gridZ / (int)stepSize;

        // Move the heat map above the ground on init
        Vector3 currentPosition = heatMapPlane.transform.position;
        heatMapPlane.transform.position = new Vector3(currentPosition.x, 10, currentPosition.z);


        heatValues = new float[gridSizeX, gridSizeZ];
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int z = 0; z < gridSizeZ; z++)
            {
                heatValues[x, z] = 0f;  // Initialize all heat values to zero
            }
        }
    }

    // Update the heat map with buildings and their contributions
    public void UpdateHeatMap(List<Transform> allBuildings)
    {
        print("UpdateHeatMap");
        print(allBuildings.Count);

        // Reset heat values before recalculating
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int z = 0; z < gridSizeZ; z++)
            {
                heatValues[x, z] = 0f;
            }
        }

        print("heat map reset done, Loop over buildings");

        // Calculate heat contributions from buildings

        foreach (Transform building in allBuildings)
        {
            BuildingProperties buildingProp = building.GetComponent<BuildingProperties>();
            if (buildingProp == null)
            {
                print("--------------NO PROPS: " + building.name);
            }
            if (buildingProp != null)
            {
                print(building.name + " --- " + buildingProp.heatContribution);

                int rescaleVal = 10;
                int gridX = Mathf.RoundToInt(building.position.x / rescaleVal);
                int gridZ = Mathf.RoundToInt(building.position.z / rescaleVal);

                print("X: " + gridX + "  Z: " + gridZ);

                if (gridX >= 0 && gridX < gridSizeX && gridZ >= 0 && gridZ < gridSizeZ)
                {

                    int heatContribution = buildingProp.heatContribution;
                    print("Apply HeatContribution: " + heatContribution);
                    heatValues[gridX, gridZ] += heatContribution;
                    print(heatValues[gridX, gridZ]);
                }
            }
        }

        // Generate the texture to represent the heat map
        GenerateHeatMapTexture();
        DisplayHeatMap();
    }

    // Create the heat map texture based on heat values
    private void GenerateHeatMapTexture()
    {
        print("GenerateHeatMapTexture || " + "X:" + gridSizeX + "   Z:" + gridSizeZ);
        heatMapTexture = new Texture2D(gridSizeX, gridSizeZ);
        float heatMin = 0f;
        float heatMax = 100f;

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int z = 0; z < gridSizeZ; z++)
            {
                float normalizedHeat = Mathf.InverseLerp(heatMin, heatMax + 1f, heatValues[x, z]);
                float normalizedAlpha = NumbersUtils.Remap(heatMin, heatMax + 1f, 0f, 0.5f, heatValues[x, z]);
                Color heatColor = Color.Lerp(Color.blue, Color.red, normalizedHeat);

                heatColor.a = normalizedAlpha;
                print(normalizedHeat);
                print("heat color: " + heatColor);
                heatMapTexture.SetPixel(x, z, heatColor);
            }
        }

        heatMapTexture.Apply();
    }

    // Display the heat map texture on a plane in the scene
    private void DisplayHeatMap()
    {
        print("DisplayHeatMap");
        Renderer renderer = heatMapPlane.GetComponent<Renderer>();
        renderer.material.mainTexture = heatMapTexture;

        // renderer.material.SetFloat("_Mode", 3); // 3 corresponds to Transparent mode in the Standard shader
    }
}
