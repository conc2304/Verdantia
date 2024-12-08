using System;
using System.Collections;
using Unity.VisualScripting;
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
    public Action<Mission> onMissionStarted;
    public GameObject loadingScreen;


    private void Awake()
    {
        currentMission = null;
        saveDataTrigger = FindObjectOfType<SaveDataTrigger>();
        cameraController = FindObjectOfType<CameraController>();
    }

    private void Start()
    {
        cityMetricsManager.OnTimeUpdated += OnTimeUpdated;
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

            Debug.Log($"Unit: {unit}");

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
            print($"objective Target | {objective.metricName}, {objective.targetValue}");
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

        // Reset Targets based on initial metrics (for percent change based objectives)
        cityMetricsManager.UpdateCityMetrics();

        mission = UpdateMissionTargets(mission);
        onMissionStarted?.Invoke(mission);
        loadingScreen.SetActive(false);
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
            cityMetricsManager.OnTimeUpdated -= OnTimeUpdated;
    }
}
