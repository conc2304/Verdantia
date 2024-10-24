using UnityEngine;
using TMPro;

public class TempDisplay : MonoBehaviour
{
    public CityMetricsManager cityMetricsManager;
    public TMP_Text tempText;

    void Start()
    {
        // Initialize the time text with the current time
        tempText.text = cityMetricsManager.cityTemp.ToString();
    }

    private void Awake()
    {
        // Subscribe to the OnTempUpdated event
        cityMetricsManager.OnTempUpdated += UpdateTempText;
    }

    // Update the UI text when the month/year changes
    void UpdateTempText()
    {
        tempText.text = cityMetricsManager.cityTemp.ToString();
    }

    void OnDestroy()
    {
        // Unsubscribe from the event to avoid memory leaks
        cityMetricsManager.OnTempUpdated -= UpdateTempText;
    }
}
