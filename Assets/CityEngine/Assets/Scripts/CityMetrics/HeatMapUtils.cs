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
                // Normalize the heat/alpha value to a range of 0 to 1
                float normalizedHeat = invertValues ?
                    Mathf.InverseLerp(heatMax, heatMin, dataGrid[x, z]) :
                    Mathf.InverseLerp(heatMin, heatMax, dataGrid[x, z]);

                normalizedHeat = dataGrid[x, z] == float.NegativeInfinity ? 0.5f : normalizedHeat;

                // print($"Normalized heat : {normalizedHeat}");
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

    public static (int minX, int maxX, int minZ, int maxZ) AdjustCropToAspectRatio(
        int minX,
        int maxX,
        int minZ,
        int maxZ,
        float targetAspectRatio,
        int maxGridWidth,
        int maxGridHeight
    )
    {
        // Calculate the current crop width and height
        int currentWidth = maxX - minX + 1;
        int currentHeight = maxZ - minZ + 1;

        // Calculate the current aspect ratio
        float currentAspectRatio = (float)currentWidth / currentHeight;

        // Adjust the crop to match the target aspect ratio
        if (currentAspectRatio < targetAspectRatio)
        {
            // Increase width to match the target aspect ratio
            int newWidth = Mathf.RoundToInt(currentHeight * targetAspectRatio);
            int deltaWidth = newWidth - currentWidth;

            // Expand minX and maxX equally, clamping to grid bounds
            minX = Mathf.Max(0, minX - deltaWidth / 2);
            maxX = Mathf.Min(maxGridWidth - 1, maxX + deltaWidth / 2);
        }
        else if (currentAspectRatio > targetAspectRatio)
        {
            // Increase height to match the target aspect ratio
            int newHeight = Mathf.RoundToInt(currentWidth / targetAspectRatio);
            int deltaHeight = newHeight - currentHeight;

            // Expand minZ and maxZ equally, clamping to grid bounds
            minZ = Mathf.Max(0, minZ - deltaHeight / 2);
            maxZ = Mathf.Min(maxGridHeight - 1, maxZ + deltaHeight / 2);
        }

        // Recalculate dimensions after adjustments
        currentWidth = maxX - minX + 1;
        currentHeight = maxZ - minZ + 1;

        // Ensure the aspect ratio is exact by recalculating if necessary
        float adjustedAspectRatio = (float)currentWidth / currentHeight;

        if (Mathf.Abs(adjustedAspectRatio - targetAspectRatio) > 0.01f)
        {
            // Minor tweaks if floating-point rounding caused slight mismatch
            if (adjustedAspectRatio < targetAspectRatio && maxX < maxGridWidth - 1)
                maxX++;
            else if (adjustedAspectRatio > targetAspectRatio && maxZ < maxGridHeight - 1)
                maxZ++;
        }

        return (minX, maxX, minZ, maxZ);
    }

}
