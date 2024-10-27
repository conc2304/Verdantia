public Mission GreenSpaceRenaissance = new Mission
{
    missionName = "Green Space Renaissance",
    missionObjective = "Increase green space by 30% while keeping pollution below a set level and achieving a 10% boost in happiness.",
    missionBrief = "The city council has called for a 'Green Space Renaissance' to improve the quality of life and promote environmental health. You’re tasked with designing an expansion that adds parks, forests, and green rooftops to meet this goal. However, the city’s industrial and commercial zones generate high pollution, so you’ll need to strategically place buildings like recycling stations and green energy facilities to offset it.",
    missionMetrics = "Green Space, Pollution, Happiness",
    timeLimitInMonths = 5 * 12,
    startingBudget = 2000000,
    objectives = new MissionObjective[]
    {
        new MissionObjective
        {
            metricName = MetricTitle.GreenSpace,
            objectiveType = MissionObjective.ObjectiveType.IncreaseByPercentage,
            comparisonPercentage = 30
        },
        new MissionObjective
        {
            metricName = MetricTitle.Pollution,
            objectiveType = MissionObjective.ObjectiveType.MaintainBelow,
            targetValue = 50
        },
        new MissionObjective
        {
            metricName = MetricTitle.Happiness,
            objectiveType = MissionObjective.ObjectiveType.IncreaseByPercentage,
            comparisonPercentage = 10
        }
    }
};
