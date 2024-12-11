using UnityEngine;
using TMPro;
using GreenCityBuilder.Missions;
using System.Collections;
using System;

/**
manages the visual and functional behavior of "floating value indicators,"
which display changes in city metrics (e.g., happiness, temperature, or revenue) above buildings or other relevant locations in the game.

Key features include:

Visual Feedback: Displays a numeric value and an optional icon that changes color based on whether the value is positive (green) or negative (red).
Dynamic Animation: The indicator floats upward, scales smoothly, and disappears after a set lifetime to create a polished, informative effect.
Zoom Adaptation: The size and movement of the indicator adjust dynamically based on the camera's zoom level, ensuring clarity at different viewing scales.
Customizable Timing: Delayed display and adjustable animation speeds allow indicators to appear at appropriate moments without overwhelming the player.
This enhances the player's experience by providing clear, real-time feedback on their decisions.
**/
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
    private bool isVisible = false;

    [Range(0, 2f)]
    public float delayMultiplier = 0.3f;
    [Range(0, 5f)]
    public float startScale = 0.5f;
    [Range(0, 5f)]
    public float endtScale = 1.5f;
    private CameraController cameraController;
    private SaveDataTrigger sdt;

    private void Start()
    {
        cameraController = FindObjectOfType<CameraController>();
        sdt = FindObjectOfType<SaveDataTrigger>();
        if (sdt == null || !sdt.cityLoadInitialized) Destroy(gameObject); // dont show pop ups on city load
    }

    public void Initialize(string valueString, bool isPositive, MetricTitle? metricTitle, float displayDelay)
    {
        cameraController = FindObjectOfType<CameraController>();

        // the further the zoom, the faster the items move and the larger they are
        float zoomPos = cameraController.toZoom.y;
        float zoomMultiplier = 1 + Mathf.InverseLerp(cameraController.minZoom, cameraController.maxZoom, zoomPos);  // 0 to 1
        zoomMultiplier *= Mathf.Lerp(0.8f, 10, zoomMultiplier);
        startScale *= zoomMultiplier;
        endtScale *= zoomMultiplier;
        floatSpeed *= Mathf.Lerp(0.3f, 5, zoomMultiplier);

        // Update the text
        valueText.text = valueString;
        valueText.color = isPositive ? positiveColor : negativeColor;

        Sprite icon = metricTitle.HasValue && MissionRepository.metricIcons.ContainsKey(metricTitle.Value)
            ? MissionRepository.metricIcons[metricTitle.Value]
            : null;

        // Update the icon if the metric has one
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

        // Initialize size and position so it doesnt jump on update
        transform.Translate(floatDirection * floatSpeed * 0, Space.World);
        transform.localScale = new Vector3(startScale, startScale, startScale);

        // Wait for the delay duration
        StartCoroutine(DelayedPopup(displayDelay));
    }

    private IEnumerator DelayedPopup(float displayDelay)
    {
        // Hide components during the delay
        valueText?.gameObject.SetActive(false);
        iconRenderer?.gameObject.SetActive(false);

        yield return new WaitForSeconds(displayDelay * delayMultiplier);

        // Show components after the delay
        valueText?.gameObject.SetActive(true);
        iconRenderer?.gameObject.SetActive(true);

        // Start the movement and lifetime countdown
        isVisible = true;
        elapsedTime = 0f;
    }

    void Update()
    {
        // Only move and count lifetime if the popup is visible
        if (isVisible && elapsedTime >= 0f)
        {
            float progress = elapsedTime / lifetime;
            float scale = Mathf.Lerp(startScale, endtScale, progress * progress);

            transform.localScale = new Vector3(scale, scale, scale);
            transform.Translate(floatDirection * floatSpeed * Time.deltaTime, Space.World);

            // Destroy after lifetime
            elapsedTime += Time.deltaTime;

        }
        if (elapsedTime >= lifetime)
        {
            Destroy(gameObject);
        }
    }
}
