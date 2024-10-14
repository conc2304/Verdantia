using UnityEngine;
using TMPro;
using System.Reflection;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using System;

public class BuildingInfoDisplay : MonoBehaviour
{
    // Reference to the building's data
    private BuildingProperties building; // Reference to a BuildingProperties script or similar component

    // Prefab for the label-value display (can be a simple UI text pair)
    public GameObject labelValuePrefab;

    // The parent transform where the labels and values will be displayed (e.g., a vertical layout group)
    public Transform displayParent;

    // List of properties (you can fetch this from the building itself if necessary)
    public readonly string[] dataProps = {
        "constructionCost",
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
        "industryBoost"
    };

    void Start()
    {
        // Create and display each label-value pair for each property
        // DisplayBuildingData();
    }

    public void DisplayBuildingData(BuildingProperties buildingProps)

    {
        print(buildingProps.buildingName);

        DeleteAllChildrenFromParent(displayParent);

        displayParent.gameObject.SetActive(true);

        foreach (string prop in dataProps)
        {
            // Get the value from the building using reflection
            FieldInfo fieldInfo = buildingProps.GetType().GetField(prop, BindingFlags.Public | BindingFlags.Instance);

            if (fieldInfo != null)
            {
                object value = fieldInfo.GetValue(buildingProps);

                // Instantiate a new label-value UI element
                GameObject labelValueGO = Instantiate(labelValuePrefab, displayParent);
                labelValueGO.name = "data_item";

                // Get the TextMeshProUGUI components
                HorizontalLayoutGroup textParent = labelValueGO.GetComponentInChildren<HorizontalLayoutGroup>(true);
                TMP_Text labelText = textParent.transform.Find("Label")?.GetComponent<TMP_Text>();
                TMP_Text valueText = textParent.transform.Find("Value")?.GetComponent<TMP_Text>();

                // Set the text
                labelText.text = ConvertToLabel(prop);
                string prefix = Regex.IsMatch(prop.ToLower(), "tax|cost") ? "$" : "";
                string formattedValue = NumbersUtils.FormattedNumber(Convert.ToInt32(value), prefix);
                valueText.text = value != null ? formattedValue : "N/A";
            }
        }
    }

    // Converts camelCase to a more readable format like "Construction Cost"
    string ConvertToLabel(string camelCaseString)
    {
        string spacedString = Regex.Replace(camelCaseString, "(\\B[A-Z])", " $1");
        return char.ToUpper(spacedString[0]) + spacedString.Substring(1);
    }

    public void DeleteAllChildrenFromParent(Transform parentTransform)
    {
        // Check if the parent has any children
        if (parentTransform == null)
        {
            Debug.LogError("Parent transform is not assigned.");
            return;
        }

        // Iterate through each child of the parent and destroy it
        while (parentTransform.childCount > 0)
        {
            DestroyImmediate(parentTransform.GetChild(0).gameObject);
        }
    }
}
