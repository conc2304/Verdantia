using System;
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

    public static double ToNthDecimal(double number, int decimals)
    {
        return Math.Round(number, decimals, MidpointRounding.AwayFromZero);
    }

    public static double RoundToNearestHalf(double value)
    {
        return Math.Round(value * 2) / 2;
    }

    public static string FormatMoney(double amount, bool includeDollarSign = false)
    {
        // Determine the suffix and scale the amount
        string prefix = includeDollarSign ? "$" : "";
        string suffix = "";
        if (amount >= 1_000_000)
        {
            amount /= 1_000_000;
            suffix = "M";
        }
        else if (amount >= 1_000)
        {
            amount /= 1_000;
            suffix = "K";
        }

        // Ensure at most 3 significant digits
        string formattedAmount = amount >= 100
            ? Math.Round(amount).ToString("N0")
            : amount >= 10
                ? Math.Round(amount, 1).ToString("N1")
                : Math.Round(amount, 2).ToString("N2");

        return $"{prefix}{formattedAmount}{suffix}";
    }
}
