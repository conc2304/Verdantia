using UnityEngine;

public class MissionManager : MonoBehaviour
{
    public CityMetricsManager cityMetricsManager;

    public Mission currentMission = null;

    private bool missionInProgress = false;
    private SaveDataTrigger saveDataTrigger;
    private CameraController cameraController;

    private void Awake()
    {
        currentMission = null;
        saveDataTrigger = FindObjectOfType<SaveDataTrigger>();
        cameraController = FindObjectOfType<CameraController>();
    }

    private void Start()
    {
        cityMetricsManager.OnTimeUpdated += OnTimeUpdated;

    }

    public void StartMission(Mission mission)
    {
        currentMission = mission;
        // mission.missionBrief = $"Mission Started: {mission.missionName}\n" + mission.missionBrief;
        mission.startMonth = cityMetricsManager.currentMonth;
        mission.startYear = cityMetricsManager.currentYear;
        missionInProgress = true;

        LoadMissionCity(mission);
    }

    public void LoadMissionCity(Mission mission)
    {
        print("Load Mission");
        // Load the mission's starting city

        cameraController.ResetGameField();

        if (mission.missionName == "Free Play")
        {
            // do nothing
        }
        else
        {
            string missionFile = SaveSystem.FormatFileName(mission.missionCityFileName);
            saveDataTrigger.BuildingDataLoad(missionFile);
        }
    }

    private void OnTimeUpdated(int currentMonth, int currentYear)
    {
        if (!missionInProgress) return;

        if (currentMission != null && currentMission.missionName == "Free Play") return;

        // Check if mission objectives are met
        if (currentMission.CheckMissionStatus(cityMetricsManager, currentMonth, currentYear))
        {
            OnMissionSuccess();
        }
        else if (!currentMission.IsWithinTimeLimit(currentMonth, currentYear))
        {
            OnMissionFailure();
        }
    }

    private void OnMissionSuccess()
    {
        missionInProgress = false;
        // missionStatusUI.text = "Mission Completed!";
        print("Mission Success");
    }

    private void OnMissionFailure()
    {
        missionInProgress = false;
        // missionStatusUI.text = "Mission Failed!";

        print("Mission Failed");

    }

    private void OnDestroy()
    {
        if (cityMetricsManager != null)
            cityMetricsManager.OnTimeUpdated -= OnTimeUpdated;
    }
}
