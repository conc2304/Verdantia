using UnityEngine;

/**
Defines a simple MetricDisplay class used to represent a metric with an associated MetricTitle and a visual Sprite icon.
**/

[System.Serializable]
public class MetricDisplay
{
    public MetricTitle metricTitle;
    public Sprite icon;

    public MetricDisplay(MetricTitle title, Sprite icon)
    {
        this.metricTitle = title;
        this.icon = icon;
    }
}
