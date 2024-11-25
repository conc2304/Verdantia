using System.Collections.Generic;
using UnityEngine;

namespace GreenCityBuilder.Missions
{
    public class CoolTheCityDownMission : Mission
    {
        public CoolTheCityDownMission(Dictionary<MetricTitle, Sprite> metricIcons) : base(metricIcons)
        {
            missionName = "Cool the City Down";
            missionObjective = "Reduce the city’s temperature and urban heat levels by 10%, while maintaining the population and happiness levels.";
            {
                missionBrief = " The summer heat is impacting citizen well-being and increasing energy demands. Cool down the city by adding green space, reducing heat contributions, and managing pollution levels. Use parks, eco-friendly buildings, and urban green spaces to lower the temperature and fight the urban heat island effect. Be careful, though—budget constraints require you to balance your expenses and avoid overspending..";
                timeLimitInMonths = 6 * 12;
                missionIcon = Resources.Load<Sprite>("Missions/mission_icon-cool_the_city");
                startingBudget = 1300000;
                difficulty = DifficultyLevel.Medium;
                missionCityFileName = "cool_down_city";

                successMessage = "Your green initiatives have cooled the city, reducing urban heat and improving life for a thriving, happy population.";
                failedMessage = "The city's heat remains unchecked, highlighting the urgent need for more green spaces to combat rising temperatures.";

                AddMetric(MetricTitle.CityTemperature);
                AddMetric(MetricTitle.UrbanHeat);
                AddMetric(MetricTitle.Happiness);

                objectives = new MissionObjective[]
                {
                new MissionObjective
                {
                    metricName = MetricTitle.CityTemperature,
                    objectiveType = MissionObjective.ObjectiveType.ReduceByPercentage,
                    comparisonPercentage = 10,
                    icon = GetMetricIcon(MetricTitle.CityTemperature)
                },
                new MissionObjective
                {
                    metricName = MetricTitle.UrbanHeat,
                    objectiveType = MissionObjective.ObjectiveType.ReduceByPercentage,
                    comparisonPercentage = 10,
                    icon = GetMetricIcon(MetricTitle.UrbanHeat)

                },
                new MissionObjective
                {
                    metricName = MetricTitle.Happiness,
                    objectiveType = MissionObjective.ObjectiveType.MaintainAbove,
                    targetValue = 65,
                    icon = GetMetricIcon(MetricTitle.Happiness)
                }
                };
            }
        }
    }
}
