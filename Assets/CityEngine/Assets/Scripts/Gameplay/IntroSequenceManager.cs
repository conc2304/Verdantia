using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class IntroSequenceManager : MonoBehaviour
{
    public GameObject mainGameUI;
    public GameObject missionSelector;
    public VideoPlayer videoPlayer;
    public TMP_Text narrationText;
    public Button nextButton;
    public string continueBtnText = "Continue";
    public Sprite continueBtnIcon;
    public string startBtnText = "Let's Build!";
    public Sprite startBtnIcon;

    [System.Serializable]
    public class SequenceStep
    {
        public VideoClip videoClip;
        [TextArea]
        public string narration;
        public float displayDuration;
    }
    public SequenceStep[] sequenceSteps;


    private int currentStep = 0;
    public bool introCompleted = false;

    void Start()
    {
        nextButton.onClick.AddListener(AdvanceSequence);
        InitializeSequence();
        ShowCurrentStep();
    }


    private void InitializeSequence()
    {
        currentStep = 0;
        nextButton.GetComponentInChildren<TMP_Text>(true).color = Color.black;
        nextButton.GetComponentInChildren<TMP_Text>(true).text = continueBtnText;

        nextButton.transform.Find("Icon").transform.GetComponent<Image>().color = Color.black;
        nextButton.transform.Find("Icon").transform.GetComponent<Image>().sprite = continueBtnIcon;
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

        // Load and play the video clip in the VideoPlayer
        videoPlayer.clip = sequenceSteps[currentStep].videoClip;
        videoPlayer.Play();

        narrationText.text = sequenceSteps[currentStep].narration;

        // Update button look for last step
        if (currentStep == sequenceSteps.Length - 1)
        {
            nextButton.GetComponentInChildren<TMP_Text>().color = Color.white;
            nextButton.GetComponentInChildren<TMP_Text>().text = startBtnText;

            nextButton.transform.Find("Icon").transform.GetComponent<Image>().color = Color.white;
            nextButton.transform.Find("Icon").transform.GetComponent<Image>().sprite = startBtnIcon;
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
        introCompleted = true;

        gameObject.SetActive(false);
        mainGameUI.SetActive(true);
        missionSelector.SetActive(true);
    }
}
