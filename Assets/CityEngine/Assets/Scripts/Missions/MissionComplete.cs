using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;


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
        string msg = status ? mission.successMessage : mission.failedMessage;
        UpdateModal(status, mission.missionName, msg);
        OpenModal();
    }

    public void OpenModal()
    {
        modalGO.SetActive(false);
    }



    public void UpdateModal(bool missionCompleted, string missionName, string completedMessage)
    {
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
            messageText.text = completedMessage;
        }
    }

    private void OnDestroy()
    {
        missionManager.onMissionDone -= HandleMissionComplete;
    }
}
