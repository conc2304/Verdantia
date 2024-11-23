using System.Collections.Generic;

public static class MetricUnits
{
    public enum UnitPosition
    {
        Before,
        After
    }

    // Dictionary to store unit information and position for each metric
    public static readonly Dictionary<MetricTitle, (string unit, UnitPosition position)> Units = new Dictionary<MetricTitle, (string, UnitPosition)>
    {
        { MetricTitle.Budget, ("$", UnitPosition.Before) },
        { MetricTitle.Energy, ("kW", UnitPosition.After) },
        { MetricTitle.CarbonEmission, ("kg", UnitPosition.After) },
        { MetricTitle.Population, ("", UnitPosition.After) },
        { MetricTitle.Happiness, ("%", UnitPosition.After) },
        { MetricTitle.GreenSpace, ("%", UnitPosition.After) },
        { MetricTitle.UrbanHeat, ("", UnitPosition.After) },
        { MetricTitle.Pollution, ("%", UnitPosition.After) },
        { MetricTitle.CityTemperature, ("°F", UnitPosition.After) },
        { MetricTitle.Revenue, ("$", UnitPosition.Before) },
    };

    // Method to get the unit for a given metric title
    public static string GetUnit(MetricTitle metricTitle)
    {
        return Units.TryGetValue(metricTitle, out var unitInfo) ? unitInfo.unit : "";
    }

    // Method to get the unit position for a given metric title
    public static UnitPosition GetUnitPosition(MetricTitle metricTitle)
    {
        return Units.TryGetValue(metricTitle, out var unitInfo) ? unitInfo.position : UnitPosition.After;
    }
}
