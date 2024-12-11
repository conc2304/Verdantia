using System;
using UnityEngine;

/**
Provides utility functions for manipulating and formatting numerical data, such as remapping values between ranges, 
formatting numbers with commas or abbreviations, rounding to specific decimal places, and scaling values for better readability. 
To simplify numerical operations and improve data presentation in a more user-friendly manner.
**/
public static class NumbersUtils
{
    // Remap function to remap values from one range to another
    public static float Remap(float inputMin, float inputMax, float outputMin, float outputMax, float value)
    {
        // Normalize the input value to a range
        float t = Mathf.InverseLerp(inputMin, inputMax, value);

        // Remap the normalized value to the output range
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

    public static string NumberToAbrev(double amount, string prefix = "", string suffix = "")
    {
        // Determine the suffix and scale the amount
        string numberSuffix = "";
        if (amount >= 1_000_000)
        {
            amount /= 1_000_000;
            numberSuffix = "M";
        }
        else if (amount >= 1_000)
        {
            amount /= 1_000;
            numberSuffix = "K";
        }

        // Ensure at most 3 significant digits
        string formattedAmount = amount >= 100
            ? Math.Round(amount).ToString("N0")
            : amount >= 10
                ? Math.Round(amount, 1).ToString("N1")
                : Math.Round(amount, 2).ToString("N2");

        return $"{prefix}{formattedAmount}{numberSuffix} {suffix}";
    }
}
