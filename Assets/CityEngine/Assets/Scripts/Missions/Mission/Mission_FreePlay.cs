using UnityEngine;

namespace GreenCityBuilder.Missions
{
    public class FreePlayMission : Mission
    {
        public FreePlayMission()
        {
            missionName = "Free Play Mode";
            missionObjective = "Enjoy a limitless city-building experience without objectives or time constraints.";
            missionBrief = "Build and manage the city at your pace. Experiment with different designs and layouts, manage your budget, and watch the city grow.";
            missionMetrics = "You decide what is important!";
            timeLimitInMonths = 0;
            missionIcon = Resources.Load<Sprite>("Missions/mission_icon-free_play");
            difficulty = DifficultyLevel.Medium;
            startingBudget = 750000; // TODO Set a reasonable starting budget
            objectives = new MissionObjective[0];
        }
    }
}
