using System.Collections.Generic;

namespace GreenCityBuilder.Missions
{
    public static class MissionRepository
    {
        public static List<Mission> AllMissions { get; } = new List<Mission>
        {
            new ParksAndRecMission(),
            new CoolTheCityDownMission(),
            new GreenSpaceRenaissanceMission(),
            new BudgetBalanceMission(),
            new PollutionControlMission(),
            new FreePlayMission(),
        };

        public static Mission GetMissionByName(string name)
        {
            return AllMissions.Find(m => m.missionName == name);
        }
    }
}
