using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class SaveSystem
{

    public static void SaveBuildings(SaveDataTrigger.SaveProperties[] buildingProperties)
    {
        if (System.IO.File.Exists(Application.persistentDataPath + "/binary.fun"))
            File.Delete(Application.persistentDataPath + "/binary.fun");

        BinaryFormatter formatter = new BinaryFormatter();
        string path = Application.persistentDataPath + "/binary.fun";
        FileStream stream = new FileStream(path, FileMode.Create);

        BuildingData data = new BuildingData(buildingProperties);
        formatter.Serialize(stream, data);
        stream.Close();

        Debug.Log(Application.persistentDataPath);
    }

    public static BuildingData LoadBuildings()
    {
        string path = Application.persistentDataPath + "/binary.fun";
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
            Debug.Log("Save file not found" + path);
            return null;
        }

    }

}
