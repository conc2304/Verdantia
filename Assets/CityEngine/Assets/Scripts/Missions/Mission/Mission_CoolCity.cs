using System.Collections.Generic;
using UnityEngine;

namespace GreenCityBuilder.Missions
{
    public class CoolTheCityDownMission : Mission
    {
        public CoolTheCityDownMission(Dictionary<MetricTitle, Sprite> metricIcons) : base(metricIcons)
        {
            missionName = "Cool the City Down";
            missionObjective = "Reduce the city’s temperature and urban heat levels by 20%, while maintaining the population and happiness levels.";
            {
                missionBrief = " The summer heat is impacting citizen well-being and increasing energy demands. Cool down the city by adding green space, reducing heat contributions, and managing pollution levels. Use parks, eco-friendly buildings, and urban green spaces to lower the temperature and fight the urban heat island effect. Be careful, though—budget constraints require you to balance your expenses and avoid overspending..";
                // missionMetrics = "City Temperature, Urban Heat, Happiness";
                timeLimitInMonths = 6 * 12;
                missionIcon = Resources.Load<Sprite>("Missions/mission_icon-cool_the_city");
                startingBudget = 1300000;
                difficulty = DifficultyLevel.Medium;


                AddMetric(MetricTitle.CityTemperature);
                AddMetric(MetricTitle.UrbanHeat);
                AddMetric(MetricTitle.Happiness);

                objectives = new MissionObjective[]
                {
                new MissionObjective
                {
                    metricName = MetricTitle.CityTemperature,
                    objectiveType = MissionObjective.ObjectiveType.ReduceByPercentage,
                    comparisonPercentage = 20,
                    icon = GetMetricIcon(MetricTitle.CityTemperature)
                },
                new MissionObjective
                {
                    metricName = MetricTitle.UrbanHeat,
                    objectiveType = MissionObjective.ObjectiveType.ReduceByPercentage,
                    comparisonPercentage = 20,
                    icon = GetMetricIcon(MetricTitle.UrbanHeat)

                },
                new MissionObjective
                {
                    metricName = MetricTitle.Happiness,
                    objectiveType = MissionObjective.ObjectiveType.MaintainAbove,
                    targetValue = 75,
                    icon = GetMetricIcon(MetricTitle.Happiness)
                }
                };
            }
        }
    }
}
