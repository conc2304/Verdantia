using UnityEngine;

public class MissionManager : MonoBehaviour
{
    public CityMetricsManager cityMetricsManager;

    public Mission currentMission = null;
    // public TextMeshProUGUI missionBriefUI;
    // public TextMeshProUGUI missionStatusUI;

    private bool missionInProgress = false;

    private void Awake()
    {
        currentMission = null;
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

        //     missionBriefUI.text = mission.missionBrief;
        //     missionStatusUI.text = "Mission in Progress";
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
