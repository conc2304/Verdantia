using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

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
