using UnityEngine;
using TMPro;

public class TempDisplay : MonoBehaviour
{
    public CityMetricsManager cityMetricsManager;
    public TMP_Text tempText;
    public TMP_Text tempLowText;
    public TMP_Text tempHighText;

    void Start()
    {
        // Initialize the time text with the current time
        tempText.text = cityMetricsManager.cityTemperature.ToString();
    }

    private void Awake()
    {
        // Subscribe to the OnTempUpdated event
        cityMetricsManager.OnTempUpdated += UpdateTempText;
    }

    // Update the UI text when the month/year changes
    void UpdateTempText()
    {
        tempText.text = cityMetricsManager.cityTemperature.ToString();

        if (tempLowText != null)
        {
            tempLowText.text = cityMetricsManager.cityTempLow.ToString();
        }

        if (tempHighText != null)
        {
            tempHighText.text = cityMetricsManager.cityTempMax.ToString();
        }
    }

    void OnDestroy()
    {
        // Unsubscribe from the event to avoid memory leaks
        cityMetricsManager.OnTempUpdated -= UpdateTempText;
    }
}
