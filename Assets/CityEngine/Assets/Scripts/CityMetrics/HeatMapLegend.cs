using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using System;

public class HeatMapLegend : MonoBehaviour
{

    public HeatMapOverlay heatMap;
    public RawImage legendImage;
    public GradientDirection direction = GradientDirection.Horizontal;
    public TMP_Text minValueText;
    public TMP_Text midValueText;
    public TMP_Text maxValueText;
    public TMP_Text metricNameText;
    public GameObject parentPanel;

    private int textureWidth = 1520;
    private int textureHeight = 50;
    public enum GradientDirection
    {
        Horizontal,
        Vertical
    }

    void Start()
    {
        UpdateTextureSizeAndGenerateGradient();
    }

    void OnEnable()
    {
        // Force layout update
        Canvas.ForceUpdateCanvases();
        UpdateTextureSizeAndGenerateGradient();
    }

    void UpdateTextureSizeAndGenerateGradient()
    {
        RectTransform rectTransform = legendImage.GetComponent<RectTransform>();
        textureWidth = Math.Max(textureWidth, Mathf.FloorToInt(rectTransform.rect.width));
        textureHeight = Math.Max(textureHeight, Mathf.FloorToInt(rectTransform.rect.height));

        GenerateGradient(); // Call your gradient generation function
    }
    public void GenerateGradient()
    {
        // Create the texture with the derived dimensions
        Texture2D gradientTexture = new Texture2D(textureWidth, textureHeight);

        // Loop through each pixel in the texture
        for (int x = 0; x < textureWidth; x++)
        {
            for (int y = 0; y < textureHeight; y++)
            {
                float t;

                if (direction == GradientDirection.Horizontal)
                {
                    // Calculate normalized position along the horizontal axis
                    t = (float)x / (textureWidth - 1);
                }
                else
                {
                    // Calculate normalized position along the vertical axis
                    t = (float)y / (textureHeight - 1);
                }

                // Interpolate between the gradient colors
                Color interpolatedColor = InterpolateGradient(heatMap.heatColors, t);
                interpolatedColor.a = 1;
                gradientTexture.SetPixel(x, y, interpolatedColor);
            }
        }

        gradientTexture.Apply(); // Apply the changes to the texture

        // Assign the texture to the RawImage
        legendImage.texture = gradientTexture;
    }

    Color InterpolateGradient(List<Color> colors, float t)
    {
        int segmentCount = colors.Count - 1;
        float segmentLength = 1.0f / segmentCount;

        int segmentIndex = Mathf.FloorToInt(t / segmentLength);
        segmentIndex = Mathf.Clamp(segmentIndex, 0, colors.Count - 2);

        float segmentT = (t - segmentIndex * segmentLength) / segmentLength;
        return Color.Lerp(colors[segmentIndex], colors[segmentIndex + 1], segmentT);
    }

    public void UpdateLabels(string metricName, float minValue, float maxValue, bool invertGradient)
    {

        float midValue = (maxValue + minValue) / 2;
        minValueText.text = Math.Round(minValue).ToString();
        midValueText.text = Math.Round(midValue).ToString();
        maxValueText.text = Math.Round(maxValue).ToString();

        metricNameText.text = StringsUtils.ConvertToLabel(metricName);

        legendImage.transform.localScale = new Vector3(invertGradient ? -1 : 1, 1, 1);
    }

    public void SetVisibility(bool visible)
    {
        parentPanel.SetActive(visible);
    }

}
