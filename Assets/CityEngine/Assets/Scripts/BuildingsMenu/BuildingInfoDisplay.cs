using UnityEngine;
using TMPro;
using System.Reflection;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using System;

public class BuildingInfoDisplay : MonoBehaviour
{

    public GameObject labelValuePrefab;
    public Transform displayParent;
    public GameObject proximityEffectTextPrefab;
    public GameObject modal;
    public TMP_Text buildingNameText;
    public TMP_Text modalTitle;
    public TMP_Text modalBodyText;
    public GameObject infoNavToggle;
    public GameObject menuBuildings;


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
                string prefix = Regex.IsMatch(metricName, "tax|cost|upkeep") ? "$" : "";
                string formattedValue = NumbersUtils.FormattedNumber(Convert.ToInt32(value), prefix);
                valueText.text = value != null ? formattedValue : "N/A";
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
                valueText.text = boost.boostValue != null ? formattedValue : "N/A";
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
