using UnityEngine;

// Defines a data structure to represent a metric's title and its associated description. 
[System.Serializable]
public class MetricModalItem
{
    public MetricTitle title;
    [TextArea]
    public string description;
}
