using System.Collections.Generic;
using UnityEngine;

namespace GreenCityBuilder.Missions
{
    public class GreenSpaceRenaissanceMission : Mission
    {
        public GreenSpaceRenaissanceMission(Dictionary<MetricTitle, Sprite> metricIcons) : base(metricIcons)
        {
            missionName = "Green Space Renaissance";
            missionObjective = "Increase green space by 30% while keeping pollution below a set level and achieving a 10% boost in happiness.";
            missionBrief = "The city council has called for a 'Green Space Renaissance' to improve the quality of life and promote environmental health. You’re tasked with designing an expansion that adds parks, forests, and green rooftops to meet this goal. However, the city’s industrial and commercial zones generate high pollution, so you’ll need to strategically place buildings like recycling stations and green energy facilities to offset it.";
            // missionMetrics = "Green Space, Pollution, Happiness";
            timeLimitInMonths = 5 * 12;
            startingBudget = 2000000;
            missionIcon = Resources.Load<Sprite>("Missions/mission_icon-green_space");
            difficulty = DifficultyLevel.Hard;
            missionCityFileName = "green_renaissance_city";

            AddMetric(MetricTitle.GreenSpace);
            AddMetric(MetricTitle.Pollution);
            AddMetric(MetricTitle.Happiness);

            objectives = new MissionObjective[]
            {
                new() {
                    metricName = MetricTitle.GreenSpace,
                    objectiveType = MissionObjective.ObjectiveType.IncreaseByPercentage,
                    comparisonPercentage = 30,
                    icon = GetMetricIcon(MetricTitle.GreenSpace)
                },
                new() {
                    metricName = MetricTitle.Pollution,
                    objectiveType = MissionObjective.ObjectiveType.MaintainBelow,
                    targetValue = 50,
                    icon = GetMetricIcon(MetricTitle.Pollution)
                },
                new() {
                    metricName = MetricTitle.Happiness,
                    objectiveType = MissionObjective.ObjectiveType.IncreaseByPercentage,
                    comparisonPercentage = 10,
                    icon = GetMetricIcon(MetricTitle.Happiness)
                }
            };
        }
    }
}

