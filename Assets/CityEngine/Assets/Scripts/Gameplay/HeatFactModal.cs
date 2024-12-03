using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
public class HeatFactModal : MonoBehaviour
{
    [Header("Facts Carousel Settings")]
    public RectTransform modalRect;

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

    private bool isShowingFact = false;

    // Trigger a specific fact to display
    public void TriggerFact(string fact)
    {
        if (isShowingFact) return;

        factText.text = fact;
        StartCoroutine(ShowFact());
    }

    // Show the fact with animation
    private IEnumerator ShowFact()
    {
        isShowingFact = true;

        // Set initial off-screen position (above the view)
        modalRect.anchoredPosition = Vector2.zero;

        yield return SlideToPosition(new Vector2(0, slideOffset), slideInTime);

        yield return new WaitForSeconds(pauseTime);

        yield return SlideToPosition(new Vector2(0, -slideOffset), slideOutTime);

        isShowingFact = false;
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

    // Get a random heat fact
    public string GetRandomHeatFact()
    {
        return FactLibrary.TemperatureUpFacts[Random.Range(0, FactLibrary.TemperatureUpFacts.Count)];
    }

    // Get a random temperature drop fact
    public string GetRandomTempDropFact()
    {
        return FactLibrary.TemperatureDownFacts[Random.Range(0, FactLibrary.TemperatureDownFacts.Count)];
    }
}
