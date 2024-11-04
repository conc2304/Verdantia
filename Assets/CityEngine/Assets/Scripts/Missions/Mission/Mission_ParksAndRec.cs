using System.Collections.Generic;
using UnityEngine;

namespace GreenCityBuilder.Missions
{
    public class ParksAndRecMission : Mission
    {
        public ParksAndRecMission(Dictionary<MetricTitle, Sprite> metricIcons) : base(metricIcons)
        {
            missionName = "Parks and Recreation Boost";
            missionObjective = "Increase happiness by 15% by adding parks and green spaces around residential areas.";
            missionBrief = "Residents are eager for more green spaces and recreational areas to improve their quality of life. Your task is to create an inviting environment by strategically placing small and medium parks around the city’s neighborhoods. This will boost happiness and improve the community’s overall well-being. With a reasonable budget and minimal restrictions, you can freely add parks and green zones to meet the happiness target.";
            // missionMetrics = "Happiness, Green Space";
            startingBudget = 500000;
            timeLimitInMonths = 24;
            missionIcon = Resources.Load<Sprite>("Missions/mission_icon-parks_and_rec");
            difficulty = DifficultyLevel.Easy;

            AddMetric(MetricTitle.GreenSpace);
            AddMetric(MetricTitle.Happiness);

            objectives = new MissionObjective[]
            {
                new() {
                    metricName = MetricTitle.Happiness,
                    objectiveType = MissionObjective.ObjectiveType.IncreaseByPercentage,
                    comparisonPercentage = 15,
                    icon = GetMetricIcon(MetricTitle.Happiness)

                },
                new() {
                    metricName = MetricTitle.GreenSpace,
                    objectiveType = MissionObjective.ObjectiveType.MaintainAbove,
                    targetValue = 30, // TODO UPDATE THIS
                    icon = GetMetricIcon(MetricTitle.GreenSpace)
                }
            };
        }
    }
}
