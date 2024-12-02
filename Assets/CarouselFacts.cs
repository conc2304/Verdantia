using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CarouselFacts : MonoBehaviour
{
    [Header("Facts Carousel Settings")]
    public RectTransform modalRect;
    [Tooltip("List of facts to display in the carousel.")]
    public List<string> facts;

    [Tooltip("Text component to display the facts.")]
    public TMP_Text factText;

    [Tooltip("Time (in seconds) for the modal to slide in.")]
    public float slideInTime = 1.0f;

    [Tooltip("Time (in seconds) to pause the modal in place.")]
    public float pauseTime = 2.0f;

    [Tooltip("Time (in seconds) for the modal to slide out.")]
    public float slideOutTime = 1.0f;

    [Tooltip("Offset for sliding in and out (in local space).")]
    public float slideOffset = 500.0f;

    private int currentFactIndex = 0;

    void Start()
    {
        if (factText == null)
        {
            Debug.LogError("FactText is not assigned. Assign a UI Text component.");
            return;
        }

        if (facts.Count == 0)
        {
            Debug.LogError("No facts provided for the carousel.");
            return;
        }

        // modalRect = GetComponent<RectTransform>();

        // Start the carousel coroutine
        StartCoroutine(RunCarousel());
    }

    private IEnumerator RunCarousel()
    {
        while (true)
        {
            // Set initial off-screen position for sliding in
            modalRect.anchoredPosition = new Vector2(slideOffset, 0);

            factText.text = facts[currentFactIndex];

            // Slide in
            yield return SlideToPosition(Vector2.zero, slideInTime);

            // Pause
            yield return new WaitForSeconds(pauseTime);

            // Slide out
            yield return SlideToPosition(new Vector2(-slideOffset, 0), slideOutTime);

            yield return new WaitForSeconds(1);

            // Move to the next fact
            currentFactIndex = (currentFactIndex + 1) % facts.Count;
        }
    }

    private IEnumerator SlideToPosition(Vector2 targetPosition, float duration)
    {
        Vector2 startPosition = modalRect.anchoredPosition;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            modalRect.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, t);
            yield return null;
        }

        modalRect.anchoredPosition = targetPosition;
    }
}
