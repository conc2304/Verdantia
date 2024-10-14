using UnityEngine;

public static class NumbersUtils
{
    // Remap function to remap values from one range to another
    public static float Remap(float inputMin, float inputMax, float outputMin, float outputMax, float value)
    {
        // Normalize the input value to a range using Mathf.InverseLerp
        float t = Mathf.InverseLerp(inputMin, inputMax, value);

        // Remap the normalized value to the output range using Mathf.Lerp
        return Mathf.Lerp(outputMin, outputMax, t);
    }
    public static string FormattedNumber(float number, string prefix = "", string suffix = "")
    {
        // Format the number with commas (for whole numbers) or with decimal points
        string formattedNumber = string.Format("{0:N0}", number);

        return prefix + formattedNumber + suffix;
    }
}
