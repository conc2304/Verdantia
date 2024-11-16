using System;
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
    public bool unrestrictedPlacement = false;  // Allows building to not be built next to roads or chainable buildings (ie allow forest placement anywhere)
    public bool allowChaining = false;  // Allows building to be placed not along roads if chaining
    public List<Transform> chainableTypes; // List of building types that item can chain off of

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
    public int effectRadius; // The effective radius of this building's service
    public List<MetricBoost> proximityEffects;


    private BuildingsMenuNew buildingsMenu;
    private CameraController cameraController;



    void Start()
    {
        buildingsMenu = FindObjectOfType<BuildingsMenuNew>();
        cameraController = FindObjectOfType<CameraController>();
        demolitionCost = demolitionCost != 0 ? demolitionCost : (int)(constructionCost * 0.25f);
        PassonBuildingProperties();
        effectRadius++; // boost all of them by 1
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
                        buildingProperties.proximityEffects = proximityEffects;
                    }
                }
            }
        }
    }

    public void ApplyProximityEffects(List<Transform> allBuildings = null)
    {
        allBuildings ??= FindObjectOfType<CameraController>().GetAllBuildings();
        // print($"Attempt ApplyProximityEffects from {buildingName}");
        // print($"Building Count: {allBuildings.Count}");

        foreach (Transform buildingTransform in allBuildings)
        {
            if (buildingTransform != transform && (buildingTransform.CompareTag("Building") || buildingTransform.CompareTag("Road"))) // Skip self, Skip "Spaces" and anything not a "Building"
            {
                // print($"{buildingTransform.name} : {buildingTransform.tag}");
                BuildingProperties building = buildingTransform.GetComponent<BuildingProperties>();
                if (building != null && IsWithinProximity(building))
                {
                    foreach (MetricBoost boost in proximityEffects)
                    {
                        // print($"{building.buildingName} APPLY BOOST TO {building.buildingName}");
                        ApplyBoost(building, boost);
                    }
                }
            }
        }
    }

    public void RemoveProximityEffects()
    {

        foreach (Transform buildingTransform in cameraController.GetAllBuildings())
        {
            if (buildingTransform != transform) // Skip self
            {
                BuildingProperties building = buildingTransform.GetComponent<BuildingProperties>();
                if (building != null && IsWithinProximity(building))
                {
                    foreach (MetricBoost boost in proximityEffects)
                    {
                        // print($"{building.buildingName} REMOVE BOOST FROM {building.buildingName}");
                        RemoveBoost(building, boost);
                    }
                }
            }
        }
    }

    public bool IsWithinProximity(BuildingProperties other)
    {
        // print($"PROXIMITY TEST: {buildingName} and {other.buildingName} ");

        // Check if this building's position is within the effect radius of the other building
        if (IsPositionsClose(transform.position, other.transform.position))
        {
            return true;
        }

        // Check additional spaces of this building against the other building's position
        foreach (Transform space in additionalSpace)
        {
            if (IsPositionsClose(space.position, other.transform.position))
            {
                return true;
            }
        }

        // Check additional spaces of the other building against this building's position
        foreach (Transform otherSpace in other.additionalSpace)
        {
            if (IsPositionsClose(transform.position, otherSpace.position))
            {
                return true;
            }
        }

        // Check additional spaces of this building against additional spaces of the other building
        foreach (Transform space in additionalSpace)
        {
            foreach (Transform otherSpace in other.additionalSpace)
            {
                if (IsPositionsClose(space.position, otherSpace.position))
                {
                    return true;
                }
            }
        }

        // If none of the checks found a proximity, return false
        return false;
    }

    public void ApplyBoost(BuildingProperties targetBuilding, MetricBoost boost)
    {
        // Get the property info for the specified metric name
        var propertyInfo = typeof(BuildingProperties).GetProperty(boost.metricName.ToString());
        if (propertyInfo != null && propertyInfo.CanWrite)
        {
            // print($"{buildingName} APPLY boost {boost.metricName} at {boost.boostValue} to {targetBuilding.buildingName}");
            // Get the current value and apply the boost
            int currentValue = (int)propertyInfo.GetValue(targetBuilding);
            propertyInfo.SetValue(targetBuilding, currentValue + boost.boostValue);
        }

        targetBuilding.PassonBuildingProperties();
    }

    public void RemoveBoost(BuildingProperties targetBuilding, MetricBoost boost)
    {
        // Get the property info for the specified metric name
        var propertyInfo = typeof(BuildingProperties).GetProperty(boost.metricName.ToString());
        if (propertyInfo != null && propertyInfo.CanWrite)
        {
            // Get the current value and remove the boost
            // print($"{buildingName} REMOVE boost {boost.metricName} at {boost.boostValue} to {targetBuilding.buildingName}");

            int currentValue = (int)propertyInfo.GetValue(targetBuilding);
            propertyInfo.SetValue(targetBuilding, currentValue - boost.boostValue);
        }

        targetBuilding.PassonBuildingProperties();
    }

    private bool IsPositionsClose(Vector3 positionA, Vector3 positionB)
    {
        int gridSize = 10;
        // print("IsPositionsClose");
        // Round the positions to the nearest grid point
        float roundedX_A = Mathf.Round(positionA.x / gridSize);
        float roundedZ_A = Mathf.Round(positionA.z / gridSize);
        float roundedX_B = Mathf.Round(positionB.x / gridSize);
        float roundedZ_B = Mathf.Round(positionB.z / gridSize);

        // Check if the rounded positions are equal
        if (roundedX_A == roundedX_B && roundedZ_A == roundedZ_B)
        {
            return true;
        }

        // Check distance to ensure they are within the effect radius
        float distance = Vector3.Distance(positionA, positionB) / gridSize;
        // print(distance + " vs " + effectRadius + " | " + (distance <= effectRadius));
        return distance <= effectRadius;
    }

    public Vector3 GetBuildingPopUpPlacement()
    {
        // target.TryGetComponent<BuildingProperties>(out BuildingProperties properties);

        float xTotal = transform.position.x;
        float zTotal = transform.position.z;
        int count = 1;

        // get the center of the building based on its additional spaces
        foreach (Transform additionalSpace in additionalSpace)
        {
            xTotal += additionalSpace.position.x;
            zTotal += additionalSpace.position.z;
            count++;
        }

        float yPos = (buildingHigh + 1) * 10;
        float xPos = xTotal / count;
        float zPos = zTotal / count;

        return new Vector3(xPos, yPos, zPos);
    }
}


