using UnityEngine;

public class CityMetricTopBar : MonoBehaviour
{
    public CityMetricsManager cityMetricsManager;
    public CityMetricUIItem populationUIItem;
    public CityMetricUIItem happinessUIItem;
    public CityMetricUIItem budgetUIItem;
    public CityMetricUIItem greenSpaceUIItem;
    public CityMetricUIItem urbanHeatUIItem;

    void Start()
    {
        cityMetricsManager.OnMetricsUpdate += UpdateMetrics;
    }
    public void UpdateMetrics()
    {
        populationUIItem.UpdateValue(cityMetricsManager.population.ToString());
        happinessUIItem.UpdateValue(cityMetricsManager.happiness.ToString());
        budgetUIItem.UpdateValue(cityMetricsManager.budget.ToString());
        greenSpaceUIItem.UpdateValue(cityMetricsManager.greenSpace.ToString());
        urbanHeatUIItem.UpdateValue(cityMetricsManager.urbanHeat.ToString());
    }
}
