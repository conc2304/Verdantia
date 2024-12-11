using UnityEngine;
using TMPro;

/**
Updates a temperature display in the UI to show the city's average, low, and high temperatures in real-time. 
It listens for temperature updates from the CityTemperatureController and updates the corresponding UI elements. 
The script can optionally display a temperature unit and ensures proper cleanup by unregistering from the event when destroyed.
**/
public class TempDisplay : MonoBehaviour
{
    public CityTemperatureController cityTempController;
    public TMP_Text tempText;
    public TMP_Text tempLowText;
    public TMP_Text tempHighText;
    public bool displayUnit = false;

    private void Awake()
    {
        cityTempController.OnTempUpdated += UpdateTempText;
    }

    // Update the UI text when the month/year changes
    void UpdateTempText(float cityTempAvg, float cityTempLow, float cityTempHigh)
    {
        MetricTitle metricTitle = MetricTitle.CityTemperature;
        string unit = MetricUnits.GetUnit(metricTitle);

        tempText.text = $"{cityTempAvg}{(displayUnit ? unit : "")}";

        if (tempLowText != null) tempLowText.text = $"{cityTempLow}°";
        if (tempHighText != null) tempHighText.text = $"{cityTempHigh}°";
    }

    void OnDestroy()
    {
        cityTempController.OnTempUpdated -= UpdateTempText;
    }
}
