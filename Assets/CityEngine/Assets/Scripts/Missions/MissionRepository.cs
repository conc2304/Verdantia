using System.Collections.Generic;
using UnityEngine;

/**
MissionRepository class serves as a centralized repository for managing missions and their associated icons in the Green City Builder game.

Metric Icons: A dictionary linking MetricTitle values to corresponding sprites, loaded from resources.
AllMissions: A list containing all available missions, instantiated with their relevant metric icons.
GetMissionByName: A method to retrieve a mission by its name.
This simplifies accessing missions and their assets.
**/

namespace GreenCityBuilder.Missions
{
    public static class MissionRepository
    {
        public static readonly Dictionary<MetricTitle, Sprite> metricIcons = new Dictionary<MetricTitle, Sprite>
        {
            { MetricTitle.CityTemperature, Resources.Load<Sprite>("MetricIcons/city-temperature") },
            { MetricTitle.Population, Resources.Load<Sprite>("MetricIcons/population") },
            { MetricTitle.Happiness, Resources.Load<Sprite>("MetricIcons/happiness") },
            { MetricTitle.Budget, Resources.Load<Sprite>("MetricIcons/budget") },
            // { MetricTitle.GreenSpace, Resources.Load<Sprite>("MetricIcons/green-space") },
            { MetricTitle.UrbanHeat, Resources.Load<Sprite>("MetricIcons/urban-heat") },
            { MetricTitle.Pollution, Resources.Load<Sprite>("MetricIcons/pollution") },
            { MetricTitle.Energy, Resources.Load<Sprite>("MetricIcons/energy") },
            { MetricTitle.CarbonEmission, Resources.Load<Sprite>("MetricIcons/carbon-emission") },
            { MetricTitle.Revenue, Resources.Load<Sprite>("MetricIcons/revenue") },


        };
        public static List<Mission> AllMissions { get; } = new List<Mission>
        {
            new ParksAndRecMission(metricIcons),
            new CoolTheCityDownMission(metricIcons),
            // new GreenSpaceRenaissanceMission(metricIcons),  // todo update missions without greenspace
            new BudgetBalanceMission(metricIcons),
            new PollutionControlMission(metricIcons),
            new FreePlayMission(metricIcons),
        };

        public static Mission GetMissionByName(string name)
        {
            return AllMissions.Find(m => m.missionName == name);
        }
    }
}
