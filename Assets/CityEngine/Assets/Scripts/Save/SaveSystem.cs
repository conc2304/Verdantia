using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class SaveSystem : MonoBehaviour
{
    // Make the file name editable in the Unity Inspector
    [Header("Save File Settings")]
    public string fileName = "binary.fun";  // Default file name

    public void SaveBuildings(SaveDataTrigger.SaveProperties[] buildingProperties)
    {
        // Use the file name from the Inspector
        string path = Application.persistentDataPath + "/" + fileName;

        if (File.Exists(path))
            File.Delete(path);

        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(path, FileMode.Create);

        BuildingData data = new BuildingData(buildingProperties);
        formatter.Serialize(stream, data);
        stream.Close();

        Debug.Log("Data saved to: " + path);
    }

    public BuildingData LoadBuildings()
    {
        // Use the file name from the Inspector
        string path = Application.persistentDataPath + "/" + fileName;

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
}
