using UnityEngine;

public static class NumbersUtils
{
    // Remap function to remap values from one range to another
    public static float Remap(float inputMin, float inputMax, float outputMin, float outputMax, float value)
    {
        // Normalize the input value to a 0-1 range using Mathf.InverseLerp
        float t = Mathf.InverseLerp(inputMin, inputMax, value);

        // Remap the normalized value to the output range using Mathf.Lerp
        return Mathf.Lerp(outputMin, outputMax, t);
    }

    // You can add more utility functions here as needed
}