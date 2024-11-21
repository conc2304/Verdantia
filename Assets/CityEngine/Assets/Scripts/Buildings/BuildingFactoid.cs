using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class BuildingFactoid : MonoBehaviour
{
    // UI Elements
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text buildingNameText;

    [SerializeField] private Image QRCode;
    [SerializeField] private VerticalLayoutGroup verticalSlider;
    private readonly List<float> animationDurations = new List<float>(3) { 0.5f, 10.0f, 0.5f };
    private readonly List<(float bottom, float top)> sequence = new List<(float bottom, float top)>
    {
        (-1500f, 0f ), // Start
        (0f, 0f),     // Middle
        (0f, -2000f)  // End
    };

    private string[] pastBuildings = { };

    private bool inProgress = false;

    private void Start()
    {
        // Initialize positions

        verticalSlider.padding.top = (int)sequence[0].top;
        verticalSlider.padding.bottom = (int)sequence[0].bottom;

    }

    private IEnumerator<WaitForSeconds> AnimateFactoid(System.Action onComplete)
    {
        inProgress = true;
        for (int i = 0; i < sequence.Count; i++)
        {
            float duration = animationDurations[i];
            var step = sequence[i];

            // If the duration is > 0, animate the padding
            if (duration > 0)
            {
                float elapsedTime = 0f;
                float initialTop = verticalSlider.padding.top;
                float initialBottom = verticalSlider.padding.bottom;

                float targetTop = step.top;
                float targetBottom = step.bottom;

                while (elapsedTime < duration)
                {
                    elapsedTime += Time.deltaTime;
                    float t = Mathf.Clamp01(elapsedTime / duration);

                    verticalSlider.padding.top = Mathf.RoundToInt(Mathf.Lerp(initialTop, targetTop, t));
                    verticalSlider.padding.bottom = Mathf.RoundToInt(Mathf.Lerp(initialBottom, targetBottom, t));

                    LayoutRebuilder.ForceRebuildLayoutImmediate(verticalSlider.GetComponent<RectTransform>());

                    yield return null;
                }

                verticalSlider.padding.top = Mathf.RoundToInt(targetTop);
                verticalSlider.padding.bottom = Mathf.RoundToInt(targetBottom);

                LayoutRebuilder.ForceRebuildLayoutImmediate(verticalSlider.GetComponent<RectTransform>());
            }
            else
            {
                // If duration is 0, apply the padding immediately
                verticalSlider.padding.top = Mathf.RoundToInt(step.top);
                verticalSlider.padding.bottom = Mathf.RoundToInt(step.bottom);
                LayoutRebuilder.ForceRebuildLayoutImmediate(verticalSlider.GetComponent<RectTransform>());
            }

            // Pause after applying the padding for the specified duration
            yield return new WaitForSeconds(duration);
        }
    }


    // Update the factoid's content
    public void UpdateFactoid(string buildingName, string title, string description, string caseStudyLink)
    {
        Debug.Log("UPDATE FACTOID");
        Debug.Log($"{buildingName}, {title}, {description}, {caseStudyLink}");
        if (inProgress) return;
        if (buildingName.ToLower().Contains("road") && pastBuildings.Contains(buildingName))
        {
            // Limit how often a road triggers a new factoid
            pastBuildings.Append("");
            return;
        };

        pastBuildings.Append(buildingName);

        if (buildingName != null)
        {
            buildingNameText.text = buildingName;
        }

        if (titleText != null)
        {
            titleText.text = title;
        }

        if (caseStudyLink != null)
        {
            // Generate QR Code Image from link
            // QRCode.sprite = image;
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
        verticalSlider.padding.top = Mathf.RoundToInt(sequence[0].top);
        verticalSlider.padding.bottom = Mathf.RoundToInt(sequence[0].bottom);
        LayoutRebuilder.ForceRebuildLayoutImmediate(verticalSlider.GetComponent<RectTransform>());
        inProgress = false;
        const int maxBuffer = 3;
        if (pastBuildings.Length > maxBuffer)
        {
            int startingIndex = pastBuildings.Length - maxBuffer;
            pastBuildings = ArrayUtils.Slice(pastBuildings, startingIndex);
        }

        Debug.Log("Position reset for the next trigger.");
    }
}
