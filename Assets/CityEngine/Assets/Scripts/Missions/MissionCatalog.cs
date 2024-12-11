using TMPro;
using UnityEngine;
using UnityEngine.UI;

/**
Manages the UI for displaying and selecting missions in a catalog. 
It populates a list of missions, allows users to view mission details in an overview modal, and facilitates mission selection. 
It updates the current mission title and manages the state of the mission overview and catalog interface.
**/

namespace GreenCityBuilder.Missions
{
    public class MissionCatalog : MonoBehaviour
    {
        public GameObject missionItemPrefab;
        public Transform missionListParent;
        public TMP_Text currentMissionTitle;

        public GameObject missionOverviewModal;
        public GameObject metricItemPrefab;   // Prefab with Image and Text for each metric

        private readonly string missionSelectedText = "<b>Current Mission:</b><br>";
        private readonly string missionNotSelectedText = "Select a Mission";
        private Mission selectedMission = null;

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
            missionCloseBtn.gameObject.SetActive(selectedMission != null);
            if (selectedMission == null) return;

            foreach (Transform child in missionListParent)
            {
                bool isSelected = selectedMission != null && selectedMission.missionName == child.GetComponent<MissionItem>().title.text;
                child.GetComponent<MissionItem>().SetSelected(isSelected);
            }

            missionCloseBtn.gameObject.SetActive(FindObjectOfType<MissionManager>().currentMission != null);
        }

        private void OnCatalogItemClick(Mission mission)
        {
            // Open mission overview modal with selected mission data
            missionOverviewModal.SetActive(true);
            selectedMission = mission;

            // Update Modal Text
            MissionOverviewModal modal = missionOverviewModal.GetComponent<MissionOverviewModal>();
            modal.Title.text = "<b>Mission</b><br>" + mission.missionName;
            modal.ObjectiveText.text = "<b>Objective: </b>" + mission.missionObjective;
            modal.TimeLimitText.text = "<b>Time Limit: </b>" + mission.GetFormattedTimeLimit();
            modal.DifficultyText.text = "<b>Difficulty: </b>" + mission.GetFormattedDifficuly();
            modal.MissionBriefText.text = "<b>Mission Brief: </b>" + mission.missionBrief;

            if (mission.missionMetrics.Count == 0 || mission.missionName.ToLower().Contains("free play")) modal.KeyMetricsText.text = "<b>Key Metrics: </b><br>You decide what is important!";

            // Clear any previous metrics in the container
            foreach (Transform child in modal.KeyMetricsContainer.transform)
            {
                Destroy(child.gameObject);
            }

            // Populate key metrics icons and titles
            foreach (MetricDisplay metric in mission.missionMetrics)
            {
                GameObject metricItem = Instantiate(metricItemPrefab, modal.KeyMetricsContainer.transform);
                metricItem.GetComponentInChildren<Image>().sprite = metric.icon;
                metricItem.GetComponentInChildren<TMP_Text>().text = StringsUtils.ConvertToLabel(metric.metricTitle.ToString());
            }
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
