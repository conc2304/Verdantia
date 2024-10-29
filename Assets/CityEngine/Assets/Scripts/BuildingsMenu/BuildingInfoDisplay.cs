using UnityEngine;
using TMPro;
using System.Reflection;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using System;

public class BuildingInfoDisplay : MonoBehaviour
{

    public GameObject labelValuePrefab;

    // The parent transform where the labels and values will be displayed (e.g., a vertical layout group)
    public Transform displayParent;
    public GameObject modal;
    public TMP_Text buildingNameText;
    public TMP_Text modalTitle;
    public TMP_Text modalBodyText;
    public GameObject infoNavToggle;
    public GameObject menuBuildings;

    // List of properties (you can fetch this from the building itself if necessary)
    public readonly string[] dataProps ={
        "constructionCost",
        "demolitionCost",
        "operationalCost",
        "taxRevenue",
        "upkeep",
        "energyConsumption",
        // "waterConsumption",
        // "wasteProduction",
        "resourceProduction",
        "capacity",
        // "jobsProvided",
        "happinessImpact",
        "healthImpact",
        // "educationImpact",
        "pollutionOutput",
        "pollutionReduction",
        "greenSpaceEffect",
        "heatContribution",
        "carbonFootprint",
        "taxContribution",
        // "jobCreation",
        "industryBoost",
        "coverageRadius",
    };



    public void DisplayBuildingData(BuildingProperties buildingProps)

    {
        // Clear the last data 
        DeleteAllChildrenFromParent(displayParent);

        displayParent.gameObject.SetActive(true);
        modal.SetActive(false);
        modalBodyText.text = buildingProps.buildingDescription;
        modalTitle.text = buildingProps.buildingName;

        // GameObject buildingNameGO = Instantiate(buildingNameInfoGO, displayParent);
        // buildingNameGO.transform.Find("BuildingName").GetComponent<TMP_Text>().text = buildingProps.buildingName;
        buildingNameText.text = buildingProps.buildingName;

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
                labelText.text = StringsUtils.ConvertToLabel(prop);
                string prefix = Regex.IsMatch(prop.ToLower(), "tax|cost|upkeep") ? "$" : "";
                string formattedValue = NumbersUtils.FormattedNumber(Convert.ToInt32(value), prefix);
                valueText.text = value != null ? formattedValue : "N/A";
            }
        }
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

    public void OnModalClose()
    {
        modal.SetActive(false);
        infoNavToggle.SetActive(true);
        menuBuildings.SetActive(true);
    }
    public void OnMoreInfoClick()
    {
        if (modal.activeSelf) return;
        modal.SetActive(true);
        infoNavToggle.SetActive(false);
        menuBuildings.SetActive(false);

    }
}
