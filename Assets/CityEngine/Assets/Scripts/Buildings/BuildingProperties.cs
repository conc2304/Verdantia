using System.Collections.Generic;
using UnityEngine;

public enum BuildingMetric
{
    constructionCost,
    demolitionCost,
    heatContribution,
    cityRevenue,
    netEnergy,
    capacity,
    happinessImpact,
    pollutionImpact,
    greenSpaceEffect,
    carbonFootprint,
    effectRadius,
};


// Serializable class to define metric boosts
[System.Serializable]
public class MetricBoost
{
    public BuildingMetric metricName; // The name of the metric (e.g., "happinessImpact", "taxRevenue")
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


    [Header("Simulation Properties")]
    [Space(4)]
    [Header("General Building Properties")]

    public string buildingName;
    [TextArea]
    public string buildingDescription;
    public int constructionCost;
    public int demolitionCost;

    // Resource Management Properties
    [Header("Resource Management Properties")]

    public int cityRevenue;
    public int netEnergy;

    // Population Dynamics Properties
    [Header("Population Dynamics Properties")]
    public int capacity; // For population metrics
    public int happinessImpact; // Positive or negative impact on population happiness

    [Header("Environmental Impact Properties")]
    public int pollutionImpact;  // Positive or negative impact on pollution levels
    public int heatContribution; // Contribution to urban heat island effect
    public int greenSpaceEffect; // Benefits of green space
    public int carbonFootprint; // Carbon emissions of this building
    public List<MetricBoost> proximityEffects;
    public int effectRadius; // The effective radius of this building's service

    // Props for data driven simulation metrics 
    public readonly string[] dataProps;


    private BuildingsMenuNew buildingsMenu;


    void Start()
    {
        buildingsMenu = FindObjectOfType<BuildingsMenuNew>();
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
                        buildingProperties.netEnergy = netEnergy;
                        buildingProperties.capacity = capacity;
                        buildingProperties.happinessImpact = happinessImpact;
                        buildingProperties.pollutionImpact = pollutionImpact;
                        buildingProperties.heatContribution = heatContribution;
                        buildingProperties.greenSpaceEffect = greenSpaceEffect;
                        buildingProperties.carbonFootprint = carbonFootprint;
                        buildingProperties.effectRadius = effectRadius;
                    }
                }
            }
        }
    }

}


