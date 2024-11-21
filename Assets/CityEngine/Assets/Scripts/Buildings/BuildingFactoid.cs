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

    [SerializeField] private Image QRCodeImage;
    [SerializeField] private VerticalLayoutGroup verticalSlider;
    [SerializeField] private List<float> animationDurations = new List<float>(3) { 0.5f, 12.0f, 1.5f };
    [SerializeField]
    private List<(float bottom, float top)> sequence = new List<(float bottom, float top)>
    {
        (-1800f, 0f ), // Start
        (0f, 0f),     // Middle
        (0f, -2000f)  // End
    };

    public string[] pastBuildings = { };

    public bool inProgress = false;

    private void Start()
    {
        InitializePosition();
    }

    private void InitializePosition()
    {
        verticalSlider.padding.top = (int)sequence[0].top;
        verticalSlider.padding.bottom = (int)sequence[0].bottom;
    }

    private IEnumerator AnimateFactoid(System.Action onComplete)
    {
        // Move to visible position
        yield return AnimatePosition(sequence[0], sequence[1], animationDurations[0]);

        // Wait at visible position
        yield return new WaitForSeconds(animationDurations[1]);

        // Move to exit position
        yield return AnimatePosition(sequence[1], sequence[2], animationDurations[2]);

        onComplete?.Invoke();
    }


    private IEnumerator AnimatePosition((float bottom, float top) from, (float bottom, float top) to, float duration)
    {
        print("AnimatePosition");
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            // Interpolate position based on elapsed time
            // target.position = Vector3.Lerp(from, to, elapsedTime / duration);
            float percentComplete = elapsedTime / duration;
            verticalSlider.padding.top = (int)Mathf.Lerp(from.top, to.top, percentComplete);
            verticalSlider.padding.bottom = (int)Mathf.Lerp(from.bottom, to.bottom, percentComplete);
            LayoutRebuilder.ForceRebuildLayoutImmediate(verticalSlider.GetComponent<RectTransform>());

            elapsedTime += Time.deltaTime;
            yield return null;  // Wait for the next frame
        }

        // Ensure the final position is exactly the target position
        verticalSlider.padding.top = (int)to.top;
        verticalSlider.padding.bottom = (int)to.bottom;
        LayoutRebuilder.ForceRebuildLayoutImmediate(verticalSlider.GetComponent<RectTransform>());

    }

    private IEnumerator<WaitForSeconds> AnimateFactoid_OLD(System.Action onComplete)
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
            print($"Duration: {duration}");
            yield return new WaitForSeconds(duration);
        }
        // Trigger the callback at the end of the animation
        onComplete?.Invoke();
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

        if (buildingNameText != null)
        {
            buildingNameText.text = buildingName;
        }

        if (titleText != null)
        {
            titleText.text = title;
        }

        if (QRCodeImage != null)
        {
            // Generate QR Code Image from link
            // QRCode.sprite = image;
            Texture2D qrTexture = QRGenerator.EncodeString(caseStudyLink, Color.black, Color.white);

            // Set the generated texture as the mainTexture on the quad
            // QRCodeImage.GetComponent<Renderer>().material.mainTexture = qrTexture;
            // QRCodeImage. = qrTexture;
            Sprite sprite = Sprite.Create(
                qrTexture,
                new Rect(0, 0, qrTexture.width, qrTexture.height),
                new Vector2(0.5f, 0.5f)
            );
            QRCodeImage.sprite = sprite;
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
        InitializePosition();

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
