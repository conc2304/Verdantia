using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/**
 Renders a multi-axis line chart for visualizing city metrics over time, 
 dynamically updating based on mission objectives and data from the CityMetricsManager. 
 It normalizes and plots data points, adjusts Y-axis scaling for each metric, and color-codes lines for clarity. 
 The chart adapts to include mission-specific metrics while ensuring core metrics like temperature and budget are always displayed. 
 It enables users to track and compare trends in city performance visually and in real time.
**/
public class MultiAxisLineChart : MonoBehaviour
{


    public RectTransform chartArea;
    public RectTransform axisRectLeft;
    public RectTransform axisRectRight;

    public float lineThickness = 2f;
    public CityMetricsManager cityMetricManager;
    public MissionManager missionManager;
    public YAxisRenderer yAxisRenderer;


    private void Start()
    {
        cityMetricManager.OnMetricsUpdate += HandleMetricsUpdate;

        // Pass ChartArea to YAxisRenderer
        if (yAxisRenderer != null)
        {
            yAxisRenderer.axisRect = axisRectLeft;
        }
    }



    public void HandleMetricsUpdate()
    {
        // Get all of the metrics for the mission's metrics

        if (missionManager.currentMission == null) return;

        print("HandleMetricsUpdate");

        Dictionary<MetricTitle, List<MetricData>> metricsOverTime = cityMetricManager.metricsOverTime;

        // Ensure metrics are sorted by YearMonth
        foreach (var key in metricsOverTime.Keys.ToList())
        {
            metricsOverTime[key] = metricsOverTime[key]
                .OrderBy(data => data.YearMonth)
                .ToList();
        }

        List<MetricTitle> currentMetrics = missionManager.currentMission.GetMissionMetrics();
        if (!currentMetrics.Contains(MetricTitle.CityTemperature))
        {
            currentMetrics.Add(MetricTitle.CityTemperature);
        }

        if (!currentMetrics.Contains(MetricTitle.Budget))
        {
            currentMetrics.Add(MetricTitle.Budget);
        }


        var (minYearMonth, maxYearMonth) = GetMinMaxYearMonth(metricsOverTime);
        var metricRanges = GetMinMaxForEachMetric(metricsOverTime);


        // Clear previous datasets
        foreach (Transform child in chartArea)
        {
            Destroy(child.gameObject);
        }

        // Update Y-axis rendering
        if (yAxisRenderer != null)
        {
            yAxisRenderer.metrics = currentMetrics.Select(metric => new YAxisRenderer.MetricAxis
            {
                metric = metric,
                axisColor = MetricMapping.GetMetricColor(metric),
                minValue = metricRanges.ContainsKey(metric) ? metricRanges[metric].MinValue : 0,
                maxValue = metricRanges.ContainsKey(metric) ? metricRanges[metric].MaxValue : 100
            }).ToList();

            yAxisRenderer.RenderAxes();
        }

        // Draw Data set for current mission metrics
        foreach (MetricTitle metricTitle in currentMetrics)
        {
            if (!metricRanges.ContainsKey(metricTitle))
            {
                Debug.LogWarning($"Metric {metricTitle} not found in ranges");
                continue;
            }

            float minY = metricRanges[metricTitle].MinValue;
            float maxY = metricRanges[metricTitle].MaxValue;

            // Create a new GameObject for each metric dataset
            GameObject datasetGO = new GameObject($"{metricTitle}_LineRenderer", typeof(LineChartRenderer));
            datasetGO.AddComponent<CanvasRenderer>();
            datasetGO.transform.SetParent(chartArea, false);

            LineChartRenderer lineChartRenderer = datasetGO.GetComponent<LineChartRenderer>();
            lineChartRenderer.LineColor = MetricMapping.GetMetricColor(metricTitle);
            lineChartRenderer.LineThickness = lineThickness;

            List<Vector2> normalizedPoints = new List<Vector2>();
            foreach (var metricEntry in metricsOverTime)
            {
                // Skip metrics that do not match the desired metricTitle
                if (metricEntry.Key != metricTitle)
                    continue;

                // Iterate through the list of MetricData for this metric
                foreach (var data in metricEntry.Value)
                {
                    float yearMonth = data.YearMonth;
                    float metricValue = data.Value;

                    float normalizedX = (Mathf.InverseLerp(minYearMonth, maxYearMonth, yearMonth) * chartArea.rect.width) - (chartArea.rect.width / 2);
                    float normalizedY = (Mathf.InverseLerp(minY, maxY, metricValue) * chartArea.rect.height) - (chartArea.rect.height / 2);

                    // Add the normalized data point to the dataset
                    if (normalizedX == 0 && normalizedY == 0) continue;
                    normalizedPoints.Add(new Vector2(normalizedX, normalizedY));
                }
            }

            // Draw the dataset
            lineChartRenderer.DataPoints = normalizedPoints;
            lineChartRenderer.SetVerticesDirty(); // Redraw

        }
    }


    public Dictionary<MetricTitle, (float MinValue, float MaxValue)> GetMinMaxForEachMetric(Dictionary<MetricTitle, List<MetricData>> metricsOverTime)
    {
        // Dictionary to store the min and max values for each metric
        var result = new Dictionary<MetricTitle, (float MinValue, float MaxValue)>();

        // Iterate over each metric in the dictionary
        foreach (var metric in metricsOverTime)
        {
            // If the metric has no data, skip it
            if (metric.Value == null || metric.Value.Count == 0)
                continue;

            // Get the min and max values for the current metric
            float minValue = float.MaxValue;
            float maxValue = float.MinValue;

            foreach (var data in metric.Value)
            {
                if (data.Value < minValue)
                {
                    minValue = data.Value;
                }
                if (data.Value > maxValue)
                {
                    maxValue = data.Value;
                }
            }

            // Add the results to the dictionary
            result[metric.Key] = (MinValue: minValue, MaxValue: maxValue);
        }

        return result;
    }


    public (float MinYearMonth, float MaxYearMonth) GetMinMaxYearMonth(Dictionary<MetricTitle, List<MetricData>> metricsOverTime)
    {
        float minYearMonth = float.MaxValue;
        float maxYearMonth = float.MinValue;

        // Iterate over all metrics
        foreach (var metric in metricsOverTime)
        {
            foreach (var data in metric.Value)
            {
                if (data.YearMonth < minYearMonth)
                {
                    minYearMonth = data.YearMonth;
                }
                if (data.YearMonth > maxYearMonth)
                {
                    maxYearMonth = data.YearMonth;
                }
            }
        }

        return (MinYearMonth: minYearMonth, MaxYearMonth: maxYearMonth);
    }


    private void OnDestroy()
    {
        cityMetricManager.OnMetricsUpdate -= HandleMetricsUpdate;
    }

}
