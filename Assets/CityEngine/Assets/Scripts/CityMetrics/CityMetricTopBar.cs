using UnityEngine;
using System.Collections.Generic;
using GreenCityBuilder.Missions;
using Unity.VisualScripting;

public class CityMetricTopBar : MonoBehaviour
{
    public CityMetricsManager cityMetricsManager;
    public MissionCatalog missionCatalog;
    public Transform missionMetricsContainer;
    public GameObject metricItemPrefab;  // Prefab to dynamically display metrics
    public Mission currentMission;       // Reference to the current mission (null if "free play")

    private Dictionary<MetricTitle, CityMetricUIItem> activeMetricItems = new Dictionary<MetricTitle, CityMetricUIItem>();

    void Start()
    {
        cityMetricsManager.OnMetricsUpdate += UpdateMetrics;
        missionCatalog.OnMissionAccepted += SetMission;

        DisplayMetricsForCurrentMode();
    }

    public void SetMission(Mission mission)
    {
        currentMission = mission;
        DisplayMetricsForCurrentMode();
    }

    private void DisplayMetricsForCurrentMode()
    {
        // Clear previous metrics from missionMetricsContainer
        foreach (Transform child in missionMetricsContainer)
        {
            Destroy(child.gameObject);
        }
        activeMetricItems.Clear();

        bool budgetIncluded = false;  // Track if Budget metric is included

        // If there's a current mission, display its specific metrics
        if (currentMission != null)
        {
            foreach (var objective in currentMission.objectives)
            {
                CreateAndDisplayMetricItem(objective.metricName, objective.icon);

                // Check if Budget is one of the objectives
                if (objective.metricName == MetricTitle.Budget)
                {
                    budgetIncluded = true;
                }
            }

            // Add Budget at the end if not already included
            if (!budgetIncluded)
            {
                CreateAndDisplayMetricItem(MetricTitle.Budget, MissionRepository.metricIcons[MetricTitle.Budget]);
            }
        }
        else
        {
            // Free play mode: display default metrics, including Budget
            // Free play mode: display default metrics, including Budget
            CreateAndDisplayMetricItem(MetricTitle.Population, MissionRepository.metricIcons[MetricTitle.Population]);
            CreateAndDisplayMetricItem(MetricTitle.Happiness, MissionRepository.metricIcons[MetricTitle.Happiness]);
            CreateAndDisplayMetricItem(MetricTitle.Budget, MissionRepository.metricIcons[MetricTitle.Budget]);
            CreateAndDisplayMetricItem(MetricTitle.GreenSpace, MissionRepository.metricIcons[MetricTitle.GreenSpace]);
            CreateAndDisplayMetricItem(MetricTitle.UrbanHeat, MissionRepository.metricIcons[MetricTitle.UrbanHeat]);
        }
    }

    private void CreateAndDisplayMetricItem(MetricTitle metricTitle, Sprite icon)
    {
        // Instantiate and set up a new metric item
        GameObject metricItem = Instantiate(metricItemPrefab, missionMetricsContainer);
        CityMetricUIItem metricUI = metricItem.GetComponent<CityMetricUIItem>();
        metricUI.SetLabel(StringsUtils.ConvertToLabel(metricTitle.ToString()));
        metricUI.SetIcon(icon);

        // Get unit and position, then apply it to the UI item
        string unit = MetricUnits.GetUnit(metricTitle);
        MetricUnits.UnitPosition position = MetricUnits.GetUnitPosition(metricTitle);
        metricUI.SetUnit(unit, position);

        activeMetricItems[metricTitle] = metricUI;
    }

    public void UpdateMetrics()
    {
        // Update either the mission-specific metrics or default metrics
        if (currentMission != null)
        {
            foreach (var objective in currentMission.objectives)
            {
                UpdateMetricItem(objective.metricName);
            }
        }
        else
        {
            // Free play mode: update default metrics
            UpdateMetricItem(MetricTitle.Population);
            UpdateMetricItem(MetricTitle.Happiness);
            UpdateMetricItem(MetricTitle.GreenSpace);
            UpdateMetricItem(MetricTitle.UrbanHeat);
        }
        // Budget should always be visible
        UpdateMetricItem(MetricTitle.Budget);
    }

    private void UpdateMetricItem(MetricTitle metricTitle)
    {
        if (activeMetricItems.TryGetValue(metricTitle, out CityMetricUIItem metricUI))
        {
            // Update value based on the metric from CityMetricsManager
            switch (metricTitle)
            {
                case MetricTitle.CityTemperature:
                    metricUI.UpdateValue(cityMetricsManager.cityTemperature.ToString());
                    break;
                case MetricTitle.Population:
                    metricUI.UpdateValue(cityMetricsManager.population.ToString());
                    break;
                case MetricTitle.Happiness:
                    metricUI.UpdateValue(cityMetricsManager.happiness.ToString());
                    break;
                case MetricTitle.Budget:
                    metricUI.UpdateValue(cityMetricsManager.budget.ToString());
                    break;
                case MetricTitle.GreenSpace:
                    metricUI.UpdateValue(cityMetricsManager.greenSpace.ToString());
                    break;
                case MetricTitle.UrbanHeat:
                    metricUI.UpdateValue(cityMetricsManager.urbanHeat.ToString());
                    break;
                case MetricTitle.Pollution:
                    metricUI.UpdateValue(cityMetricsManager.pollution.ToString());
                    break;
                case MetricTitle.Energy:
                    metricUI.UpdateValue(cityMetricsManager.energy.ToString());
                    break;
                case MetricTitle.CarbonEmission:
                    metricUI.UpdateValue(cityMetricsManager.carbonEmission.ToString());
                    break;
                case MetricTitle.Revenue:
                    metricUI.UpdateValue(cityMetricsManager.revenue.ToString());
                    break;
                case MetricTitle.Income:
                    metricUI.UpdateValue(cityMetricsManager.income.ToString());
                    break;
                case MetricTitle.Expenses:
                    metricUI.UpdateValue(cityMetricsManager.expenses.ToString());
                    break;
            }
        }
    }

    void OnDestroy()
    {
        cityMetricsManager.OnMetricsUpdate -= UpdateMetrics;
        missionCatalog.OnMissionAccepted -= SetMission;
    }
}
