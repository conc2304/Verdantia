using System;
using System.Collections.Generic;
using UnityEngine;

public static class HeatMapUtils
{

    public static Texture2D GenerateHeatMapTexture(
        float[,] dataGrid,
        Gradient heatGradient,
        float heatMapAlpha,
        float metricMin,
        float metricMax,
        bool invertValues = false
    )
    {

        Texture2D heatMapTexture = new Texture2D(dataGrid.GetLength(0), dataGrid.GetLength(1));
        float heatMin = metricMin;
        float heatMax = metricMax;

        for (int x = 0; x < dataGrid.GetLength(0); x++)
        {
            for (int z = 0; z < dataGrid.GetLength(1); z++)
            {
                // Normalize the heat/alpha value to a range of 0 to 1 to match gradient range
                float normalizedHeat = invertValues ?
                    Mathf.InverseLerp(heatMax, heatMin, dataGrid[x, z]) :
                    Mathf.InverseLerp(heatMin, heatMax, dataGrid[x, z]);

                normalizedHeat = dataGrid[x, z] == float.NegativeInfinity || float.IsNaN(dataGrid[x, z]) ? 0.5f : normalizedHeat;

                // Use the gradient to get the color at the normalized heat value
                Color heatColor = heatGradient.Evaluate(normalizedHeat);

                heatColor.a = dataGrid[x, z] == float.NegativeInfinity ? 0.3f : heatMapAlpha;

                // Set the pixel color in the texture
                heatMapTexture.SetPixel(x, z, heatColor);
            }
        }

        // Apply the changes to the texture
        heatMapTexture.Apply();

        return heatMapTexture;
    }

    public static Gradient InitializeGradient(List<Color> heatColors)
    {
        // Create a new Gradient
        Gradient heatGradient = new Gradient();

        List<Color> gradientPalette = new List<Color>{
            Color.blue,
            Color.cyan,
            Color.green,
            Color.yellow,
            Color.red
        };

        if (heatColors != null && heatColors.Count > 0) gradientPalette = heatColors;


        // Define the color keys and alpha keys
        GradientColorKey[] colorKeys = new GradientColorKey[gradientPalette.Count];
        float timeStep = 1f / (gradientPalette.Count - 1);

        for (int i = 0; i < gradientPalette.Count; i++)
        {
            colorKeys[i].color = gradientPalette[i];
            colorKeys[i].time = i * timeStep;
        }


        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
        alphaKeys[0].alpha = 1.0f;
        alphaKeys[0].time = 0.0f;
        alphaKeys[1].alpha = 1.0f;
        alphaKeys[1].time = 1.0f;

        // Set the color and alpha keys to the gradient
        heatGradient.SetKeys(colorKeys, alphaKeys);

        return heatGradient;
    }



}
