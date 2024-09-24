using System;
using System.Reflection;

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
}
