using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/**
Generates and updates a visual heatmap representing city center's temperature data. 
It listens to updates from the CityTemperatureController, processes a temperature grid, 
maps it to a customizable color gradient, and renders the heatmap as a texture on a UI element. 
It dynamically adjusts the heatmap to fit the display area and provides real-time visualization of temperature variations within the city's boundaries.
**/
public class CityTempHeatMap : MonoBehaviour
{
    [SerializeField] private CityTemperatureController cityTemperatureController;

    private Gradient heatGradient;
    [SerializeField] public List<Color> heatColors = new();

    [Range(0f, 1f)]
    public float heatMapAlpha = 1f;
    public RawImage heatMapImage;

    private int textureWidth = 200;
    private int textureHeight = 200;


    private void Start()

    {
        cityTemperatureController.OnTempGridUpdated += HandleTemperatureUpdate;
        UpdateTextureSizeAndGenerateGradient();
    }

    private void HandleTemperatureUpdate(
        float[,] cityTempsGrid,
        float cityBoarderMinX,
        float cityBoarderMaxX,
        float cityBoarderMinZ,
        float cityBoarderMaxZ
    )
    {
        float textureAspectRatio = (float)textureWidth / textureHeight;

        // Determine the indices within the grid corresponding to the city borders
        int minXIndex = Mathf.Clamp(Mathf.RoundToInt(cityBoarderMinX), 0, cityTempsGrid.GetLength(0) - 1);
        int maxXIndex = Mathf.Clamp(Mathf.RoundToInt(cityBoarderMaxX), 0, cityTempsGrid.GetLength(0) - 1);
        int minZIndex = Mathf.Clamp(Mathf.RoundToInt(cityBoarderMinZ), 0, cityTempsGrid.GetLength(1) - 1);
        int maxZIndex = Mathf.Clamp(Mathf.RoundToInt(cityBoarderMaxZ), 0, cityTempsGrid.GetLength(1) - 1);

        // Adjust crop points to fit the texture's aspect ratio
        (minXIndex, maxXIndex, minZIndex, maxZIndex) = ArrayUtils.AdjustCropToAspectRatio(
            minXIndex,
            maxXIndex,
            minZIndex,
            maxZIndex,
            textureAspectRatio,
            cityTempsGrid.GetLength(0),
            cityTempsGrid.GetLength(1)
        );

        float[,] textureGrid = ArrayUtils.CropMatrix(
            cityTempsGrid,
            minXIndex,
            minZIndex,
            maxXIndex - minXIndex + 1,
            maxZIndex - minZIndex + 1
        );


        // Generate and apply the heatmap texture
        float metricMin = Math.Max(cityTemperatureController.cityTempLow, cityTemperatureController.heatMapTempMin);
        float metricMax = Math.Min(cityTemperatureController.cityTempHigh, cityTemperatureController.heatMapTempMax);

        bool invertMetrics = false;
        Texture2D heatTexture = HeatMapUtils.GenerateHeatMapTexture(
            textureGrid,
            heatGradient,
            heatMapAlpha,
            metricMin,
            metricMax,
            invertMetrics
        );

        heatMapImage.texture = heatTexture;
    }



    void OnEnable()
    {
        // Force layout update
        Canvas.ForceUpdateCanvases();
        UpdateTextureSizeAndGenerateGradient();
    }



    void UpdateTextureSizeAndGenerateGradient()
    {
        RectTransform rectTransform = heatMapImage.GetComponent<RectTransform>();
        textureWidth = Math.Max(textureWidth, Mathf.FloorToInt(rectTransform.rect.width));
        textureHeight = Math.Max(textureHeight, Mathf.FloorToInt(rectTransform.rect.height));

        heatGradient = HeatMapUtils.InitializeGradient(heatColors);
    }

    private void OnDestroy()
    {
        cityTemperatureController.OnTempGridUpdated -= HandleTemperatureUpdate;
    }
}
