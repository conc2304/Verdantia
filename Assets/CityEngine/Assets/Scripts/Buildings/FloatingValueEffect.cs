using UnityEngine;
using TMPro;
using GreenCityBuilder.Missions;

public class FloatingValueEffect : MonoBehaviour
{
    public TextMeshPro valueText;
    public SpriteRenderer iconRenderer;
    public float floatSpeed = 1.5f;
    public float lifetime = 3f;
    public Color positiveColor = Color.green;
    public Color negativeColor = Color.red;

    private Vector3 floatDirection = Vector3.up;
    private float elapsedTime = 0f;

    public void Initialize(string valueString, bool isPositive, MetricTitle? metricTitle, float displayDelay)
    {
        // Update the text
        valueText.text = valueString;
        valueText.color = isPositive ? positiveColor : negativeColor;

        Sprite icon = metricTitle.HasValue && MissionRepository.metricIcons.ContainsKey(metricTitle.Value)
            ? MissionRepository.metricIcons[metricTitle.Value]
            : null;

        // Update the icon if the metric has one
        // Update the icon if provided
        if (iconRenderer != null && icon != null)
        {
            iconRenderer.sprite = icon;
            iconRenderer.enabled = true;
            iconRenderer.color = isPositive ? positiveColor : negativeColor;
        }
        else if (iconRenderer != null)
        {
            iconRenderer.enabled = false;
        }
    }

    void Update()
    {
        // Move the text upward
        transform.Translate(floatDirection * floatSpeed * Time.deltaTime);

        // Destroy after lifetime
        elapsedTime += Time.deltaTime;
        if (elapsedTime >= lifetime)
        {
            Destroy(gameObject);
        }
    }
}
