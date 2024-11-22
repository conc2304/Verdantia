using System;
using System.Reflection;
using UnityEngine;
using System.Text;

public static class ObjectPrinter
{
    // This method prints all properties and fields of an object along with their values.
    public static void PrintKeyValuePairs(object obj)
    {
        if (obj == null)
        {
            Console.WriteLine("The object is null.");
            return;
        }

        Type type = obj.GetType();

        Console.WriteLine($"Object Type: {type.Name}");

        // Iterate through all the public properties of the object
        PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (PropertyInfo property in properties)
        {
            try
            {
                object value = property.GetValue(obj);
                Console.WriteLine($"{property.Name}: {value}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{property.Name}: Unable to get value ({ex.Message})");
            }
        }

        // Iterate through all the public fields of the object
        FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
        foreach (FieldInfo field in fields)
        {
            try
            {
                object value = field.GetValue(obj);
                Console.WriteLine($"{field.Name}: {value}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{field.Name}: Unable to get value ({ex.Message})");
            }
        }
    }

    public static void PrintBuildingDataAsJson(BuildingProperties buildingProps)
    {
        // Use StringBuilder for efficient string concatenation
        StringBuilder jsonBuilder = new StringBuilder();
        jsonBuilder.Append("{\n");

        jsonBuilder.AppendFormat("  \"{0}\": {1}", "buildingName", FormatValue(buildingProps.buildingName));
        jsonBuilder.Append(",");
        jsonBuilder.Append("\n");

        jsonBuilder.AppendFormat("  \"{0}\": {1}", "prefabName", FormatValue(buildingProps.gameObject.name));
        jsonBuilder.Append(",");
        jsonBuilder.Append("\n");

        jsonBuilder.AppendFormat("  \"{0}\": {1}", "buildingSize", FormatValue(buildingProps.additionalSpace.Length + 1));
        jsonBuilder.Append(",");
        jsonBuilder.Append("\n");

        // Iterate over each property in dataProps
        foreach (BuildingMetric metric in Enum.GetValues(typeof(BuildingMetric)))
        {
            string metricName = metric.ToString();
            FieldInfo fieldInfo = buildingProps.GetType().GetField(metricName, BindingFlags.Public | BindingFlags.Instance);

            if (fieldInfo != null)
            {
                // Get the value of the property
                object value = fieldInfo.GetValue(buildingProps);

                // Append the property and its value to the JSON string
                jsonBuilder.AppendFormat("  \"{0}\": {1}", metricName, FormatValue(value));

                // Add a comma if it's not the last item
                jsonBuilder.Append(",");
                jsonBuilder.Append("\n");
            }
        }

        // Add proximity effects
        jsonBuilder.Append("  \"proximityEffects\": [\n");
        foreach (MetricBoost boost in buildingProps.proximityEffects)
        {
            jsonBuilder.Append("    {\n");
            jsonBuilder.AppendFormat("      \"title\": {0},\n", FormatValue(boost.metricName));
            jsonBuilder.AppendFormat("      \"value\": {0}\n", FormatValue(boost.boostValue));
            jsonBuilder.Append("    },\n");
        }

        // Remove trailing comma from the last proximity effect
        if (buildingProps.proximityEffects.Count > 0)
        {
            jsonBuilder.Length -= 2; // Remove the last comma and newline
            jsonBuilder.Append("\n");
        }

        jsonBuilder.Append("  ]\n");

        jsonBuilder.Append("}");

        // Print the JSON-like string
        // Debug.Log(jsonBuilder.ToString());
    }

    // Format the value to be JSON-compatible
    private static string FormatValue(object value)
    {
        if (value == null)
            return "\"N/A\"";
        else if (value is string)
            return $"\"{value}\"";
        else
            return value.ToString();
    }
}
