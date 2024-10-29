using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GreenCityBuilder.Missions
{
    public class MissionCatalog : MonoBehaviour
    {
        public GameObject missionItemPrefab;
        public Transform missionListParent;
        public TMP_Text currentMissionTitle;

        public GameObject missionOverviewModal;

        private readonly string missionSelectedText = "<b>Current Mission:</b><br>";
        private readonly string missionNotSelectedText = "Select a Mission";
        private Mission selectedMission = null;

        private bool isFirstLoad = true;
        public Button missionCloseBtn;

        private void Awake()
        {
            selectedMission = null;
        }

        private void Start()
        {
            string missionTitle = FindObjectOfType<MissionManager>().currentMission == null ? "" : FindObjectOfType<MissionManager>().currentMission.missionName;
            currentMissionTitle.text = missionTitle != "" ? missionSelectedText + missionTitle : missionNotSelectedText;


            MissionRepository.AllMissions.Sort((a, b) =>
            {
                // If "Free Play" is found, place it at the end
                if (a.missionName.Contains("Free Play")) return 1;
                if (b.missionName.Contains("Free Play")) return -1;

                // Otherwise, sort by difficulty level
                return a.difficulty.CompareTo(b.difficulty);
            });

            foreach (var mission in MissionRepository.AllMissions)
            {
                GameObject missionItem = Instantiate(missionItemPrefab, missionListParent);
                missionItem.GetComponent<MissionItem>().title.text = mission.missionName;
                missionItem.GetComponent<MissionItem>().image.sprite = mission.missionIcon;
                missionItem.GetComponent<MissionItem>().difficulty.text = "Difficulty: " + mission.GetFormattedDifficuly();
                missionItem.GetComponent<Button>().onClick.AddListener(() => OnCatalogItemClick(mission));
            }
        }

        private void OnEnable()
        {
            // print("ON ENABLE | " + )
            missionCloseBtn.gameObject.SetActive(selectedMission != null);
            if (selectedMission == null) return;

            print("MissionCatalog | ON ENABLE");
            foreach (Transform child in missionListParent)
            {
                print(selectedMission.missionName + " vs " + child.GetComponent<MissionItem>().title.text);
                bool isSelected = selectedMission != null && selectedMission.missionName == child.GetComponent<MissionItem>().title.text;
                child.GetComponent<MissionItem>().SetSelected(isSelected);
            }

            missionCloseBtn.gameObject.SetActive(FindObjectOfType<MissionManager>().currentMission != null);
        }

        private void OnCatalogItemClick(Mission mission)
        {
            // open mission overview modal with selected mission data
            missionOverviewModal.SetActive(true);
            selectedMission = mission;

            // Update Modal Text
            missionOverviewModal.GetComponent<MissionOverviewModal>().Title.text = "<b>Mission</b><br>" + mission.missionName;
            missionOverviewModal.GetComponent<MissionOverviewModal>().ObjectiveText.text = "<b>Objective: </b>" + mission.missionObjective;
            missionOverviewModal.GetComponent<MissionOverviewModal>().TimeLimitText.text = "<b>Time Limit: </b>" + mission.GetFormattedTimeLimit();
            missionOverviewModal.GetComponent<MissionOverviewModal>().DifficultyText.text = "<b>Difficulty: </b>" + mission.GetFormattedDifficuly();
            missionOverviewModal.GetComponent<MissionOverviewModal>().MissionBriefText.text = "<b>Mission Brief: </b>" + mission.missionBrief;
            missionOverviewModal.GetComponent<MissionOverviewModal>().KeyMetricsText.text = "<b>Key Metrics: </b>" + mission.missionMetrics;
        }


        public void OnCatalogClose()
        {
            gameObject.SetActive(false);
            missionOverviewModal.SetActive(false);
        }

        public void OnAcceptMission()
        {
            FindObjectOfType<MissionManager>().StartMission(selectedMission);
            currentMissionTitle.text = missionSelectedText + selectedMission.missionName;
            OnCatalogClose();
        }

        public void OnMissionOverviewClose()
        {
            gameObject.SetActive(true);
            missionOverviewModal.SetActive(false);
        }

        private void OnDisable()
        {
            missionOverviewModal.SetActive(false);
        }
    }
}
