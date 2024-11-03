using UnityEngine;

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
