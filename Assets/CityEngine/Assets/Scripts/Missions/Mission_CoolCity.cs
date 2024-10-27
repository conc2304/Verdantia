public Mission CoolTheCityDown = new Mission
{
    missionName = "Cool the City Down",
    missionObjective = "Reduce the city’s <b>temperature</b> and <b>urban heat</b> levels by 20% within 6 years, while maintaining the <b>population</b> and <b>happiness</b> levels.",
    missionBrief = "The summer heat is impacting citizen well-being and increasing energy demands. Cool down the city by adding green space, reducing heat contributions, and managing pollution levels. Use parks, eco-friendly buildings, and urban green spaces to lower the temperature and fight the urban heat island effect. Be careful, though—budget constraints require you to balance your expenses and avoid overspending.",
    missionMetrics = "City Temperature, Urban Heat, Green Space, Budget, Happiness",
    timeLimitInMonths = 6 * 12,
    startingBudget = 1200000,
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
    }
};
