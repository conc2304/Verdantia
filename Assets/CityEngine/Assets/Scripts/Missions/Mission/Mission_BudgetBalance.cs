using System.Collections.Generic;
using UnityEngine;

namespace GreenCityBuilder.Missions
{
    public class BudgetBalanceMission : Mission
    {
        public BudgetBalanceMission(Dictionary<MetricTitle, Sprite> metricIcons) : base(metricIcons)
        {
            missionName = "Budget Balance for a Greener Future";
            missionObjective = "Increase the city’s budget by 25% while keeping carbon emissions low and happiness stable.";
            missionBrief = "Your goal is to balance the budget to fund new eco-initiatives without compromising citizen satisfaction or raising carbon emissions. Develop eco-friendly infrastructure and make use of tax-generating buildings like offices, malls, and entertainment venues. Implement energy-saving solutions to keep expenses in check while creating a sustainable revenue stream to support ongoing green initiatives.";
            timeLimitInMonths = 4 * 12;
            startingBudget = 1700000;
            difficulty = DifficultyLevel.Medium;
            missionIcon = Resources.Load<Sprite>("Missions/mission_icon-budget_balance");
            missionCityFileName = "budget_balance_city";

            // Add metrics with icons
            AddMetric(MetricTitle.Budget);
            AddMetric(MetricTitle.CarbonEmission);
            AddMetric(MetricTitle.Happiness);

            objectives = new MissionObjective[]
            {
                new MissionObjective() {
                    metricName = MetricTitle.Budget,
                    objectiveType = MissionObjective.ObjectiveType.IncreaseByPercentage,
                    comparisonPercentage = 25,
                    icon = GetMetricIcon(MetricTitle.Budget)
                },
                new MissionObjective() {
                    metricName = MetricTitle.CarbonEmission,
                    objectiveType = MissionObjective.ObjectiveType.MaintainBelow,
                    targetValue = 200,
                    icon = GetMetricIcon(MetricTitle.CarbonEmission)
                },
                new MissionObjective() {
                    metricName = MetricTitle.Happiness,
                    objectiveType = MissionObjective.ObjectiveType.MaintainAbove,
                    targetValue = 70,
                    icon = GetMetricIcon(MetricTitle.Happiness)
                }
            };
        }
    }
}
