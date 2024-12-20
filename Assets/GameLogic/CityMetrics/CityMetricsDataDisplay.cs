using System.Collections.Generic;
using UnityEngine;
using TMPro;

/**
Manages the display and interaction of city metric data in a user interface.
ensures that the user interface remains synchronized with the city metrics system, providing players with both an overview and detailed insights into the city’s health and progress. 
**/

public class CityMetricsDataDisplay : MonoBehaviour
{
    public CityMetricsManager cityMetricsManager;
    public GameObject modal;
    public TMP_Text modalTitle;
    public TMP_Text modalBodyText;
    public List<MetricModalItem> items = new List<MetricModalItem>();

    public CityMetricUIItem populationUIItem;
    public CityMetricUIItem happinessUIItem;
    public CityMetricUIItem budgetUIItem;
    // public CityMetricUIItem greenSpaceUIItem;
    public CityMetricUIItem urbanHeatUIItem;
    public CityMetricUIItem pollutionUIItem;
    public CityMetricUIItem energyUIItem;
    public CityMetricUIItem carbonEmissionUIItem;
    public CityMetricUIItem revenueUIItem;



    void Start()
    {
        cityMetricsManager.OnMetricsUpdate += UpdateMetrics;
    }

    void Awake()
    {
        UpdateMetrics();
    }

    void OnDestroy()
    {
        cityMetricsManager.OnMetricsUpdate -= UpdateMetrics;
    }

    public void UpdateMetrics()
    {
        populationUIItem.UpdateValue(cityMetricsManager.population.ToString());
        happinessUIItem.UpdateValue(cityMetricsManager.happiness.ToString());
        budgetUIItem.UpdateValue(NumbersUtils.NumberToAbrev(cityMetricsManager.budget, "", ""));
        urbanHeatUIItem.UpdateValue(cityMetricsManager.urbanHeat.ToString());
        pollutionUIItem.UpdateValue(cityMetricsManager.pollution.ToString());
        energyUIItem.UpdateValue(NumbersUtils.NumberToAbrev(cityMetricsManager.energy, "", "KW"));
        carbonEmissionUIItem.UpdateValue(cityMetricsManager.carbonEmission.ToString());
        revenueUIItem.UpdateValue(NumbersUtils.NumberToAbrev(cityMetricsManager.revenue, "", ""));
    }


    public void OnMetricClick(MetricTitle metricTitle)
    {

        if (modal.activeSelf) return;

        modal.SetActive(true);

        MetricModalItem selectedItem = items.Find(item => item.title.ToString().ToLower() == metricTitle.ToString().ToLower());

        if (selectedItem != null)
        {
            // Update the modal's description text
            modalBodyText.text = selectedItem.description;
            modalTitle.text = StringsUtils.ConvertToLabel(selectedItem.title.ToString());
        }
        else
        {
            // If the metric is not found, show an error or fallback message
            modalBodyText.text = "Metric description not found.";
            modalTitle.text = StringsUtils.ConvertToLabel(metricTitle.ToString());
        }
    }


    public void OnModalClose() { modal.SetActive(false); }
    public void OnPopulationClick() { OnMetricClick(MetricTitle.Population); }
    public void OnHappinessClick() { OnMetricClick(MetricTitle.Happiness); }
    public void OnBudgetClick() { OnMetricClick(MetricTitle.Budget); }
    // public void OnGreenSpaceClick() { OnMetricClick(MetricTitle.GreenSpace); }
    public void OnUrbanHeatClick() { OnMetricClick(MetricTitle.UrbanHeat); }
    public void OnPollutionClick() { OnMetricClick(MetricTitle.Pollution); }
    public void OnEnergyClick() { OnMetricClick(MetricTitle.Energy); }
    public void OnCarbonEmissionClick() { OnMetricClick(MetricTitle.CarbonEmission); }
    public void OnRevenueClick() { OnMetricClick(MetricTitle.Revenue); }

}
