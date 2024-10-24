using System;
using System.Collections.Generic;
using UnityEngine;

public class CityMetricsManager : MonoBehaviour
{

    // public static CityMetricsManager Instance { get; private set; }

    // Global city metrics
    public int startingBudget = 1000;
    // public int startingBudget = 500000;
    public float startingTemp = 72.0f;
    private string tempSuffix = "°F";
    public float cityTemp = 72.0f;
    public int population { get; private set; }
    public float happiness { get; private set; } // Store as float for calculation
    public int budget { get; private set; }
    public float greenSpace { get; private set; }
    public int urbanHeat { get; private set; }
    public int pollution { get; private set; }
    public int energy { get; private set; }
    public int carbonEmission { get; private set; }
    public int revenue { get; private set; }
    public int income { get; private set; }
    public int expenses { get; private set; }
    public CameraController cameraController;

    // Time-keeping variables
    public int currentMonth = 1; // January starts as 1
    public int currentYear = 2024; // Set the starting year
    public float monthDuration = 30.0f; // Duration of a "month" in seconds (for testing purposes)
    private float monthTimer = 0f;

    public event Action<int, int> OnTimeUpdated; // (month, year)
    public event Action OnMetricsUpdate;
    public event Action OnTempUpdated;


    void Start()
    {
        budget = startingBudget;
        UpdateCityMetrics();
        OnMetricsUpdate?.Invoke();
    }


    void Update()
    {
        // Accumulate budget monthly
        monthTimer += Time.deltaTime;
        if (monthTimer >= monthDuration)
        {
            // Every month, calculate the city's financial status
            UpdateMonthlybudget();
            AdvanceMonth();
            monthTimer = 0f;
        }
    }

    void AdvanceMonth()
    {
        // Advance the month
        currentMonth++;

        // Check if we've reached the end of the year
        if (currentMonth > 12)
        {
            currentMonth = 1; // Reset to January
            currentYear++; // Move to the next year
        }

        // Notify any systems 
        OnMetricsUpdate?.Invoke();
        OnTimeUpdated?.Invoke(currentMonth, currentYear);
    }

    // Update budget based on revenue and expenses (done every "month")
    private void UpdateMonthlybudget()
    {
        // Calculate income from tax revenue
        int monthlyIncome = income;

        // Calculate expenses (operational cost and upkeep of all buildings)
        int monthlyExpenses = expenses;

        // Update the budget by subtracting expenses from income
        budget += monthlyIncome - monthlyExpenses;
    }

    // Update city metrics based on all buildings
    public void UpdateCityMetrics()
    {
        // Reset all metrics before recalculating them
        ResetMetrics();

        foreach (Transform building in cameraController.allBuildings)
        {
            BuildingProperties buildingProps = building.GetComponent<BuildingProperties>();
            population += buildingProps.capacity;
            happiness += buildingProps.happinessImpact;
            budget -= buildingProps.operationalCost; // Decrease budget based on operational costs
            budget += buildingProps.taxRevenue; // Increase budget by tax revenue
            greenSpace += buildingProps.greenSpaceEffect;
            urbanHeat += buildingProps.heatContribution;
            pollution += buildingProps.pollutionOutput - buildingProps.pollutionReduction; // Net pollution after reduction
            energy += buildingProps.resourceProduction - buildingProps.energyConsumption;
            carbonEmission += buildingProps.carbonFootprint;
            revenue += buildingProps.taxContribution;
            income += buildingProps.taxRevenue;
            expenses += buildingProps.upkeep;
        }

        // Adjust happiness to be averaged over all buildings
        happiness = cameraController.allBuildings.Count > 0 ? (happiness / cameraController.allBuildings.Count) : 0;
        happiness = (float)Math.Truncate((double)happiness * 100 / 100);

        // If greenSpace is in percentage (e.g., out of 100% max coverage), you can adjust based on area.
        // Adjust accordingly depending on game design.
    }

    // Method to reset all metrics to initial state before recalculation
    private void ResetMetrics()
    {
        population = 0;
        happiness = 0;
        budget = 0;
        greenSpace = 0;
        urbanHeat = 0;
        pollution = 0;
        energy = 0;
        carbonEmission = 0;
        revenue = 0;
        income = 0;
        expenses = 0;
    }

    // Example method to manually add income to the city
    public void AddRevenue(int amount)
    {
        budget += amount;
        Debug.Log($"Revenue added: {amount}, New budget: {budget}");
    }

    // Example method to manually deduct expenses from the city
    public void DeductExpenses(int amount)
    {
        budget -= amount;
        Debug.Log($"Expenses deducted: {amount}, New budget: {budget}");
    }
}
