using UnityEngine;
using TMPro;

public class TempDisplay : MonoBehaviour
{
    public CityTemperatureController cityTempController;
    public TMP_Text tempText;
    public TMP_Text tempLowText;
    public TMP_Text tempHighText;

    private void Awake()
    {
        cityTempController.OnTempUpdated += UpdateTempText;
    }

    // Update the UI text when the month/year changes
    void UpdateTempText(float cityTempAvg, float cityTempLow, float cityTempHigh)
    {
        MetricTitle metricTitle = MetricTitle.CityTemperature;
        string unit = MetricUnits.GetUnit(metricTitle);

        tempText.text = $"{cityTempAvg}{unit}";

        if (tempLowText != null) tempLowText.text = $"L {cityTempLow}°";
        if (tempHighText != null) tempHighText.text = $"H {cityTempHigh}°";

    }

    void OnDestroy()
    {
        cityTempController.OnTempUpdated -= UpdateTempText;
    }
}
