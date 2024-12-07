using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class YAxisRenderer : MonoBehaviour
{
    [System.Serializable]
    public class MetricAxis
    {
        public MetricTitle metric;
        public Color axisColor = Color.white;
        public float minValue = 0f;
        public float maxValue = 100f;
    }

    public RectTransform axisRect;
    public GameObject axisPrefab;
    public int numberOfTicks = 5;
    public float tickWidth = 10;
    public List<MetricAxis> metrics;
    public Font font;

    private List<GameObject> instantiatedAxes = new List<GameObject>();

    public void RenderAxes()
    {
        // Clear previous axes
        foreach (var axis in instantiatedAxes)
        {
            Destroy(axis);
        }
        instantiatedAxes.Clear();

        // Render each metric's axis
        foreach (var metric in metrics)
        {
            RenderAxis(metric);
        }
    }

    private void RenderAxis(MetricAxis metric)
    {
        // Instantiate the axis prefab
        var axisGO = Instantiate(axisPrefab, this.axisRect);
        instantiatedAxes.Add(axisGO);

        RectTransform axisRect = axisGO.GetComponent<RectTransform>();

        // Set the axis to align with the left or right side of the chart and start at the bottom
        axisRect.anchorMin = new Vector2(0f, 0f); // Bottom-left corner of the chart
        axisRect.anchorMax = new Vector2(0f, 1f); // Top-left corner of the chart
        axisRect.pivot = new Vector2(0f, 0f);     // Pivot at the bottom-left
        axisRect.anchoredPosition = new Vector2(0f, 0f); // Start at the bottom of the chart
        axisRect.sizeDelta = new Vector2(1f, this.axisRect.rect.height); // Match chart height

        // Set the axis color
        Image axisImage = axisGO.GetComponent<Image>();
        if (axisImage != null) axisImage.color = metric.axisColor;

        // Generate ticks and labels
        for (int i = 0; i <= numberOfTicks; i++)
        {
            float normalizedValue = i / (float)numberOfTicks;
            float yPosition = normalizedValue * this.axisRect.rect.height;

            // Create tick mark
            GameObject tickGO = new GameObject("Tick", typeof(RectTransform), typeof(Image));
            tickGO.transform.SetParent(axisGO.transform, false);

            RectTransform tickRect = tickGO.GetComponent<RectTransform>();
            tickRect.anchorMin = new Vector2(0f, 0f); // Left edge of the axis
            tickRect.anchorMax = new Vector2(0f, 0f); // Left edge of the axis
            tickRect.pivot = new Vector2(0f, 0f);     // Pivot at the bottom-left corner
            tickRect.sizeDelta = new Vector2(tickWidth, 2f);  // Tick size
            tickRect.anchoredPosition = new Vector2(0f, yPosition); // Position relative to the axis

            Image tickImage = tickGO.GetComponent<Image>();
            tickImage.color = metric.axisColor;

            // Create label
            GameObject labelGO = new GameObject("Label", typeof(RectTransform), typeof(Text));
            labelGO.transform.SetParent(axisGO.transform, false);

            RectTransform labelRect = labelGO.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0.5f, 0f); // Center the label horizontally
            labelRect.anchorMax = new Vector2(0.5f, 0f); // Center the label horizontally
            labelRect.pivot = new Vector2(0.5f, 0f);     // Pivot at the bottom
            labelRect.sizeDelta = new Vector2(50f, 20f); // Label size
            labelRect.anchoredPosition = new Vector2(-30f, yPosition); // Position relative to the tick
            Text labelText = labelGO.GetComponent<Text>();

            string unit = MetricUnits.GetUnit(metric.metric);
            MetricUnits.UnitPosition position = MetricUnits.GetUnitPosition(metric.metric);
            string tickValue = NumbersUtils.NumberToAbrev(Mathf.Lerp(metric.minValue, metric.maxValue, normalizedValue));
            if (position == MetricUnits.UnitPosition.Before)
            {
                labelText.text = $"{unit}{tickValue}";
            }
            else
            {
                labelText.text = $"{tickValue}{unit}";
            }

            labelText.alignment = TextAnchor.MiddleRight;
            labelText.color = metric.axisColor;
            labelText.font = font;
            labelText.fontSize = 14;
        }
    }

}
