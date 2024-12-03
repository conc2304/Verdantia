using UnityEngine;
using TMPro;
using UnityEngine.UI;

[ExecuteAlways] // Allows this to work both at runtime and in the Editor
public class CityMetricUIItem : MonoBehaviour
{
    [Header("UI Components")]
    public TMP_Text labelText;
    public TMP_Text valueText;
    public TMP_Text targetText;
    public Image iconImage;
    public Image infoImage;

    [Header("Properties")]
    public string label;
    public Sprite icon;
    public bool displayInfoIcon = false;
    public string value = "0";
    public string targetValue;
    public string valuePrefix = "";
    public string valueSuffix = "";
    private string unit = "";
    private MetricUnits.UnitPosition unitPosition = MetricUnits.UnitPosition.After;

    void Start()
    {
        infoImage.gameObject.SetActive(displayInfoIcon);
    }

    void OnValidate()
    {
        UpdateLabelText();
        UpdateIconImage();
        UpdateValueText(value);
        infoImage.gameObject.SetActive(displayInfoIcon);
    }

    public void SetLabel(string newLabel)
    {
        label = newLabel;
        UpdateLabelText();
    }

    public void SetUnit(string newUnit, MetricUnits.UnitPosition position)
    {
        unit = newUnit;
        unitPosition = position;
    }

    public void SetIcon(Sprite newIcon)
    {
        icon = newIcon;
        UpdateIconImage();
    }

    public void SetValueFormatting(string prefix = "", string suffix = "")
    {
        valuePrefix = prefix;
        valueSuffix = suffix;
    }

    public void UpdateValue(string newValue)
    {
        value = newValue;
        UpdateValueText(newValue);
    }

    public void UpdateTargetValue(string newValue)
    {
        targetValue = newValue;
        targetText.gameObject.SetActive(true);
        UpdateTargetText(targetValue);
    }



    private void UpdateLabelText()
    {
        if (labelText != null)
        {
            labelText.text = label;
        }
    }


    private void UpdateIconImage()
    {
        if (iconImage != null)
        {
            if (icon != null)
            {
                iconImage.sprite = icon;
                iconImage.gameObject.SetActive(true);
            }
            else
            {
                iconImage.gameObject.SetActive(false); // Hide icon if no sprite is provided
            }
        }
    }

    public void UpdateValueText(string newValue)
    {
        if (unitPosition == MetricUnits.UnitPosition.Before)
        {
            valueText.text = $"{unit}{newValue}";
        }
        else
        {
            valueText.text = $"{newValue}{unit}";
        }
    }

    public void UpdateTargetText(string newValue)
    {
        if (unitPosition == MetricUnits.UnitPosition.Before)
        {
            targetText.text = $"/ {unit}{newValue}";
        }
        else
        {
            targetText.text = $"/ {newValue}{unit}";
        }
    }

}
