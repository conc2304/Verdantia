using System;
using System.Collections;
using UnityEngine;


/**
Manages the lifecycle and state of missions within the game and handles starting missions, 
tracking mission progress, checking for success or failure conditions, and resetting the game field when necessary. 
It integrates with city metrics to dynamically adjust mission objectives and listens for metric updates or time progression to evaluate mission outcomes. 
It supports "free play" and mission-specific modes, using coroutines to load mission-specific city data while displaying a loading screen. 
It also includes events for mission start, completion, and reset, ensuring other game systems can respond appropriately to mission changes.
**/
public class MissionManager : MonoBehaviour
{
    public CityMetricsManager cityMetricsManager;

    public Mission currentMission = null;

    public bool missionInProgress = false;
    private SaveDataTrigger saveDataTrigger;
    private CameraController cameraController;
    public GameObject timeRemainingGO;

    public Action<Mission, bool> onMissionDone;
    public Action onStartOver;
    public Action<Mission> onMissionStarted;
    public GameObject loadingScreen;

    public bool updateDefaultCity = false;


    private void Awake()
    {
        currentMission = null;
        saveDataTrigger = FindObjectOfType<SaveDataTrigger>();
        cameraController = FindObjectOfType<CameraController>();
    }

    private void Start()
    {
        cityMetricsManager.OnTimeUpdated += HandleTimeUpdated;
        cityMetricsManager.OnMetricsUpdate += HandleMetricsUpdated;
        loadingScreen.SetActive(false);
    }

    public void StartMission(Mission mission)
    {
        currentMission = mission;
        mission.startMonth = cityMetricsManager.currentMonth;
        mission.startYear = cityMetricsManager.currentYear;
        missionInProgress = true;

        bool isFreePlay = IsMissionFreePlay();
        if (!isFreePlay)
        {
            cityMetricsManager.missionMonthsRemaining = mission.timeLimitInMonths;
        }

        timeRemainingGO.SetActive(!isFreePlay);

        LoadMissionCity(mission);
    }

    public Mission UpdateMissionTargets(Mission mission)
    {
        // Dynamically set the target value for each mission objective based on starting city metrics
        foreach (var objective in mission.objectives)
        {
            float currentMetricValue = cityMetricsManager.GetMetricValue(objective.metricName);
            string unit = MetricUnits.GetUnit(objective.metricName);
            float percentChange = objective.comparisonPercentage;

            //  if value is already a percentage then just increment, else calculate target based on percent change of value

            switch (objective.objectiveType)
            {
                case MissionObjective.ObjectiveType.IncreaseByPercentage:
                    // Baseline for increase
                    objective.targetValue = unit == "%" ?
                        currentMetricValue + objective.comparisonPercentage :
                        currentMetricValue * (1 + percentChange / 100f);
                    break;

                case MissionObjective.ObjectiveType.ReduceByPercentage:
                    objective.targetValue = unit == "%" ?
                        currentMetricValue - objective.comparisonPercentage :
                        currentMetricValue * (1 - percentChange / 100f);
                    break;

                case MissionObjective.ObjectiveType.MaintainAbove:
                case MissionObjective.ObjectiveType.MaintainBelow:
                    // For these, the target value should already be preset in the mission data
                    break;
            }
        }
        return mission;
    }

    public bool IsMissionFreePlay()
    {
        return currentMission != null && currentMission.missionName.ToLower().Contains("free play");
    }


    public void LoadMissionCity(Mission mission)
    {

        StartCoroutine(LoadMissionCityCoroutine(mission));
    }

    private IEnumerator LoadMissionCityCoroutine(Mission mission)
    {
        loadingScreen.SetActive(true);

        // Wait for ResetGameField to finish
        yield return StartCoroutine(cameraController.ResetGameField());


        if (!IsMissionFreePlay())
        {
            string missionFile = SaveSystem.FormatFileName(mission.missionCityFileName);
            saveDataTrigger.BuildingDataLoad(missionFile);
        }
        else if (updateDefaultCity)
        {
            saveDataTrigger.BuildingDataLoad();
        }


        // Reset Targets based on initial metrics (for percent change based objectives)
        cityMetricsManager.UpdateCityMetrics();

        mission = UpdateMissionTargets(mission);
        onMissionStarted?.Invoke(mission);
        loadingScreen.SetActive(false);
    }

    private void HandleTimeUpdated(int currentMonth, int currentYear, int missionMonthsRemaining)
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

    private void HandleMetricsUpdated()
    {
        if (!missionInProgress) return;

        if (currentMission != null && IsMissionFreePlay()) return;

        // Check if mission objectives are met
        if (currentMission.CheckMissionStatus(cityMetricsManager, cityMetricsManager.currentMonth, cityMetricsManager.currentYear))
        {
            OnMissionSuccess();
        }
        else if (!currentMission.IsWithinTimeLimit(cityMetricsManager.currentMonth, cityMetricsManager.currentYear))
        {
            OnMissionFailure();
        }
    }

    private void OnMissionSuccess()
    {
        missionInProgress = false;
        print("Mission Success");
        onMissionDone?.Invoke(currentMission, true);
    }

    private void OnMissionFailure()
    {
        missionInProgress = false;
        print("Mission Failed");
        onMissionDone?.Invoke(currentMission, false);
    }

    // Called by button click handler
    public void OnStartOver()
    {
        currentMission = null;
        missionInProgress = false;
        onStartOver?.Invoke(); // TODO add start over logic to various steps/components
    }

    private void OnDestroy()
    {
        if (cityMetricsManager != null)
        {
            cityMetricsManager.OnTimeUpdated -= HandleTimeUpdated;
            cityMetricsManager.OnMetricsUpdate -= HandleMetricsUpdated;
        }
    }
}
