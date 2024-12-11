using System.Collections.Generic;
using UnityEngine;

/**
Defines an easy-level mission in the Green City Builder game. 
Players must increase happiness by 7% and maintain the city’s temperature below 70°F 
by strategically adding parks and green spaces near residential areas. 
With a starting budget of $500,000 and a 2-year time limit, 
the mission focuses on improving community well-being and demonstrating the cooling effects of greenery. 
Success highlights the benefits of green spaces for happiness and health, 
while failure underscores their importance for urban quality of life. 
Metrics include happiness and city temperature.
**/
namespace GreenCityBuilder.Missions
{
    public class ParksAndRecMission : Mission
    {
        public ParksAndRecMission(Dictionary<MetricTitle, Sprite> metricIcons) : base(metricIcons)
        {
            missionName = "Parks and Recreation Boost";
            missionObjective = "Increase happiness by 7% and keep the city below 70°F by adding parks and green spaces around residential areas.";
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
            AddMetric(MetricTitle.CityTemperature);

            objectives = new MissionObjective[]
            {
                new() {
                    metricName = MetricTitle.Happiness,
                    objectiveType = MissionObjective.ObjectiveType.IncreaseByPercentage,
                    comparisonPercentage = 7,
                    icon = GetMetricIcon(MetricTitle.Happiness)
                },

                new() {
                    metricName = MetricTitle.CityTemperature,
                    objectiveType = MissionObjective.ObjectiveType.MaintainBelow,
                    targetValue = 70,
                    icon = GetMetricIcon(MetricTitle.Happiness)
                },
            };
        }


    }
}
