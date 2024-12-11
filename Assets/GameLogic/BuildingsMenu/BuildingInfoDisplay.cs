using UnityEngine;
using TMPro;
using System.Reflection;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using System;

/**
Handles the display of detailed information about a selected building in the game, 
providing insights into its properties and effects on city metrics.

Key features include:

Dynamic Data Display: 
Displays key building metrics like construction cost, demolition cost, happiness impact, and energy consumption 
by dynamically reading properties from the selected building.

Budget Visualization: 
Shows whether the building's construction or demolition cost exceeds the city's budget by highlighting values in red when over-budget.

Proximity Effects: 
Lists nearby buildings' effects (e.g., boosts or penalties) on the selected building’s metrics, such as increased happiness or reduced pollution.

Interactive Modal Window:
Open: Shows additional information about the building in a modal view, including its description and detailed metrics.
Close: Returns to the main interface when the modal is closed.
UI Management: Automatically clears and repopulates UI elements to ensure accurate and up-to-date data is displayed for each selected building.

This functionality provides players with an intuitive way to understand how 
individual buildings impact the overall city simulation, promoting informed decision-making.
**/

public class BuildingInfoDisplay : MonoBehaviour
{

    public GameObject labelValuePrefab;
    public Transform displayParent;
    public GameObject proximityEffectTextPrefab;
    public GameObject modal;
    public TMP_Text buildingNameText;
    public TMP_Text cityBudgetText;
    public TMP_Text modalTitle;
    public TMP_Text modalBodyText;
    public GameObject infoNavToggle;
    public GameObject menuBuildings;


    public void DisplayBuildingData(BuildingProperties buildingProps)

    {
        DeleteAllChildrenFromParent(displayParent);
        float budget = (float)FindObjectOfType<CityMetricsManager>().budget;
        string formattedBudget = NumbersUtils.FormattedNumber(budget, "$");
        cityBudgetText.text = $"City Budget: {formattedBudget}";

        // Clear the last data 
        displayParent.gameObject.SetActive(true);
        modal.SetActive(false);
        modalBodyText.text = buildingProps.buildingDescription;
        modalTitle.text = buildingProps.buildingName;

        // GameObject buildingNameGO = Instantiate(buildingNameInfoGO, displayParent);
        // buildingNameGO.transform.Find("BuildingName").GetComponent<TMP_Text>().text = buildingProps.buildingName;
        buildingNameText.text = buildingProps.buildingName;

        // Loop over each metric in the BuildingMetric enum
        foreach (BuildingMetric metric in Enum.GetValues(typeof(BuildingMetric)))
        {
            string metricName = metric.ToString();

            // Get the value from the building using reflection
            FieldInfo fieldInfo = buildingProps.GetType().GetField(metricName, BindingFlags.Public | BindingFlags.Instance);

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
                labelText.text = StringsUtils.ConvertToLabel(metricName);
                string prefix = Regex.IsMatch(metricName.ToLower(), "tax|cost|upkeep|income|revenue") ? "$" : "";
                int valueInt = Convert.ToInt32(value);
                string formattedValue = NumbersUtils.FormattedNumber(valueInt, prefix);
                valueText.text = value != null ? formattedValue : "N/A";

                // show if building/ is out of budget 
                if (metricName == BuildingMetric.constructionCost.ToString() || metricName == BuildingMetric.demolitionCost.ToString())
                {
                    valueText.color = valueInt > budget ? Color.red : Color.white;
                }
            }
        }

        // Display list of proximity effects
        if (buildingProps.proximityEffects.Count > 0)
        {
            // Text Description as Prefab
            Instantiate(proximityEffectTextPrefab, displayParent);

            // Add the list of proximity effects
            foreach (MetricBoost boost in buildingProps.proximityEffects)
            {
                GameObject labelValueGO = Instantiate(labelValuePrefab, displayParent);
                labelValueGO.name = "data_item";

                // Get the TextMeshProUGUI components
                HorizontalLayoutGroup textParent = labelValueGO.GetComponentInChildren<HorizontalLayoutGroup>(true);
                TMP_Text labelText = textParent.transform.Find("Label")?.GetComponent<TMP_Text>();
                TMP_Text valueText = textParent.transform.Find("Value")?.GetComponent<TMP_Text>();

                // Set the text
                string metricName = boost.metricName.ToString();
                labelText.text = StringsUtils.ConvertToLabel(metricName);
                string prefix = Regex.IsMatch(metricName, "tax|cost|upkeep|income|revenue") ? "$" : "";
                string formattedValue = NumbersUtils.FormattedNumber(Convert.ToInt32(boost.boostValue), prefix);
                valueText.text = formattedValue;
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
