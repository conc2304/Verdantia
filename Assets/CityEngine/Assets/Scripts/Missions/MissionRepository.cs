using System.Collections.Generic;
using UnityEngine;

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
            { MetricTitle.GreenSpace, Resources.Load<Sprite>("MetricIcons/green-space") },
            { MetricTitle.UrbanHeat, Resources.Load<Sprite>("MetricIcons/urban-heat") },
            { MetricTitle.Pollution, Resources.Load<Sprite>("MetricIcons/pollution") },
            { MetricTitle.Energy, Resources.Load<Sprite>("MetricIcons/energy") },
            { MetricTitle.CarbonEmission, Resources.Load<Sprite>("MetricIcons/carbon-emission") },
            { MetricTitle.Revenue, Resources.Load<Sprite>("MetricIcons/revenue") },
            // { MetricTitle.Income, Resources.Load<Sprite>("MetricIcons/income") },
            // { MetricTitle.Expenses, Resources.Load<Sprite>("MetricIcons/expenses") }

        };
        public static List<Mission> AllMissions { get; } = new List<Mission>
        {
            new ParksAndRecMission(metricIcons),
            new CoolTheCityDownMission(metricIcons),
            new GreenSpaceRenaissanceMission(metricIcons),
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
