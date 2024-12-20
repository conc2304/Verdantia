using UnityEngine;
using TMPro;

/**
Manages the display of the current date and the remaining mission time in a UI. 
It listens to updates from the CityMetricsManager to refresh the displayed month, year, and months remaining for the mission. 
The script ensures proper cleanup by unsubscribing from events when destroyed.
**/
public class TimeDisplay : MonoBehaviour
{
    public CityMetricsManager cityMetricsManager;
    public TMP_Text timeText;
    public TMP_Text timeRemaningText;

    void Start()
    {
        timeText.text = GetFormattedDate();
    }

    private void Awake()
    {
        cityMetricsManager.OnTimeUpdated += UpdateTimeText;
    }

    // Update the UI text when the month/year changes
    void UpdateTimeText(int month, int year, int missionMonthsRemaining)
    {
        timeText.text = GetFormattedDate();

        if (timeRemaningText != null && missionMonthsRemaining >= 0)
        {
            timeRemaningText.text = $"Months Left: {missionMonthsRemaining}";
        }
    }

    void OnDestroy()
    {
        // Unsubscribe from the event to avoid memory leaks
        cityMetricsManager.OnTimeUpdated -= UpdateTimeText;
    }

    string GetFormattedDate()
    {
        return StringsUtils.GetMonthAbbreviation(cityMetricsManager.currentMonth) + " " + cityMetricsManager.currentYear;
    }
}
