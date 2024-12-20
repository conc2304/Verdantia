using System.Collections.Generic;
using UnityEngine;

/**
// NOTE Deprecated due to temporary/permament removal of the Green Space metric from the game

Defines a high-difficulty mission in the Green City Builder game, 
where players are tasked with transforming the city by reducing pollution and increasing happiness by 10%. 
The mission emphasizes creating an environmentally friendly urban expansion while 
strategically managing industrial and commercial zones to control pollution. 
It includes a time limit of 5 years, a starting budget of $2,000,000, and specific metrics for pollution and happiness. 
Success reflects a thriving and balanced urban ecosystem, while failure highlights the need for better planning.
**/
namespace GreenCityBuilder.Missions
{
    public class GreenSpaceRenaissanceMission : Mission
    {
        public GreenSpaceRenaissanceMission(Dictionary<MetricTitle, Sprite> metricIcons) : base(metricIcons)
        {
            missionName = "Green Space Renaissance";
            // TODO update text to not include green space as part of mission
            missionObjective = "Increase green space by 30% while keeping pollution below a set level and achieving a 10% boost in happiness.";
            missionBrief = "The city council has called for a 'Green Space Renaissance' to improve the quality of life and promote environmental health. You’re tasked with designing an expansion that adds parks, forests, and green rooftops to meet this goal. However, the city’s industrial and commercial zones generate high pollution, so you’ll need to strategically place buildings like recycling stations and green energy facilities to offset it.";
            timeLimitInMonths = 5 * 12;
            startingBudget = 2000000;
            missionIcon = Resources.Load<Sprite>("Missions/mission_icon-green_space");
            difficulty = DifficultyLevel.Hard;
            missionCityFileName = "green_renaissance_city";

            successMessage = "Your green expansion has transformed the city, reducing pollution and boosting happiness while creating lush, thriving spaces.";
            failedMessage = "Without enough green growth, pollution and dissatisfaction persist, showing the city's need for a true green renaissance.";


            // AddMetric(MetricTitle.GreenSpace);
            AddMetric(MetricTitle.Pollution);
            AddMetric(MetricTitle.Happiness);

            objectives = new MissionObjective[]
            {
                // new() {
                //     metricName = MetricTitle.GreenSpace,
                //     objectiveType = MissionObjective.ObjectiveType.IncreaseByPercentage,
                //     comparisonPercentage = 30,
                //     icon = GetMetricIcon(MetricTitle.GreenSpace)
                // },
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

