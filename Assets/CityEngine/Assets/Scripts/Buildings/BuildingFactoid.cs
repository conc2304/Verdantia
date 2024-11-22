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
        inProgress = true;
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


    // Update the factoid's content
    public void UpdateFactoid(string buildingName, string title, string description, string caseStudyLink)
    {

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
            Texture2D qrTexture = QRGenerator.EncodeString(caseStudyLink, Color.black, Color.white);

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
    }
}
