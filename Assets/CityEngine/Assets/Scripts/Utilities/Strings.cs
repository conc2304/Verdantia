using System.Text.RegularExpressions;
using System.Globalization;

public static class StringsUtils
{
    public static string ConvertToLabel(string camelCaseString)
    {
        string spacedString = Regex.Replace(camelCaseString, "(\\B[A-Z])", " $1");
        return char.ToUpper(spacedString[0]) + spacedString.Substring(1);
    }

    public static string ConvertToCamelCase(string label)
    {
        string[] words = label.Split(' ');
        string camelCaseString = words[0].ToLower();

        for (int i = 1; i < words.Length; i++)
        {
            camelCaseString += CultureInfo.CurrentCulture.TextInfo.ToTitleCase(words[i].ToLower());
        }

        return camelCaseString;
    }

    public static string GetMonthAbbreviation(int month)
    {
        string[] monthNames = { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };

        // Subtract 1 because arrays are 0-indexed but months are 1-12
        if (month >= 1 && month <= 12)
        {
            return monthNames[month - 1];
        }
        else
        {
            return "Invalid Month";
        }
    }
}


