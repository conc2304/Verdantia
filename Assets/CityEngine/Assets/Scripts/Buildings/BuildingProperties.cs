﻿using System.Collections;
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
    public ZoneType zoneRequirement;
    public int constructionCost;
    public int demolitionCost;
    public int operationalCost;
    public int taxRevenue;
    public int upkeep;

    // Resource Management Properties
    [Header("Resource Management Properties")]

    public int energyConsumption;
    public int waterConsumption;
    public int wasteProduction;
    public int resourceProduction; // If applicable, for resource-producing buildings

    // Population Dynamics Properties
    [Header("Population Dynamics Properties")]

    public int capacity; // Number of people that can live or work in this building
    public int jobsProvided; // Number of jobs provided by commercial/industrial buildings
    public int happinessImpact; // Positive or negative impact on population happiness
    public int healthImpact; // Effect on population health
    public int educationImpact; // Effect on education levels


    [Header("Environmental Impact Properties")]
    public int pollutionOutput; // How much pollution this building generates
    public int pollutionReduction; // How much this building reduces pollution (for parks, etc.)
    public int heatContribution; // Contribution to urban heat island effect
    public int greenSpaceEffect; // Benefits of green space
    public int carbonFootprint; // Carbon emissions of this building

    [Header("Economic Properties")]
    public int taxContribution; // Amount of tax generated by this building
    public int jobCreation; // Number of jobs created by this building

    [Header("Industry Boosts")]
    public List<IndustryBoost> industryBoosts; // List of industry boosts, configurable in the editor

    public int coverageRadius; // The effective radius of this building's service

    // Props for data driven simulation metrics 
    public readonly string[] dataProps ={
        "constructionCost",
        "demolitionCost",
        "operationalCost",
        "taxRevenue",
        "upkeep",
        "energyConsumption",
        "waterConsumption",
        "wasteProduction",
        "resourceProduction",
        "capacity",
        "jobsProvided",
        "happinessImpact",
        "healthImpact",
        "educationImpact",
        "pollutionOutput",
        "pollutionReduction",
        "heatContribution",
        "greenSpaceEffect",
        "carbonFootprint",
        "taxContribution",
        "jobCreation",
        "industryBoost",
        "coverageRadius",
    };

    private BuildingsMenuNew buildingsMenu;


    void Start()
    {
        buildingsMenu = FindObjectOfType<BuildingsMenuNew>();
        PassonBuildingProperties();
        demolitionCost = demolitionCost != null && demolitionCost != 0 ? demolitionCost : (int)(constructionCost * 0.25f);
        ApplyBoostToNearbyBuildings();
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
                        buildingProperties.heatContribution = heatContribution;
                        buildingProperties.zoneRequirement = zoneRequirement;
                        buildingProperties.constructionCost = constructionCost;
                        buildingProperties.demolitionCost = demolitionCost;
                        buildingProperties.operationalCost = operationalCost;
                        buildingProperties.taxRevenue = taxRevenue;
                        buildingProperties.upkeep = upkeep;
                        buildingProperties.energyConsumption = energyConsumption;
                        buildingProperties.waterConsumption = waterConsumption;
                        buildingProperties.wasteProduction = wasteProduction;
                        buildingProperties.resourceProduction = resourceProduction;
                        buildingProperties.capacity = capacity;
                        buildingProperties.jobsProvided = jobsProvided;
                        buildingProperties.happinessImpact = happinessImpact;
                        buildingProperties.healthImpact = healthImpact;
                        buildingProperties.educationImpact = educationImpact;
                        buildingProperties.pollutionOutput = pollutionOutput;
                        buildingProperties.pollutionReduction = pollutionReduction;
                        buildingProperties.heatContribution = heatContribution;
                        buildingProperties.greenSpaceEffect = greenSpaceEffect;
                        buildingProperties.carbonFootprint = carbonFootprint;
                        buildingProperties.taxContribution = taxContribution;
                        buildingProperties.jobCreation = jobCreation;
                        buildingProperties.industryBoosts = industryBoosts;
                        buildingProperties.coverageRadius = coverageRadius;
                    }
                }
            }
        }
    }

    // Apply the industry boost to buildings within the coverage radius
    public void ApplyBoostToNearbyBuildings()
    {
        Collider[] nearbyBuildings = Physics.OverlapSphere(transform.position, coverageRadius);

        foreach (Collider buildingCollider in nearbyBuildings)
        {
            BuildingProperties nearbyBuilding = buildingCollider.GetComponent<BuildingProperties>();

            if (nearbyBuilding != null && nearbyBuilding != this)
            {
                // Apply all the industry boosts
                foreach (IndustryBoost boost in industryBoosts)
                {
                    foreach (MetricBoost metricBoost in boost.metricBoosts)
                    {
                        ApplyBoostToBuilding(nearbyBuilding, metricBoost);
                        nearbyBuilding.PassonBuildingProperties();
                    }
                }
            }
        }
    }

    // Remove the industry boost from buildings within the coverage radius
    public void RemoveBoostFromNearbyBuildings()
    {
        Collider[] nearbyBuildings = Physics.OverlapSphere(transform.position, coverageRadius);

        foreach (Collider buildingCollider in nearbyBuildings)
        {
            BuildingProperties nearbyBuilding = buildingCollider.GetComponent<BuildingProperties>();

            if (nearbyBuilding != null && nearbyBuilding != this)
            {
                // Remove all the industry boosts
                foreach (IndustryBoost boost in industryBoosts)
                {
                    foreach (MetricBoost metricBoost in boost.metricBoosts)
                    {
                        RemoveBoostFromBuilding(nearbyBuilding, metricBoost);
                        nearbyBuilding.PassonBuildingProperties();
                    }
                }
            }
        }
    }

    void OnDestroy()
    {
        RemoveBoostFromNearbyBuildings();
    }

    // Apply boost to the specified building
    private void ApplyBoostToBuilding(BuildingProperties building, MetricBoost boost)
    {
        FieldInfo field = building.GetType().GetField(boost.metricName);
        if (field != null && field.FieldType == typeof(int))
        {
            int currentValue = (int)field.GetValue(building);
            field.SetValue(building, currentValue + boost.boostValue);
            Debug.Log($"{building.buildingName} {boost.metricName} increased by {boost.boostValue}.");
        }
    }

    // Remove boost from the specified building
    private void RemoveBoostFromBuilding(BuildingProperties building, MetricBoost boost)
    {
        FieldInfo field = building.GetType().GetField(boost.metricName);
        if (field != null && field.FieldType == typeof(int))
        {
            int currentValue = (int)field.GetValue(building);
            field.SetValue(building, currentValue - boost.boostValue);
            Debug.Log($"{building.buildingName} {boost.metricName} decreased by {boost.boostValue}.");
        }
    }



    private Dictionary<string, (int min, int max)> GetPropertyRanges()
    {
        Dictionary<string, (int min, int max)> propertyRanges = buildingsMenu.GetPropertyRanges();
        return propertyRanges;
    }
}


