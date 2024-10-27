
namespace GreenCityBuilder.Missions
{
    public class PollutionControlMission : Mission
    {
        public PollutionControlMission()
        {
            missionName = "Pollution Control Challenge";
            missionObjective = "Reduce pollution by 40% within a set timeframe, while maintaining positive revenue and population growth.";
            missionBrief = "With rising pollution levels, the city is facing public health and environmental crises. Take decisive action to cut pollution by switching to green energy sources, upgrading factories to eco-friendly versions, and increasing green infrastructure. Ensure that revenue sources are maintained to cover the costs of these upgrades and attract more residents by creating a healthier city.";
            missionMetrics = "Pollution, Revenue, Population, Health Impact";
            timeLimitInMonths = 3 * 12;
            startingBudget = 27000000;
            objectives = new MissionObjective[]
            {
                new MissionObjective
                {
                    metricName = MetricTitle.Pollution,
                    objectiveType = MissionObjective.ObjectiveType.ReduceByPercentage,
                    comparisonPercentage = 40
                },
                new MissionObjective
                {
                    metricName = MetricTitle.Revenue,
                    objectiveType = MissionObjective.ObjectiveType.MaintainAbove,
                    targetValue = 10000
                },
                new MissionObjective
                {
                    metricName = MetricTitle.Population,
                    objectiveType = MissionObjective.ObjectiveType.MaintainAbove,
                    targetValue = 50000
                }
            };
        }
    }
}
