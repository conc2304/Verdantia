using System;
using System.Collections.Generic;

public class MetricMapping
{
    private static readonly Dictionary<BuildingMetric, MetricTitle> metricMapping = new Dictionary<BuildingMetric, MetricTitle>
    {
        { BuildingMetric.heatContribution, MetricTitle.UrbanHeat },
        { BuildingMetric.cityRevenue, MetricTitle.Revenue },
        { BuildingMetric.netEnergy, MetricTitle.Energy },
        { BuildingMetric.capacity, MetricTitle.Population },
        { BuildingMetric.happinessImpact, MetricTitle.Happiness },
        { BuildingMetric.pollutionImpact, MetricTitle.Pollution },
        { BuildingMetric.greenSpaceEffect, MetricTitle.GreenSpace },
        { BuildingMetric.carbonFootprint, MetricTitle.CarbonEmission }
    };

    public static MetricTitle? GetMetricTitle(BuildingMetric metric)
    {
        if (metricMapping.TryGetValue(metric, out MetricTitle title))
        {
            return title;
        }
        return null; // Return null for unmatched metrics
    }

    // Mapping for metrics that should be inverted
    public static readonly Dictionary<MetricTitle, bool> ShouldInvertMetric = new Dictionary<MetricTitle, bool>
    {
        { MetricTitle.CityTemperature, false },  // TODO evaluate this
        { MetricTitle.Population, false },
        { MetricTitle.Happiness, false },
        { MetricTitle.Budget, false },
        { MetricTitle.GreenSpace, false },
        { MetricTitle.UrbanHeat, true },
        { MetricTitle.Pollution, true },
        { MetricTitle.Energy, false },
        { MetricTitle.CarbonEmission, true },
        { MetricTitle.Revenue, false }
    };

    private static readonly Dictionary<BuildingMetric, bool> ShouldInvertBuildingMetric = new Dictionary<BuildingMetric, bool>
    {
        { BuildingMetric.heatContribution, true },    // Higher heat contribution is bad
        { BuildingMetric.capacity, false },           // Higher capacity (population) is good
        { BuildingMetric.happinessImpact, false },    // Higher happiness is good
        { BuildingMetric.cityRevenue, false },        // Higher revenue is good
        { BuildingMetric.greenSpaceEffect, false },   // More green space is good
        { BuildingMetric.pollutionImpact, true },     // Higher pollution impact is bad
        { BuildingMetric.netEnergy, false },          // Higher net energy is good
        { BuildingMetric.carbonFootprint, true }      // Higher carbon footprint is bad
    };

    // Dictionary to map strings to BuildingMetric
    private static readonly Dictionary<string, BuildingMetric> StringToBuildingMetric = new Dictionary<string, BuildingMetric>(StringComparer.OrdinalIgnoreCase)
    {
        { "heatContribution", BuildingMetric.heatContribution },
        { "capacity", BuildingMetric.capacity },
        { "happinessImpact", BuildingMetric.happinessImpact },
        { "cityRevenue", BuildingMetric.cityRevenue },
        { "greenSpaceEffect", BuildingMetric.greenSpaceEffect },
        { "pollutionImpact", BuildingMetric.pollutionImpact },
        { "netEnergy", BuildingMetric.netEnergy },
        { "carbonFootprint", BuildingMetric.carbonFootprint }
    };

    // Method to determine if a metric's scale should be inverted
    public static bool CityMetricIsInverted(MetricTitle metricTitle)
    {
        return ShouldInvertMetric.TryGetValue(metricTitle, out bool isInverted) && isInverted;
    }

    public static bool BuildingMetricIsInverted(BuildingMetric buildingMetric)
    {
        return ShouldInvertBuildingMetric.TryGetValue(buildingMetric, out bool isInverted) && isInverted;
    }

    public static BuildingMetric? GetBuildingMetric(string metricString)
    {
        if (string.IsNullOrWhiteSpace(metricString))
        {
            return null; // Handle null or empty input gracefully
        }

        if (StringToBuildingMetric.TryGetValue(metricString, out BuildingMetric buildingMetric))
        {
            return buildingMetric;
        }

        return null; // Return null if no match is found
    }


}
