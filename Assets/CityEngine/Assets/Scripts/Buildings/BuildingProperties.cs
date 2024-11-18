using System;
using System.Collections.Generic;
using System.Reflection;
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
[Serializable]
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
    public GameObject floatingValuePrefab;


    private CameraController cameraController;
    private Vector3 popupPlacement;
    private int gridSize = 10;



    void Start()
    {
        cameraController = FindObjectOfType<CameraController>();
        demolitionCost = demolitionCost != 0 ? demolitionCost : (int)(constructionCost * 0.25f);
        PassonBuildingProperties();
        popupPlacement = GetBuildingPopUpPlacement();
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
                        buildingProperties.cityRevenue = cityRevenue;
                    }
                }
            }
        }
    }

    // Apply proximity effect of this building on all neighboring buildings
    public float ApplyProximityEffects(List<Transform> allBuildings = null)
    {
        float maxDelay = 0;
        allBuildings ??= FindObjectOfType<CameraController>().GetAllBuildings();

        foreach (Transform existingBuilding in allBuildings)
        {

            if (existingBuilding != transform && (existingBuilding.CompareTag("Building") || existingBuilding.CompareTag("Road"))) // Skip self, Skip "Spaces" and anything not a "Building"
            {
                BuildingProperties building = existingBuilding.GetComponent<BuildingProperties>();
                // Check if building[i] is within THIS building's effect radius

                if (building != null && IsWithinProximity(building, effectRadius))
                {
                    // Delay pop ups based on distance
                    Vector3 positionA = building.GetBuildingPopUpPlacement();
                    positionA.y = 0;
                    Vector3 positionB = GetBuildingPopUpPlacement();
                    positionB.y = 0;
                    float roundedDistance = Vector3.Distance(positionA, positionB) / gridSize; // in number of grid spaces
                    float popupDelay = 0;
                    int metricCount = 0;

                    foreach (MetricBoost boost in proximityEffects)
                    {
                        popupDelay = roundedDistance + (metricCount * 14);
                        Debug.Log($"{name} APPLY BOOST TO {building.name} | {boost.metricName}");
                        ApplyBoost(building, boost, popupDelay);
                        maxDelay = Math.Max(maxDelay, popupDelay);
                        metricCount++;
                    }
                }
            }
        }

        return maxDelay;
    }

    // Remove proximity effect of this building on all neighboring buildings
    public float RemoveProximityEffects()
    {
        float maxDelay = 0;
        foreach (Transform buildingTransform in cameraController.GetAllBuildings())
        {
            if (buildingTransform != transform) // Skip self
            {
                BuildingProperties building = buildingTransform.GetComponent<BuildingProperties>();
                // Check if building[i] is within THIS building's effect radius
                if (building != null && IsWithinProximity(building, effectRadius))
                {
                    Vector3 positionA = building.GetBuildingPopUpPlacement();
                    positionA.y = 0;
                    Vector3 positionB = GetBuildingPopUpPlacement();
                    positionB.y = 0;
                    float roundedDistance = Vector3.Distance(positionA, positionB) / gridSize; // in number of grid spaces
                    float popupDelay = 0;
                    int metricCount = 0;
                    foreach (MetricBoost boost in proximityEffects)
                    {
                        popupDelay = roundedDistance + (metricCount * 14);
                        RemoveBoost(building, boost, popupDelay);
                        metricCount++;
                        maxDelay = Math.Max(maxDelay, popupDelay);
                    }
                }
            }
        }
        return maxDelay;
    }

    // Check if THIS building is within the proximity of the OTHER building
    public bool IsWithinProximity(BuildingProperties other, float threshold)
    {

        // Check if this building's position is within the effect radius of the other building
        if (IsPositionsClose(transform.position, other.transform.position, threshold))
        {
            return true;
        }

        // Check additional spaces of this building against the other building's position
        foreach (Transform space in additionalSpace)
        {
            if (IsPositionsClose(space.position, other.transform.position, threshold))
            {
                return true;
            }
        }

        // Check additional spaces of the other building against this building's position
        foreach (Transform otherSpace in other.additionalSpace)
        {
            if (IsPositionsClose(transform.position, otherSpace.position, threshold))
            {
                return true;
            }
        }

        // Check additional spaces of this building against additional spaces of the other building
        foreach (Transform space in additionalSpace)
        {
            foreach (Transform otherSpace in other.additionalSpace)
            {
                if (IsPositionsClose(space.position, otherSpace.position, threshold))
                {
                    return true;
                }
            }
        }

        // If none of the checks found a proximity, return false
        return false;
    }

    // Apply boost to target building 
    public void ApplyBoost(BuildingProperties targetBuilding, MetricBoost boost, float displayDelay)
    {
        ModifyProperty(targetBuilding, boost.metricName.ToString(), boost.boostValue);
        targetBuilding.PassonBuildingProperties();
        targetBuilding.ShowFloatingValue(boost.metricName, boost.boostValue, displayDelay);
    }

    // Remove boost from target building
    public void RemoveBoost(BuildingProperties targetBuilding, MetricBoost boost, float displayDelay)
    {
        // Get the property info for the specified metric name
        ModifyProperty(targetBuilding, boost.metricName.ToString(), -boost.boostValue);
        targetBuilding.ShowFloatingValue(boost.metricName, -boost.boostValue, displayDelay);
        targetBuilding.PassonBuildingProperties();
    }

    private bool IsPositionsClose(Vector3 positionA, Vector3 positionB, float threshold = 0)
    {
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

        print($"{distance} | {threshold}");

        return distance <= threshold;
    }

    public Vector3 GetBuildingPopUpPlacement()
    {

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

    public void ShowFloatingValue(BuildingMetric metric, int boostValue, float displayDelay = 0)
    {
        if (CompareTag("Space"))
        {
            Debug.LogError("Attempting to show floating value for SPACE");
            return;
        }
        if (floatingValuePrefab == null)
        {
            Debug.LogError($"{name} has no floating value prefab");
            return;
        };
        MetricTitle? metricTitle = MetricMapping.GetMetricTitle(metric);

        if (!metricTitle.HasValue)
        {
            Debug.LogError($"{metric} has no city metric mapping");
            return;
        }

        // Get the popup position
        Vector3 popupPosition = GetBuildingPopUpPlacement();
        popupPosition.y = popupPosition.y / 2;

        // Instantiate the floating value prefab
        GameObject floatingValue = Instantiate(floatingValuePrefab, popupPosition, Quaternion.identity);

        // Set the text and color based on the boost value
        bool isPositive = boostValue > 0;

        string valueText = $"{(isPositive ? "+" : "")}{boostValue}";
        bool metricIsInverted = MetricMapping.CityMetricIsInverted(metricTitle.Value);
        if (metricIsInverted) isPositive = !isPositive; // swap the color after the sign assignment
        floatingValue.GetComponent<FloatingValueEffect>().Initialize(valueText, isPositive, metricTitle, displayDelay);
    }

    // Method to add or subtract a value to/from a property or field
    public void ModifyProperty(object component, string propertyName, object deltaValue)
    {
        if (component == null)
        {
            Debug.LogError("Component is null.");
            return;
        }

        Type type = component.GetType();

        // Try to modify a property
        PropertyInfo property = type.GetProperty(propertyName);
        if (property != null && property.CanRead && property.CanWrite)
        {
            object currentValue = property.GetValue(component);

            if (currentValue is int intValue && deltaValue is int deltaInt)
            {
                property.SetValue(component, intValue + deltaInt);
                Debug.Log($"Property '{propertyName}' modified to: {intValue + deltaInt}");
                return;
            }
            if (currentValue is float floatValue && deltaValue is float deltaFloat)
            {
                property.SetValue(component, floatValue + deltaFloat);
                Debug.Log($"Property '{propertyName}' modified to: {floatValue + deltaFloat}");
                return;
            }
        }

        // Try to modify a field
        FieldInfo field = type.GetField(propertyName);
        if (field != null)
        {
            object currentValue = field.GetValue(component);

            if (currentValue is int intValue && deltaValue is int deltaInt)
            {
                field.SetValue(component, intValue + deltaInt);
                // Debug.Log($"Field '{propertyName}' modified to: {intValue + deltaInt}");
                return;
            }
            if (currentValue is float floatValue && deltaValue is float deltaFloat)
            {
                field.SetValue(component, floatValue + deltaFloat);
                Debug.Log($"Field '{propertyName}' modified to: {floatValue + deltaFloat}");
                return;
            }
        }

        Debug.LogError($"Property or field '{propertyName}' not found, or type mismatch in {type.Name}.");
    }
}


