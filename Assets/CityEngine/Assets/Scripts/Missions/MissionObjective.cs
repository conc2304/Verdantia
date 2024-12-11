using UnityEngine;

/**
Defines a MissionObjective, representing a specific goal or condition for a mission in the game. 
It associates a metric (like budget or happiness) with a target value 
and specifies how the value should be evaluated (e.g., increase by percentage, maintain above a threshold). 
The IsObjectiveMet method checks whether the objective is achieved based on the current metric value in the CityMetricsManager, 
supporting comparison types like reduce, increase, maintain above, or maintain below. 
The class also includes visual representation through an optional icon.
**/

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
        string unit = MetricUnits.GetUnit(metricName);

        return objectiveType switch
        {
            ObjectiveType.ReduceByPercentage => targetValue != float.NegativeInfinity && currentMetricValue <= targetValue,
            ObjectiveType.IncreaseByPercentage => targetValue != float.NegativeInfinity && currentMetricValue >= targetValue,
            ObjectiveType.MaintainAbove => currentMetricValue >= targetValue,
            ObjectiveType.MaintainBelow => currentMetricValue <= targetValue,
            _ => false,
        };
    }
}
