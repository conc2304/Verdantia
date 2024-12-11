using UnityEngine;
using UnityEngine.UI;
using TMPro;

/**
Handles the UI for displaying mission completion status. 
It listens to the mission manager for mission completion or restart events, 
and updates a modal with the mission's status, name, and message upon completion. 
The class also generates QR codes for a "Learn More" link, which are displayed in the modal. 
It ensures proper cleanup by unsubscribing from events on destruction.
**/

public class MissionComplete : MonoBehaviour
{
    public MissionManager missionManager;
    public GameObject modalGO;

    public TMP_Text missionStatusText;
    public TMP_Text missionNameText;
    public TMP_Text messageText;

    public GameObject metricsParent;
    public string learnMoreLink = "https://onetreeplanted.org/blogs/stories/urban-heat-island";
    public Image QRCodeImageLeft;
    public Image QRCodeImageRight;

    public string completedMessage = "Completed";
    public string uncompletedMessage = "Not Completed";


    private void Start()
    {
        modalGO.SetActive(false);
        if (missionManager == null) missionManager = FindObjectOfType<MissionManager>();

        missionManager.onMissionDone += HandleMissionComplete;
        missionManager.onStartOver += HandleStartOver;

        Texture2D qrTexture = QRGenerator.EncodeString(learnMoreLink, Color.black, Color.white);
        Sprite sprite = Sprite.Create(
            qrTexture,
            new Rect(0, 0, qrTexture.width, qrTexture.height),
            new Vector2(0.5f, 0.5f)
        );

        if (QRCodeImageLeft != null)
        {
            QRCodeImageLeft.sprite = sprite;
        }

        if (QRCodeImageRight != null)
        {
            QRCodeImageRight.sprite = sprite;
        }
    }

    public void HandleMissionComplete(Mission mission, bool status)
    {
        Debug.Log("HandleMissionComplete");

        string msg = status ? mission.successMessage : mission.failedMessage;
        OpenModal();
        UpdateModal(status, mission.missionName, msg);
    }

    public void OpenModal()
    {

        Debug.Log("OpenModal");
        modalGO.SetActive(true);
    }



    public void UpdateModal(bool missionCompleted, string missionName, string message)
    {

        Debug.Log("UpdateModal");

        if (missionStatusText != null)
        {
            missionStatusText.text = missionCompleted ? completedMessage : uncompletedMessage;
        }

        if (missionNameText != null)
        {
            missionNameText.text = missionName;
        }

        if (messageText != null)
        {
            messageText.text = message;
        }
    }

    public void HandleStartOver()
    {
        modalGO.SetActive(false);
    }


    private void OnDestroy()
    {
        missionManager.onMissionDone -= HandleMissionComplete;
        missionManager.onStartOver -= HandleStartOver;
    }


}
