using System.Collections.Generic;
using UnityEngine;

namespace GreenCityBuilder.Missions
{
    public class ParksAndRecMission : Mission
    {
        public ParksAndRecMission(Dictionary<MetricTitle, Sprite> metricIcons) : base(metricIcons)
        {
            missionName = "Parks and Recreation Boost";
            missionObjective = "Increase happiness by 5% by adding parks and green spaces around residential areas.";
            missionBrief = "Residents are eager for more green spaces and recreational areas to improve their quality of life. Your task is to create an inviting environment by strategically placing small and medium parks around the city’s neighborhoods. This will boost happiness and improve the community’s overall well-being. With a reasonable budget and minimal restrictions, you can freely add parks and green zones to meet the happiness target.";
            startingBudget = 500000;
            timeLimitInMonths = 24;
            missionIcon = Resources.Load<Sprite>("Missions/mission_icon-parks_and_rec");
            difficulty = DifficultyLevel.Easy;
            missionCityFileName = "parks_and_rec_city";

            successMessage = "Your parks and green spaces have made residents happier and healthier, proving that greenery cools cities and uplifts communities.";
            failedMessage = "Without enough parks and green spaces, residents remain unhappy, showing how vital greenery is for cooling and well-being.";


            // AddMetric(MetricTitle.GreenSpace);
            AddMetric(MetricTitle.Happiness);

            objectives = new MissionObjective[]
            {
                new() {
                    metricName = MetricTitle.Happiness,
                    objectiveType = MissionObjective.ObjectiveType.IncreaseByPercentage,
                    comparisonPercentage = 5,
                    icon = GetMetricIcon(MetricTitle.Happiness)
                },
            };
        }
    }
}
