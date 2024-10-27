namespace GreenCityBuilder.Missions
{
    public class CoolTheCityDownMission : Mission
    {
        public CoolTheCityDownMission()
        {
            missionName = "Cool the City Down";
            missionObjective = "Reduce the city’s temperature and urban heat levels by 20% within 6 years, while maintaining the population and happiness levels.";
            missionBrief = "The summer heat is impacting citizen well-being and increasing energy demands.";
            missionMetrics = "City Temperature, Urban Heat, Happiness";
            timeLimitInMonths = 6 * 12;

            objectives = new MissionObjective[]
            {
                new MissionObjective
                {
                    metricName = MetricTitle.CityTemperature,
                    objectiveType = MissionObjective.ObjectiveType.ReduceByPercentage,
                    comparisonPercentage = 20
                },
                new MissionObjective
                {
                    metricName = MetricTitle.UrbanHeat,
                    objectiveType = MissionObjective.ObjectiveType.ReduceByPercentage,
                    comparisonPercentage = 20
                },
                new MissionObjective
                {
                    metricName = MetricTitle.Happiness,
                    objectiveType = MissionObjective.ObjectiveType.MaintainAbove,
                    targetValue = 75
                }
            };
        }
    }
}
