using UnityEngine;

[System.Serializable]
public class MissionObjective
{
    public MetricTitle metricName;
    public float targetValue;
    public ObjectiveType objectiveType; // How the metric should be evaluated
    public float comparisonPercentage = 0;
    public bool allowDecrease;
    public Sprite icon;
    public enum ObjectiveType
    {
        ReduceByPercentage,   // Objective to reduce metric by X%
        IncreaseByPercentage, // Objective to increase metric by X%
        MaintainAbove,        // Keep the metric above a certain value
        MaintainBelow,        // Keep the metric below a certain value
    }

    public bool IsObjectiveMet(CityMetricsManager metrics)
    {
        float currentMetricValue = metrics.GetMetricValue(metricName); // Example method in CityMetricsManager
        switch (objectiveType)
        {
            case ObjectiveType.ReduceByPercentage:
                return currentMetricValue <= targetValue * (1 - comparisonPercentage / 100f);
            case ObjectiveType.IncreaseByPercentage:
                return currentMetricValue >= targetValue * (1 + comparisonPercentage / 100f);
            case ObjectiveType.MaintainAbove:
                return currentMetricValue >= targetValue;
            case ObjectiveType.MaintainBelow:
                return currentMetricValue <= targetValue;
            default:
                return false;
        }
    }
}
