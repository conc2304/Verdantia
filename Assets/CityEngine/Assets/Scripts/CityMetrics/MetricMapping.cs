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

    // Method to determine if a metric's scale should be inverted
    public static bool IsInverted(MetricTitle metricTitle)
    {
        return ShouldInvertMetric.TryGetValue(metricTitle, out bool isInverted) && isInverted;
    }
}
