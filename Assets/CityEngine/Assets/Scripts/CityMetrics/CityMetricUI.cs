using UnityEngine;
using TMPro;
using UnityEngine.UI;

[ExecuteAlways] // Allows this to work both at runtime and in the Editor
public class CityMetricUIItem : MonoBehaviour
{
    // Public fields to be set in the Unity Editor
    [Header("UI Components")]
    public TMP_Text labelText; // Text for the label
    public TMP_Text valueText;
    public Image iconImage; // Image for the icon

    [Header("Properties")]
    public string label; // The label text, set from the Editor
    public Sprite icon; // The icon sprite, set from the Editor

    public string value = "0";
    public string valuePrefix = "";
    public string valueSuffix = "";

    // This will be called when the object is instantiated or updated in the Editor
    void OnValidate()
    {
        if (labelText != null)
        {
            labelText.text = label;
        }

        if (iconImage != null && icon != null)
        {
            iconImage.sprite = icon;
            iconImage.gameObject.SetActive(true);
        }
        else if (iconImage != null)
        {
            iconImage.gameObject.SetActive(false); // Hide icon if no sprite is provided
        }

        if (valueText != null)
        {
            valueText.text = valuePrefix + value + valueSuffix;
        }
    }

    public void UpdateValue(string newValue)
    {
        value = newValue;
        valueText.text = valuePrefix + value + valueSuffix;
    }

}
