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
    // buildingDescription
};


[Serializable]
public class MetricBoost
{
    public BuildingMetric metricName;
    public float boostValue;
}

[Serializable]
public class BuildingFact
{
    public string title;
    [TextArea]
    public string fact;
}

public class Factoid : BuildingFact
{
    public string caseStudyLink;
}

public class BuildingProperties : MonoBehaviour
{
    public int buildingIndex = -1;
    public GameObject environment;
    public bool connectToRoad;
    public bool unrestrictedPlacement = false;  // Allows building to not be built next to roads or chainable buildings (ie allow forest placement anywhere)
    public bool allowChaining = false;  // Allows building to be placed not along roads if chaining
    public List<Transform> chainableTypes; // List of building types that item can chain off of (ie allow connecting of parks)

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
    public float constructionCost;
    public float demolitionCost;

    // Resource Management Properties
    [Header("Resource Management Properties")]

    public float cityRevenue;
    public float netEnergy;

    // Population Dynamics Properties
    [Header("Population Dynamics Properties")]
    public float capacity; // For population metrics
    public float happinessImpact;

    [Header("Environmental Impact Properties")]
    public float pollutionImpact;
    public float heatContribution;
    public float greenSpaceEffect;
    public float carbonFootprint;
    public float effectRadius;
    public List<MetricBoost> proximityEffects;
    public GameObject floatingValuePrefab;
    private readonly int gridSize = 10;

    public List<BuildingFact> funFacts;
    public string caseStudyLink = "https://onetreeplanted.org/blogs/stories/urban-heat-island";


    void Start()
    {
        demolitionCost = demolitionCost != 0 ? demolitionCost : (int)(constructionCost * 0.25f);
        PassonBuildingProperties();
    }


    private void PassonBuildingProperties()
    {
        // Make all "Additional Spaces" inherit all properties from the Building Parent
        if (additionalSpace != null && additionalSpace.Length > 0)
        {
            // Loop through each Transform in the additionalSpace array
            for (int i = 0; i < additionalSpace.Length; i++)
            {
                Transform space = additionalSpace[i];

                if (space != null && space.CompareTag("Space") && space.TryGetComponent(out BuildingProperties buildingProperties)) // Ensure the Transform is not null
                {
                    TransferBuildingProperties(buildingProperties);
                }
            }
        }
    }


    public void TransferBuildingProperties(BuildingProperties transferTarget)
    {
        transferTarget.constructionCost = constructionCost;
        transferTarget.demolitionCost = demolitionCost;
        transferTarget.heatContribution = heatContribution;
        transferTarget.netEnergy = netEnergy;
        transferTarget.capacity = capacity;
        transferTarget.happinessImpact = happinessImpact;
        transferTarget.pollutionImpact = pollutionImpact;
        transferTarget.heatContribution = heatContribution;
        transferTarget.greenSpaceEffect = greenSpaceEffect;
        transferTarget.carbonFootprint = carbonFootprint;
        transferTarget.effectRadius = effectRadius;
        transferTarget.proximityEffects = proximityEffects;
        transferTarget.cityRevenue = cityRevenue;
    }

    // Apply proximity effect of this building on all neighboring buildings
    public float ApplyProximityEffects(bool removeEffect = false)
    {
        float maxDelay = 0;
        bool includeSpaces = false;
        List<Transform> allBuildings = FindObjectOfType<CameraController>().GetAllBuildings(includeSpaces);

        foreach (Transform existingBuilding in allBuildings)
        {
            if (gameObject.GetInstanceID() == existingBuilding.gameObject.GetInstanceID())
            {
                continue;
            }

            if (!(existingBuilding.CompareTag("Building") || existingBuilding.CompareTag("Road")))
            {
                continue;
            }
            // Skip self, Skip "Spaces" and anything not a "Building"

            BuildingProperties building = existingBuilding.GetComponent<BuildingProperties>();
            // Check if building[i] is within THIS building's effect radius

            if (building != null && IsWithinProximity(building, effectRadius))
            {
                // Delay pop ups based on distance
                Vector3 positionA = building.GetBuildingPopUpPlacement();
                positionA.y = 0;
                Vector3 positionB = GetBuildingPopUpPlacement();
                positionB.y = 0;
                // dissipate the value of the effect over the distance
                float roundedDistance = (float)Math.Round(Vector3.Distance(positionA, positionB)) / gridSize; // in number of grid spaces
                float distanceMultiplier = Math.Min(1, roundedDistance / effectRadius);

                float popupDelay = 0;
                int metricCount = 0;

                foreach (MetricBoost boost in proximityEffects)
                {
                    // apply boost delay based on distance from boost initiator
                    boost.boostValue = (float)NumbersUtils.RoundToNearestHalf(boost.boostValue * distanceMultiplier);
                    popupDelay = (roundedDistance * 3) + (metricCount * 6);
                    ApplyBoost(building, boost, popupDelay, removeEffect);
                    maxDelay = Math.Max(maxDelay, popupDelay);
                    metricCount++;
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
    public void ApplyBoost(BuildingProperties targetBuilding, MetricBoost boost, float displayDelay, bool removeEffect = false)
    {
        float valueInverter = removeEffect ? -1f : 1f;
        ModifyProperty(targetBuilding, boost.metricName.ToString(), valueInverter * boost.boostValue);
        targetBuilding.PassonBuildingProperties();
        targetBuilding.ShowFloatingValue(boost.metricName, valueInverter * boost.boostValue, displayDelay);
    }


    private bool IsPositionsClose(Vector3 positionA, Vector3 positionB, float threshold = 0)
    {
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

    public void ShowFloatingValue(BuildingMetric metric, float boostValue, float displayDelay = 0)
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

        if (boostValue != 0) floatingValue.GetComponent<FloatingValueEffect>().Initialize(valueText, isPositive, metricTitle, displayDelay);
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
                return;
            }
            if (currentValue is float floatValue && deltaValue is float deltaFloat)
            {
                property.SetValue(component, floatValue + deltaFloat);
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
                return;
            }
        }

        Debug.LogError($"Property or field '{propertyName}' not found, or type mismatch in {type.Name}.");
    }

    public Factoid? GetRandomBuildingFact()
    {
        if (funFacts.Count > 0)
        {
            BuildingFact randomFact = funFacts[UnityEngine.Random.Range(1, funFacts.Count)];
            return new Factoid
            {
                fact = randomFact.fact,
                title = randomFact.title,
                caseStudyLink = caseStudyLink,
            };
        }

        return null;
    }



}


