using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

[System.Serializable]
public class IndustryBoost
{
    public BuildingProperties.IndustryType industryType; // The industry type
    public List<MetricBoost> metricBoosts; // List of metrics this industry affects
}

// Serializable class to define metric boosts
[System.Serializable]
public class MetricBoost
{
    public string metricName; // The name of the metric (e.g., "happinessImpact", "taxRevenue")
    public int boostValue;    // The value by which this metric is boosted
}

public class BuildingProperties : MonoBehaviour
{

    public int buildingIndex = -1;

    public GameObject environment;

    public bool connectToRoad;
    public PathTarget[] carsPathTargetsToConnect;
    public Transform[] carsPathTargetsToSpawn;
    public PathTarget[] citizensPathTargetsToConnect;
    public Transform[] citizensPathTargetsToSpawn;

    public Transform[] additionalSpace;

    public BuildConstruction buildConstruction;
    public float buildingHigh = 1;
    public float buildingTime = 10;

    public int spaceWidth = 1;



    // Simulation Driven/Driving Stats

    public enum ZoneType
    {
        Residential,
        Commercial,
        Community,
        Industrial,
        Infrastructure,
        Park
    }
    public enum IndustryType
    {
        Entertainment,
        Technology,
        Manufacturing,
        Agriculture,
        Education,
        Healthcare
    }
    [Header("Simulation Properties")]
    [Space(4)]
    [Header("General Building Properties")]

    public string buildingName;
    [TextArea]
    public string buildingDescription;
    public ZoneType zoneRequirement;
    public int constructionCost;
    public int demolitionCost;

    // Resource Management Properties
    [Header("Resource Management Properties")]
    public int income;
    public int expense;
    public int energyConsumption;
    public int resourceProduction; // If applicable, for resource-producing buildings

    // Population Dynamics Properties
    [Header("Population Dynamics Properties")]
    public int capacity; // Number of people that can live or work in this building
    public int happinessImpact; // Positive or negative impact on population happiness

    [Header("Environmental Impact Properties")]
    public int pollutionImpact;
    public int heatContribution; // Contribution to urban heat island effect
    public int greenSpaceEffect; // Benefits of green space
    public int carbonFootprint; // Carbon emissions of this building
    public int coverageRadius; // The effective radius of this building's service

    // Props for data driven simulation metrics 
    public readonly string[] dataProps ={
        "constructionCost",
        "demolitionCost",
        "heatContribution",
        "income",
        "expense",
        "energyConsumption",
        "resourceProduction",
        "capacity",
        "happinessImpact",
        "pollutionImpact",
        "heatContribution",
        "greenSpaceEffect",
        "carbonFootprint",
        "coverageRadius",
    };

    private BuildingsMenuNew buildingsMenu;


    void Start()
    {
        buildingsMenu = FindObjectOfType<BuildingsMenuNew>();

        // heatContribution /= 1 + additionalSpace.Length;

        demolitionCost = demolitionCost != 0 ? demolitionCost : (int)(constructionCost * 0.25f);
        PassonBuildingProperties();
    }


    private void PassonBuildingProperties()
    {
        if (gameObject.CompareTag("Building"))
        {
            BuildingProperties buildingProps = GetComponent<BuildingProperties>();
            Transform[] additionalSpace = buildingProps.additionalSpace;
            // Make all "Additional Spaces" inherit all properties from the Building Parent
            if (additionalSpace != null && additionalSpace.Length > 0)
            {
                // Loop through each Transform in the additionalSpace array
                for (int i = 0; i < additionalSpace.Length; i++)
                {
                    Transform space = additionalSpace[i];

                    if (space != null && space.CompareTag("Space") && space.TryGetComponent<BuildingProperties>(out BuildingProperties buildingProperties)) // Ensure the Transform is not null
                    {
                        buildingProperties.constructionCost = constructionCost;
                        buildingProperties.demolitionCost = demolitionCost;
                        buildingProperties.heatContribution = heatContribution;
                        buildingProperties.income = income;
                        buildingProperties.expense = expense;
                        buildingProperties.energyConsumption = energyConsumption;
                        buildingProperties.resourceProduction = resourceProduction;
                        buildingProperties.capacity = capacity;
                        buildingProperties.happinessImpact = happinessImpact;
                        buildingProperties.pollutionImpact = pollutionImpact;
                        buildingProperties.heatContribution = heatContribution;
                        buildingProperties.greenSpaceEffect = greenSpaceEffect;
                        buildingProperties.carbonFootprint = carbonFootprint;
                        buildingProperties.coverageRadius = coverageRadius;
                    }
                }
            }
        }
    }

}


