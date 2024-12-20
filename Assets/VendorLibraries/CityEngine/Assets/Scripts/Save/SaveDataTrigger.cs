﻿using UnityEngine;

/**
Part of the "City Engine" Asset from the Unity Asset store (unchanged except for initialization flag, and flexible filenames)

Handles saving and loading city-building game data, including buildings and roads. 
It serializes properties like position, rotation, and timers into SaveProperties and saves them using SaveSystem. 
On load, it recreates buildings and roads from saved data by instantiating prefabs and restoring their properties.  
It integrates with the CameraController to ensure proper placement and behavior in the game, enabling persistence and seamless gameplay continuity as well as loading city mission levels.
**/

public class SaveDataTrigger : MonoBehaviour
{

    public const string defaultFilePath = "binary.fun";
    public string cityFileName = "binary.fun"; // default city
    public class SaveProperties
    {
        public int index = -1;
        public int x = 0;
        public int y = 0;
        public int z = 0;
        public int rot_x = 0;
        public int rot_y = 0;
        public int rot_z = 0;
        public float timer;
        public string name;
    }

    public GameObject[] buildingsPropertiesForIndex;
    SaveProperties[] buildingsPropertiesBuilded;

    public GameObject road;

    CameraController cameraController;

    GameObject building;

    BuildingData data;

    public bool doSave = false;



    public bool cityLoadInitialized { get; private set; } = false;
    private void Awake()
    {
        cameraController = FindObjectOfType<CameraController>();

        BuildingDataLoad(cityFileName);
        string buildingsStr = "";
        for (int i = 0; i < buildingsPropertiesForIndex.Length; i++)
        {

            buildingsStr += buildingsPropertiesForIndex[i].name + " | " + buildingsPropertiesForIndex[i].GetComponent<BuildingProperties>().buildingName + ", ";
            try
            {
                buildingsPropertiesForIndex[i].GetComponent<BuildingProperties>().buildingIndex = i;
                ObjectPrinter.PrintBuildingDataAsJson(buildingsPropertiesForIndex[i].GetComponent<BuildingProperties>());
            }
            catch
            {
                buildingsPropertiesForIndex[i].GetComponent<RoadProperties>().buildingIndex = i;
            }

        }

        // print(buildingsStr);
        cityLoadInitialized = true;
    }

    public void BuildingDataSave(string fileName = defaultFilePath)
    {

        buildingsPropertiesBuilded = new SaveProperties[cameraController.buildingsParent.childCount + cameraController.roadsParent.childCount];
        for (int i = 0; i < cameraController.buildingsParent.childCount + cameraController.roadsParent.childCount; i++)
            buildingsPropertiesBuilded[i] = new SaveProperties();

        for (int i = 0; i < cameraController.buildingsParent.childCount; i++)
        {
            buildingsPropertiesBuilded[i].index = cameraController.buildingsParent.GetChild(i).GetComponent<BuildingProperties>().buildingIndex;
            buildingsPropertiesBuilded[i].name = cameraController.buildingsParent.GetChild(i).name;
            try
            {
                buildingsPropertiesBuilded[i].timer = cameraController.buildingsParent.GetChild(i).GetComponent<BuildingProperties>().buildConstruction.timer;
            }
            catch
            {
                buildingsPropertiesBuilded[i].timer = 0.99f;
            }

            buildingsPropertiesBuilded[i].x = (int)cameraController.buildingsParent.GetChild(i).transform.position.x;
            buildingsPropertiesBuilded[i].y = (int)cameraController.buildingsParent.GetChild(i).transform.position.y;
            buildingsPropertiesBuilded[i].z = (int)cameraController.buildingsParent.GetChild(i).transform.position.z;

            buildingsPropertiesBuilded[i].rot_x = (int)cameraController.buildingsParent.GetChild(i).transform.localEulerAngles.x;
            buildingsPropertiesBuilded[i].rot_y = (int)cameraController.buildingsParent.GetChild(i).transform.localEulerAngles.y;
            buildingsPropertiesBuilded[i].rot_z = (int)cameraController.buildingsParent.GetChild(i).transform.localEulerAngles.z;
        }

        for (int i = 0; i < cameraController.roadsParent.childCount; i++)
        {
            buildingsPropertiesBuilded[cameraController.buildingsParent.childCount + i].index = cameraController.roadsParent.GetChild(i).GetComponent<RoadProperties>().buildingIndex;
            buildingsPropertiesBuilded[i].name = cameraController.roadsParent.GetChild(i).name;
            buildingsPropertiesBuilded[cameraController.buildingsParent.childCount + i].x = (int)cameraController.roadsParent.GetChild(i).transform.position.x;
            buildingsPropertiesBuilded[cameraController.buildingsParent.childCount + i].y = (int)cameraController.roadsParent.GetChild(i).transform.position.y;
            buildingsPropertiesBuilded[cameraController.buildingsParent.childCount + i].z = (int)cameraController.roadsParent.GetChild(i).transform.position.z;
        }

        SaveSystem.SaveBuildings(buildingsPropertiesBuilded, fileName);
    }

    public void BuildingDataLoad(string fileName = defaultFilePath)
    {
        bool spawnInitialization = true;
        cityLoadInitialized = false;

        data = SaveSystem.LoadBuildings(fileName);
        if (data != null)
        {
            for (int i = 0; i < data.length; i++)
            {
                building = null;
                if (data.buildingIndex[i] != 0 && data.buildingIndex[i] != 1 && data.buildingIndex[i] != 2 && data.buildingIndex[i] != 3 && data.buildingIndex[i] != 4 && data.buildingIndex[i] != 5)
                {
                    if (data.buildingIndex[i] != -1)
                    {
                        building = Instantiate(buildingsPropertiesForIndex[data.buildingIndex[i]].gameObject, new Vector3(0, 0, 0), Quaternion.identity);

                        SetPosition(i);
                        building.transform.localRotation = Quaternion.Euler(0, data.rotation[i][1], 0);

                        building.GetComponent<BuildingProperties>().buildConstruction.timer = data.timer[i];

                        cameraController.SpawnBuilding(building.transform, spawnInitialization);
                    }
                    else
                    {

                        Debug.LogError($"Building was not saved : {data.buildingIndex[i]}");
                    }
                }
                else
                {
                    building = Instantiate(road);
                    SetPosition(i);
                    cameraController.SpawnRoad(building.transform, spawnInitialization);
                    Destroy(cameraController.target.gameObject);
                }
            }
        }
        else
        {
            Debug.Log("Save not found");
        }

        cityLoadInitialized = true;
    }

    void SetPosition(int i)
    {
        Vector3 position;
        position.x = data.position[i][0];
        position.y = data.position[i][1];
        position.z = data.position[i][2];
        building.transform.position = position;
    }

}
