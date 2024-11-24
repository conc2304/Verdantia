using System;
using UnityEngine;

public class MissionManager : MonoBehaviour
{
    public CityMetricsManager cityMetricsManager;

    public Mission currentMission = null;

    private bool missionInProgress = false;
    private SaveDataTrigger saveDataTrigger;
    private CameraController cameraController;
    public GameObject timeRemainingGO;

    public Action<Mission, bool> onMissionDone;
    public Action onStartOver;


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
        mission.startMonth = cityMetricsManager.currentMonth;
        mission.startYear = cityMetricsManager.currentYear;
        missionInProgress = true;

        timeRemainingGO.SetActive(!IsMissionFreePlay());

        LoadMissionCity(mission);
    }

    public bool IsMissionFreePlay()
    {
        return currentMission != null && currentMission.missionName.ToLower().Contains("free play");
    }

    // Load the mission's starting city
    public void LoadMissionCity(Mission mission)
    {
        cameraController.ResetGameField();

        if (IsMissionFreePlay())
        {
            // do nothing
        }
        else
        {
            string missionFile = SaveSystem.FormatFileName(mission.missionCityFileName);
            saveDataTrigger.BuildingDataLoad(missionFile);
        }
    }

    private void OnTimeUpdated(int currentMonth, int currentYear, int missionMonthsRemaining)
    {
        if (!missionInProgress) return;

        if (currentMission != null && IsMissionFreePlay()) return;

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
        onMissionDone?.Invoke(currentMission, true);
    }

    private void OnMissionFailure()
    {
        missionInProgress = false;
        // missionStatusUI.text = "Mission Failed!";

        print("Mission Failed");
        onMissionDone?.Invoke(currentMission, false);
    }

    public void OnStartOver()
    {
        currentMission = null;
        missionInProgress = false;
        onStartOver?.Invoke(); // TODO add start over logic to various steps/components
    }

    private void OnDestroy()
    {
        if (cityMetricsManager != null)
            cityMetricsManager.OnTimeUpdated -= OnTimeUpdated;
    }
}
