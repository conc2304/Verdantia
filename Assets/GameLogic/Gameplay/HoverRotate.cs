using UnityEngine;

/**
creates a hovering and rotating effect for a GameObject. 
The object moves up and down smoothly based on a sine wave (hover effect) and optionally rotates around its Y-axis.
**/
public class HoverRotate : MonoBehaviour
{
    [Header("Hover Settings")]
    public float hoverHeight = 0.5f;
    public float hoverSpeed = 2f;

    [Header("Rotation Settings")]
    public bool rotate = true;
    public float rotationSpeed = 50f;

    private Vector3 originalPosition;

    void Start()
    {
        // Store the original position of the object
        originalPosition = transform.localPosition;
    }

    void OnEnable()
    {
        originalPosition = transform.localPosition;
    }

    void Update()
    {
        // Apply bobbing effect
        float newY = originalPosition.y + Mathf.Sin(Time.time * hoverSpeed) * hoverHeight;
        transform.localPosition = new Vector3(originalPosition.x, newY, originalPosition.z);

        // Apply rotation if enabled
        if (rotate)
        {
            transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
        }
    }
}
