[System.Serializable]
public class Mission
{
    public string missionName;
    public string missionObjective;
    public string missionBrief;
    public string missionMetrics;
    public int startingBudget;
    public MissionObjective[] objectives;
    public int timeLimitInMonths;
    public int startMonth;
    public int startYear;

    public bool IsWithinTimeLimit(int currentMonth, int currentYear)
    {
        int monthsElapsed = (currentYear - startYear) * 12 + (currentMonth - startMonth);
        return monthsElapsed <= timeLimitInMonths;
    }

    public bool CheckMissionStatus(CityMetricsManager metrics, int currentMonth, int currentYear)
    {
        if (!IsWithinTimeLimit(currentMonth, currentYear)) return false;

        foreach (var objective in objectives)
        {
            if (!objective.IsObjectiveMet(metrics))
            {
                return false;
            }
        }
        return true;
    }
}
