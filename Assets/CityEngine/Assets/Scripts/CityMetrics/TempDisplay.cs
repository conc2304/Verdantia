using UnityEngine;
using TMPro;

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
