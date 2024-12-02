using UnityEngine;

public class Pulsate : MonoBehaviour
{
    [Header("Pulsate Settings")]
    [Tooltip("Minimum scale multiplier.")]
    public float minScale = 0.9f;

    [Tooltip("Maximum scale multiplier.")]
    public float maxScale = 1.1f;

    [Tooltip("Speed of pulsation.")]
    public float speed = 2.0f;

    private Vector3 originalScale;

    void Start()
    {
        originalScale = transform.localScale;
    }

    void Update()
    {
        // Calculate the pulsating scale factor using a sin wave
        float scale = Mathf.Lerp(minScale, maxScale, (Mathf.Sin(Time.time * speed) + 1.0f) / 2.0f);

        transform.localScale = originalScale * scale;
    }
}
