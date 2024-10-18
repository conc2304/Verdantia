using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;


public class CityMetricsDataDisplay : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject modal;
    public TMP_Text modalTitle;
    public TMP_Text modalBodyText;
    public List<MetricModalItem> items = new List<MetricModalItem>();

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
    public void OnGreenSpaceClick() { OnMetricClick(MetricTitle.GreenSpace); }
    public void OnUrbanHeatClick() { OnMetricClick(MetricTitle.UrbanHeat); }
    public void OnPollutionClick() { OnMetricClick(MetricTitle.Pollution); }
    public void OnEnergyClick() { OnMetricClick(MetricTitle.Energy); }
    public void OnCarbonEmissionClick() { OnMetricClick(MetricTitle.CarbonEmission); }
    public void OnRevenueClick() { OnMetricClick(MetricTitle.Revenue); }
    public void OnIncomeClick() { OnMetricClick(MetricTitle.Income); }
    public void OnExpensesClick() { OnMetricClick(MetricTitle.Expenses); }
}
