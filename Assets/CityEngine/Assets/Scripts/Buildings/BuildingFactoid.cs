using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic; // Optional, if you are using TextMeshPro

public class BuildingFactoid : MonoBehaviour
{
    // UI Elements
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private Image QRCode;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private VerticalLayoutGroup layoutGroup;
    private readonly List<float> animationDurations = new List<float>(3) { 0.5f, 2.0f, 0.5f };
    private readonly List<(float bottom, float top)> sequence = new List<(float bottom, float top)>
    {
        (-1500f, 0f ), // Start
        (0f, 0f),     // Middle
        (0f, -1800f)  // End
    };

    private void Start()
    {
        // Initialize positions

        layoutGroup.padding.top = (int)sequence[0].top;
        layoutGroup.padding.bottom = (int)sequence[0].bottom;

    }

    private IEnumerator<WaitForSeconds> AnimateFactoid(System.Action onComplete)
    {
        for (int i = 0; i < sequence.Count; i++)
        {
            float duration = animationDurations[i];
            var step = sequence[i];

            // If the duration is > 0, animate the padding
            if (duration > 0)
            {
                float elapsedTime = 0f;
                float initialTop = layoutGroup.padding.top;
                float initialBottom = layoutGroup.padding.bottom;

                float targetTop = step.top;
                float targetBottom = step.bottom;

                while (elapsedTime < duration)
                {
                    elapsedTime += Time.deltaTime;
                    float t = Mathf.Clamp01(elapsedTime / duration);

                    layoutGroup.padding.top = Mathf.RoundToInt(Mathf.Lerp(initialTop, targetTop, t));
                    layoutGroup.padding.bottom = Mathf.RoundToInt(Mathf.Lerp(initialBottom, targetBottom, t));

                    LayoutRebuilder.ForceRebuildLayoutImmediate(layoutGroup.GetComponent<RectTransform>());

                    yield return null;
                }

                layoutGroup.padding.top = Mathf.RoundToInt(targetTop);
                layoutGroup.padding.bottom = Mathf.RoundToInt(targetBottom);

                LayoutRebuilder.ForceRebuildLayoutImmediate(layoutGroup.GetComponent<RectTransform>());
            }
            else
            {
                // If duration is 0, apply the padding immediately
                layoutGroup.padding.top = Mathf.RoundToInt(step.top);
                layoutGroup.padding.bottom = Mathf.RoundToInt(step.bottom);
                LayoutRebuilder.ForceRebuildLayoutImmediate(layoutGroup.GetComponent<RectTransform>());
            }

            // Pause after applying the padding for the specified duration
            yield return new WaitForSeconds(duration);
        }
    }


    // Update the factoid's content
    public void UpdateFactoid(string title, Sprite image, string description)
    {
        if (titleText != null)
        {
            titleText.text = title;
        }

        if (QRCode != null)
        {
            QRCode.sprite = image;
        }

        if (descriptionText != null)
        {
            descriptionText.text = description;
        }

        // Start the animation coroutine
        StartCoroutine(AnimateFactoid(() => ResetPosition()));
    }

    private void ResetPosition()
    {
        layoutGroup.padding.top = Mathf.RoundToInt(sequence[0].top);
        layoutGroup.padding.bottom = Mathf.RoundToInt(sequence[0].bottom);
        LayoutRebuilder.ForceRebuildLayoutImmediate(layoutGroup.GetComponent<RectTransform>());

        Debug.Log("Position reset for the next trigger.");
    }
}
