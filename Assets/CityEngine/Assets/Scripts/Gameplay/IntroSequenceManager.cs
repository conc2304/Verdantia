using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IntroSequenceManager : MonoBehaviour
{
    public GameObject mainGameUI;
    public Image visualImage;
    public TMP_Text narrationText;
    public Button nextButton;
    public string continueBtnText = "Continue";
    public Sprite continueBtnIcon;
    public string startBtnText = "Let's Build!";
    public Sprite startBtnIcon;

    [System.Serializable]
    public class SequenceStep
    {
        public Sprite visual;
        [TextArea]
        public string narration;
        public float displayDuration;
    }
    public SequenceStep[] sequenceSteps;


    private int currentStep = 0;


    void Start()
    {
        nextButton.onClick.AddListener(AdvanceSequence);
        InitializeSequence();
        ShowCurrentStep();
    }


    private void InitializeSequence()
    {
        currentStep = 0;
        nextButton.GetComponentInChildren<Text>().color = Color.black;
        nextButton.GetComponentInChildren<Text>().text = continueBtnText;

        nextButton.GetComponentInChildren<Image>().color = Color.black;
        nextButton.GetComponentInChildren<Image>().sprite = continueBtnIcon;

    }

    private void OnEnable()
    {
        InitializeSequence();
    }
    private void OnDisable()
    {
        InitializeSequence();
    }


    private void ShowCurrentStep()
    {
        // Set visual and narration text for the current step

        visualImage.sprite = sequenceSteps[currentStep].visual;
        narrationText.text = sequenceSteps[currentStep].narration;

        // Update button look for last step
        if (currentStep == sequenceSteps.Length - 1)
        {
            nextButton.GetComponentInChildren<Text>().color = Color.white;
            nextButton.GetComponentInChildren<Text>().text = startBtnText;
            nextButton.GetComponentInChildren<Image>().color = Color.white;
            nextButton.GetComponentInChildren<Image>().sprite = startBtnIcon;
        }

        // Check if displayDuration is set for auto-advance
        if (sequenceSteps[currentStep].displayDuration > 0)
        {
            StartCoroutine(AutoAdvance(sequenceSteps[currentStep].displayDuration));
        }
    }

    private IEnumerator AutoAdvance(float delay)
    {
        yield return new WaitForSeconds(delay);
        AdvanceSequence();
    }

    public void AdvanceSequence()
    {
        // Move to the next step if there are more steps
        if (currentStep < sequenceSteps.Length - 1)
        {
            currentStep++;
            ShowCurrentStep();
        }
        else
        {
            EndIntroSequence();
        }
    }

    private void EndIntroSequence()
    {
        // Hide the modal and transition to the game interface
        gameObject.SetActive(false);
        mainGameUI.SetActive(true);
    }
}
