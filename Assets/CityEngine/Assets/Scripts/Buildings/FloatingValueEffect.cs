using UnityEngine;
using TMPro;
using GreenCityBuilder.Missions;
using System.Collections;

public class FloatingValueEffect : MonoBehaviour
{
    public TextMeshPro valueText;
    public SpriteRenderer iconRenderer;
    public float floatSpeed = 2.5f;
    public float lifetime = 8f;
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

        StartCoroutine(DelayedPopup(displayDelay));
    }

    private IEnumerator DelayedPopup(float displayDelay)
    {
        // Hide components during the delay
        valueText?.gameObject.SetActive(false);
        iconRenderer?.gameObject.SetActive(false);

        // Wait for the delay duration
        yield return new WaitForSeconds(displayDelay);

        // Show components after the delay
        valueText?.gameObject.SetActive(true);
        iconRenderer?.gameObject.SetActive(true);

        // Start the movement and lifetime countdown
        elapsedTime = 0f;
    }

    void Update()
    {
        // Only move and count lifetime if the popup is visible
        if (elapsedTime >= 0f)
        {
            transform.Translate(floatDirection * floatSpeed * Time.deltaTime, Space.World);

            // Destroy after lifetime
            elapsedTime += Time.deltaTime;
            if (elapsedTime >= lifetime)
            {
                Destroy(gameObject);
            }
        }
    }
}
