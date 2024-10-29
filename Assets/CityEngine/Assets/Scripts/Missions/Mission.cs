using UnityEngine;


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
    public int difficulty;
    public Sprite missionIcon;


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

    public string GetFormattedTimeLimit()
    {
        string formattedTimeLimit;

        int years = timeLimitInMonths / 12;
        int months = timeLimitInMonths % 12;

        if (years > 0 && months > 0)
        {
            formattedTimeLimit = $"{years} {(years == 1 ? "Year" : "Years")} and {months} {(months == 1 ? "Month" : "Months")}";
        }
        else if (years > 0)
        {
            formattedTimeLimit = $"{years} {(years == 1 ? "Year" : "Years")}";
        }
        else
        {
            formattedTimeLimit = $"{months} {(months == 1 ? "Month" : "Months")}";
        }
        return formattedTimeLimit;
    }

    public string GetFormattedDifficuly()
    {
        return StringsUtils.DifficultyToString(difficulty);
    }
}
