using UnityEngine;

namespace GreenCityBuilder.Missions
{
    public class BudgetBalanceMission : Mission
    {
        public BudgetBalanceMission()
        {
            missionName = "Budget Balance for a Greener Future";
            missionObjective = "Increase the city’s budget by 25% while keeping carbon emissions low and happiness stable.";
            missionBrief = "Your goal is to balance the budget to fund new eco-initiatives without compromising citizen satisfaction or raising carbon emissions. Develop eco-friendly infrastructure and make use of tax-generating buildings like offices, malls, and entertainment venues. Implement energy-saving solutions to keep expenses in check while creating a sustainable revenue stream to support ongoing green initiatives.";
            missionMetrics = "Budget, Carbon Emissions, Happiness, Income, Expenses";
            timeLimitInMonths = 4 * 12;
            startingBudget = 1700000;
            difficulty = 1; // medium;
            missionIcon = Resources.Load<Sprite>("Missions/mission_icon-budget_balance");

            objectives = new MissionObjective[]
            {
                new() {
                    metricName = MetricTitle.Budget,
                    objectiveType = MissionObjective.ObjectiveType.IncreaseByPercentage,
                    comparisonPercentage = 25
                },
                new() {
                    metricName = MetricTitle.CarbonEmission,
                    objectiveType = MissionObjective.ObjectiveType.MaintainBelow,
                    targetValue = 200
                },
                new() {
                    metricName = MetricTitle.Happiness,
                    objectiveType = MissionObjective.ObjectiveType.MaintainAbove,
                    targetValue = 70
                }
            };
        }
    }
}
