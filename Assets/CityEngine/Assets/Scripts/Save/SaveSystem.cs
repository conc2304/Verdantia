using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

/**
Part of the "City Engine" Asset from the Unity Asset store (unchanged except for initialization flag, and flexible filenames)
provides methods for saving and loading game data, focusing on buildings and roads. 
It uses binary serialization to store data into a BuildingData object, writing it to a file in the persistent data path. 
The SaveBuildings method serializes and saves data, while LoadBuildings deserializes it to recreate game elements. 
The FormatFileName method standardizes file names for consistency. 
This ensures reliable data persistence for game continuity.
**/
public static class SaveSystem
{

    private const string defaultFilePath = "binary.fun";
    public static void SaveBuildings(SaveDataTrigger.SaveProperties[] buildingProperties, string fileName = defaultFilePath)
    {
        string path = Application.persistentDataPath + "/" + (fileName ?? defaultFilePath);

        if (File.Exists(path)) File.Delete(path);

        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(path, FileMode.Create);

        BuildingData data = new BuildingData(buildingProperties);
        formatter.Serialize(stream, data);
        stream.Close();

        Debug.Log(Application.persistentDataPath);
    }

    public static BuildingData LoadBuildings(string fileName = defaultFilePath)
    {
        string path = Application.persistentDataPath + "/" + (fileName ?? defaultFilePath);

        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            BuildingData data = formatter.Deserialize(stream) as BuildingData;

            stream.Close();
            return data;
        }
        else
        {
            Debug.Log("Save file not found at: " + path);
            return null;
        }
    }

    public static string FormatFileName(string name)
    {
        return $"binary_{name}.fun";
    }

}
