using System.Collections.Generic;
using UnityEngine;

public class Mission
{
    public string missionName;
    public string missionObjective;
    public string missionBrief;
    public int startingBudget;
    public MissionObjective[] objectives;
    public int timeLimitInMonths;
    public int startMonth;
    public int startYear;
    public DifficultyLevel difficulty;
    public Sprite missionIcon;

    // List of metric displays
    public List<MetricDisplay> missionMetrics;

    private Dictionary<MetricTitle, Sprite> metricIcons;

    public Mission(Dictionary<MetricTitle, Sprite> icons)
    {
        metricIcons = icons;
        missionMetrics = new List<MetricDisplay>();
    }

    public void AddMetric(MetricTitle metricTitle)
    {
        if (metricIcons.ContainsKey(metricTitle))
        {
            missionMetrics.Add(new MetricDisplay(metricTitle, metricIcons[metricTitle]));
        }
    }


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
    // Method to retrieve icon for a given MetricTitle
    public Sprite GetMetricIcon(MetricTitle metricTitle)
    {
        if (metricIcons != null && metricIcons.ContainsKey(metricTitle))
        {
            return metricIcons[metricTitle];
        }
        else
        {
            Debug.LogWarning($"Icon for {metricTitle} not found.");
            return null; // or a default icon if you have one
        }
    }
}
