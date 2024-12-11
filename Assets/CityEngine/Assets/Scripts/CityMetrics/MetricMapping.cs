using System;
using System.Collections.Generic;
using UnityEngine;

/**
This file manages mappings and utilities for metrics in the system. It provides:

Metric Mapping: Links BuildingMetric enums to corresponding MetricTitle enums for consistent usage across the game.
Inversion Logic: Identifies which metrics should be displayed inversely (e.g., higher values being bad).
String-to-Metric Conversion: Converts string representations of metrics into their corresponding BuildingMetric enum.
Metric Colors: Assigns distinct colors to each metric for visual representation.
Heat Map Inversion Check: Determines if a metric or building metric's values should be inverted for heatmap rendering.

This facilitates dynamic metric handling, visualization, and consistent logic across different game systems.
**/
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
        // { BuildingMetric.greenSpaceEffect, MetricTitle.GreenSpace },
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
        { MetricTitle.CityTemperature, false },
        { MetricTitle.Population, false },
        { MetricTitle.Happiness, false },
        { MetricTitle.Budget, false },
        // { MetricTitle.GreenSpace, false },
        { MetricTitle.UrbanHeat, true },
        { MetricTitle.Pollution, true },
        { MetricTitle.Energy, false },
        { MetricTitle.CarbonEmission, true },
        { MetricTitle.Revenue, false }
    };

    private static readonly Dictionary<BuildingMetric, bool> ShouldInvertBuildingMetric = new Dictionary<BuildingMetric, bool>
    {
        { BuildingMetric.heatContribution, false },    // Higher values are hot (yellow)
        { BuildingMetric.capacity, false },           // Higher capacity (population) is good
        { BuildingMetric.happinessImpact, false },    // Higher happiness is good
        { BuildingMetric.cityRevenue, true },        // Higher revenue is good
        // { BuildingMetric.greenSpaceEffect, true },   // More green space is good
        { BuildingMetric.pollutionImpact, false },     // Higher pollution impact is bad
        { BuildingMetric.netEnergy, true },          // Higher net energy is good
        { BuildingMetric.carbonFootprint, false }      // Higher carbon footprint is bad
    };

    // Dictionary to map strings to BuildingMetric
    private static readonly Dictionary<string, BuildingMetric> StringToBuildingMetric = new Dictionary<string, BuildingMetric>(StringComparer.OrdinalIgnoreCase)
    {
        { "heatContribution", BuildingMetric.heatContribution },
        { "capacity", BuildingMetric.capacity },
        { "happinessImpact", BuildingMetric.happinessImpact },
        { "cityRevenue", BuildingMetric.cityRevenue },
        // { "greenSpaceEffect", BuildingMetric.greenSpaceEffect },
        { "pollutionImpact", BuildingMetric.pollutionImpact },
        { "netEnergy", BuildingMetric.netEnergy },
        { "carbonFootprint", BuildingMetric.carbonFootprint }
    };

    public static Color GetMetricColor(MetricTitle metricTitle)
    {
        // Generate a unique color for each metric 
        switch (metricTitle)
        {
            case MetricTitle.CityTemperature: return Color.red;
            case MetricTitle.UrbanHeat: return Color.yellow;
            // case MetricTitle.GreenSpace: return Color.green;
            case MetricTitle.Budget: return Color.blue;
            case MetricTitle.Happiness: return Color.magenta;
            case MetricTitle.Pollution: return Color.gray;
            case MetricTitle.Population: return Color.cyan;
            case MetricTitle.CarbonEmission: return new Color(1.0f, 0.5f, 0.0f); // orange
            default: return Color.white;
        }
    }

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
