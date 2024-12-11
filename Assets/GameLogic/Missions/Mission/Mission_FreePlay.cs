using System.Collections.Generic;
using UnityEngine;

/**
Represents a sandbox mode in the Green City Builder game, where players can build and manage their city without any objectives or time constraints. 
It encourages experimentation with different designs, layouts, and strategies. 
The mission includes a starting budget of $950,000 and no defined metrics or objectives, providing players with complete creative freedom.
**/
namespace GreenCityBuilder.Missions
{
    public class FreePlayMission : Mission
    {
        public FreePlayMission(Dictionary<MetricTitle, Sprite> metricIcons) : base(metricIcons)
        {
            missionName = "Free Play Mode";
            missionObjective = "Enjoy a limitless city-building experience without objectives or time constraints.";
            missionBrief = "Build and manage the city at your pace. Experiment with different designs and layouts, manage your budget, and watch the city grow.";
            // missionMetrics = "You decide what is important!";
            timeLimitInMonths = 0;
            missionIcon = Resources.Load<Sprite>("Missions/mission_icon-free_play");
            difficulty = DifficultyLevel.Medium;
            startingBudget = 950000; // TODO Set a reasonable starting budget
            objectives = new MissionObjective[0];
            missionCityFileName = null;
        }
    }
}
