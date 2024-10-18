using UnityEngine;

[System.Serializable] // This makes the class show up in the Unity Editor
public class MetricModalItem
{
    public MetricTitle title;
    [TextArea]
    public string description;
}
