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
        // Subscribe to the OnTempUpdated event
        cityTempController.OnTempUpdated += UpdateTempText;
    }

    // Update the UI text when the month/year changes
    void UpdateTempText(float cityTempAvg, float cityTempLow, float cityTempHigh)
    {
        tempText.text = cityTempAvg.ToString();

        if (tempLowText != null)
        {
            tempLowText.text = cityTempLow.ToString();
        }

        if (tempHighText != null)
        {
            tempHighText.text = cityTempHigh.ToString();
        }
    }

    void OnDestroy()
    {
        // Unsubscribe from the event to avoid memory leaks
        cityTempController.OnTempUpdated -= UpdateTempText;
    }
}
