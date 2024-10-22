using UnityEngine;
using TMPro;

public class TimeDisplay : MonoBehaviour
{
    public CityMetricsManager cityMetricsManager;
    public TMP_Text timeText;

    void Start()
    {
        // Subscribe to the OnTimeUpdated event
        cityMetricsManager.OnTimeUpdated += UpdateTimeText;

        // Initialize the time text with the current time
        timeText.text = GetFormattedDate();
    }

    // Update the UI text when the month/year changes
    void UpdateTimeText(int month, int year)
    {
        timeText.text = GetFormattedDate();
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
