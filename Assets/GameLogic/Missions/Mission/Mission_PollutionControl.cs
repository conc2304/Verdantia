using System.Collections.Generic;
using UnityEngine;

/**
Defines an extreme-level mission in the Green City Builder game. 
Players are tasked with reducing pollution by 40% while maintaining positive revenue and population growth. 
With a starting budget of $27,000,000 and a 3-year time limit, players must implement green energy, 
eco-friendly factory upgrades, and green infrastructure to improve the city's health and sustainability. 
Success highlights the impact of green initiatives on reducing pollution and fostering growth, 
while failure underscores the risks of inaction. 
Metrics include pollution, revenue, and population.
**/
namespace GreenCityBuilder.Missions
{
    public class PollutionControlMission : Mission
    {
        public PollutionControlMission(Dictionary<MetricTitle, Sprite> metricIcons) : base(metricIcons)
        {
            missionName = "Pollution Control Challenge";
            missionObjective = "Reduce pollution by 40% while maintaining positive revenue and population growth.";
            missionBrief = "With rising pollution levels, the city is facing public health and environmental crises. Take decisive action to cut pollution by switching to green energy sources, upgrading factories to eco-friendly versions, and increasing green infrastructure. Ensure that revenue sources are maintained to cover the costs of these upgrades and attract more residents by creating a healthier city.";
            timeLimitInMonths = 3 * 12;
            startingBudget = 27000000;
            missionIcon = Resources.Load<Sprite>("Missions/mission_icon-pollution_control");
            difficulty = DifficultyLevel.Extreme;
            missionCityFileName = "pollution_control_city";

            successMessage = "Your green upgrades have slashed pollution, boosted revenue, and attracted new residents to a cleaner, healthier city.";
            failedMessage = "Pollution levels remain critical, and without decisive action, the city's health and growth are at risk.";

            AddMetric(MetricTitle.Pollution);
            AddMetric(MetricTitle.Revenue);
            AddMetric(MetricTitle.Population);

            objectives = new MissionObjective[]
            {
                new MissionObjective
                {
                    metricName = MetricTitle.Pollution,
                    objectiveType = MissionObjective.ObjectiveType.ReduceByPercentage,
                    comparisonPercentage = 40,
                    icon = GetMetricIcon(MetricTitle.Pollution)
                },
                new MissionObjective
                {
                    metricName = MetricTitle.Revenue,
                    objectiveType = MissionObjective.ObjectiveType.MaintainAbove,
                    targetValue = 10000,
                    icon = GetMetricIcon(MetricTitle.Revenue)

                },
                new MissionObjective
                {
                    metricName = MetricTitle.Population,
                    objectiveType = MissionObjective.ObjectiveType.MaintainAbove,
                    targetValue = 50000,
                    icon = GetMetricIcon(MetricTitle.Population)

                }
            };
        }
    }
}
