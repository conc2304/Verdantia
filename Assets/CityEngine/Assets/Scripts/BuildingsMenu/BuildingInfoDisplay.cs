using UnityEngine;
using TMPro; // Import TextMesh Pro namespace
using System.Reflection;

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
        foreach (string prop in dataProps)
        {
            // Get the value from the building using reflection
            FieldInfo fieldInfo = buildingProps.GetType().GetField(prop, BindingFlags.Public | BindingFlags.Instance);

            if (fieldInfo != null)
            {
                object value = fieldInfo.GetValue(building);

                // Instantiate a new label-value UI element
                GameObject labelValueGO = Instantiate(labelValuePrefab, displayParent);

                // Get the TextMeshProUGUI components (assuming label and value texts are children of the prefab)
                TMP_Text labelText = labelValueGO.transform.Find("Label").GetComponent<TMP_Text>();
                TMP_Text valueText = labelValueGO.transform.Find("Value").GetComponent<TMP_Text>();

                // Set the label text
                labelText.text = ConvertToLabel(prop);

                // Set the value text (format if necessary)
                valueText.text = value != null ? value.ToString() : "N/A";
            }
        }
    }

    // Converts camelCase to a more readable format like "Construction Cost"
    string ConvertToLabel(string camelCaseString)
    {
        string spacedString = System.Text.RegularExpressions.Regex.Replace(camelCaseString, "(\\B[A-Z])", " $1");
        return char.ToUpper(spacedString[0]) + spacedString.Substring(1);
    }
}
