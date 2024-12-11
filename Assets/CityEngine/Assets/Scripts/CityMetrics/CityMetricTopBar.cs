using UnityEngine;
using System.Collections.Generic;
using GreenCityBuilder.Missions;

/**
Manages the display and updating of city metrics in a top bar UI, either for specific mission objectives or for general "free play" mode. 
It dynamically creates metric UI items based on the current mission or the default metrics for free play, 
updates them in response to metric changes, and ensures metrics like the budget are always displayed. 
It listens for mission start events and metric updates, using this information to update the displayed values and relevant labels. 
This script integrates closely with CityMetricsManager and MissionManager to provide real-time feedback 
to the player about the state of the city and mission progress.
**/
public class CityMetricTopBar : MonoBehaviour
{
    public CityMetricsManager cityMetricsManager;
    public MissionManager missionManager;
    public Transform missionMetricsContainer;
    public GameObject metricItemPrefab;
    public Mission currentMission;

    private Dictionary<MetricTitle, CityMetricUIItem> activeMetricItems = new Dictionary<MetricTitle, CityMetricUIItem>();

    void Start()
    {
        cityMetricsManager.OnMetricsUpdate += UpdateMetrics;
        missionManager.onMissionStarted += HandleMissionStarted;

        DisplayMetricsForCurrentMode(currentMission);
    }

    public void HandleMissionStarted(Mission mission)
    {
        currentMission = mission;
        DisplayMetricsForCurrentMode(mission);
        UpdateMetrics();
    }

    private void DisplayMetricsForCurrentMode(Mission mission)
    {
        // Clear previous metrics from missionMetricsContainer
        foreach (Transform child in missionMetricsContainer)
        {
            Destroy(child.gameObject);
        }
        activeMetricItems.Clear();

        bool budgetIncluded = false;  // Track if Budget metric is included

        // If there's a current mission, display its specific metrics
        if (mission != null && mission.objectives.Length > 0)
        {
            foreach (MissionObjective objective in mission.objectives)
            {
                if (objective.metricName == MetricTitle.CityTemperature) continue; // city temp has its own ui

                CreateAndDisplayMetricItem(objective.metricName, objective.icon, objective.targetValue.ToString());

                // Check if Budget is one of the objectives
                if (objective.metricName == MetricTitle.Budget) budgetIncluded = true;

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
            CreateAndDisplayMetricItem(MetricTitle.Population, MissionRepository.metricIcons[MetricTitle.Population]);
            CreateAndDisplayMetricItem(MetricTitle.Happiness, MissionRepository.metricIcons[MetricTitle.Happiness]);
            // CreateAndDisplayMetricItem(MetricTitle.GreenSpace, MissionRepository.metricIcons[MetricTitle.GreenSpace]);
            CreateAndDisplayMetricItem(MetricTitle.UrbanHeat, MissionRepository.metricIcons[MetricTitle.UrbanHeat]);
            CreateAndDisplayMetricItem(MetricTitle.Budget, MissionRepository.metricIcons[MetricTitle.Budget]);
        }
    }

    private void CreateAndDisplayMetricItem(MetricTitle metricTitle, Sprite icon, string targetValue = null)
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

        // if metric has a target objective then display it
        if (targetValue != null)
        {
            metricUI.UpdateTargetValue(targetValue);
        }
        else
        {
            metricUI.targetText.gameObject.SetActive(false);
        }

        activeMetricItems[metricTitle] = metricUI;
    }

    public void UpdateMetrics()
    {
        // Update either the mission-specific metrics or default metrics
        if (currentMission != null)
        {
            foreach (MissionObjective objective in currentMission.objectives)
            {
                UpdateMetricItem(objective.metricName);
            }
        }
        else
        {
            // Free play mode: update default metrics
            UpdateMetricItem(MetricTitle.Population);
            UpdateMetricItem(MetricTitle.Happiness);
            // UpdateMetricItem(MetricTitle.GreenSpace);
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
                    metricUI.UpdateValue(NumbersUtils.NumberToAbrev(cityMetricsManager.budget, "", ""));
                    break;
                // case MetricTitle.GreenSpace:
                //     metricUI.UpdateValue(cityMetricsManager.greenSpace.ToString() + "");
                //     break;
                case MetricTitle.UrbanHeat:
                    metricUI.UpdateValue(cityMetricsManager.urbanHeat.ToString());
                    break;
                case MetricTitle.Pollution:
                    metricUI.UpdateValue(cityMetricsManager.pollution.ToString());
                    break;
                case MetricTitle.Energy:
                    metricUI.UpdateValue(NumbersUtils.NumberToAbrev(cityMetricsManager.energy, "", "KW"));
                    break;
                case MetricTitle.CarbonEmission:
                    metricUI.UpdateValue(cityMetricsManager.carbonEmission.ToString());
                    break;
                case MetricTitle.Revenue:
                    metricUI.UpdateValue(NumbersUtils.NumberToAbrev(cityMetricsManager.revenue, "", ""));
                    break;
            }
        }
    }

    void OnDestroy()
    {
        cityMetricsManager.OnMetricsUpdate -= UpdateMetrics;
        missionManager.onMissionStarted -= HandleMissionStarted;
    }
}
