using UnityEngine;

[System.Serializable]
public class MissionObjective
{
    public MetricTitle metricName;
    public float targetValue = float.NegativeInfinity;
    public ObjectiveType objectiveType; // How the metric should be evaluated
    public float comparisonPercentage = 0;
    public bool allowDecrease;
    public Sprite icon;
    public enum ObjectiveType
    {
        ReduceByPercentage,
        IncreaseByPercentage,
        MaintainAbove,
        MaintainBelow,
    }

    public bool IsObjectiveMet(CityMetricsManager metrics)
    {
        float currentMetricValue = metrics.GetMetricValue(metricName);
        return objectiveType switch
        {
            ObjectiveType.ReduceByPercentage => targetValue != float.NegativeInfinity && currentMetricValue <= targetValue * (1 - comparisonPercentage / 100f),
            ObjectiveType.IncreaseByPercentage => targetValue != float.NegativeInfinity && currentMetricValue >= targetValue * (1 + comparisonPercentage / 100f),
            ObjectiveType.MaintainAbove => currentMetricValue >= targetValue,
            ObjectiveType.MaintainBelow => currentMetricValue <= targetValue,
            _ => false,
        };
    }
}
