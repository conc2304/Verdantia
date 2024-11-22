using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadGenerator : MonoBehaviour
{

    private Transform newRoad, newRoadType;

    public Transform newRoadSaved;
    public List<Transform> allRoads = new List<Transform>();
    [HideInInspector]
    public Transform[] nearRoads = new Transform[4];
    [HideInInspector]
    public Transform[] nearBuildings = new Transform[8];
    [HideInInspector]
    public List<RoadProperties> nearRoadsProperties = new List<RoadProperties>();
    public LineRenderer electricityLine;
    RoadProperties newRoadSave;

    public Transform roadZero, roadOne, roadTwo, roadTwoTurn, roadThree, roadFour;

    bool top, down, left, right;
    int countWays;
    int roadRotationY;

    CameraController cameraController;
    Spawner spawner;

    private void Awake()
    {
        nearRoads = new Transform[4];
        nearBuildings = new Transform[8];
        cameraController = FindObjectOfType<CameraController>();
        spawner = FindObjectOfType<Spawner>();
    }

    // This gets called by the camera controller on road spawn
    public bool CheckRoadType(Transform roadForSpawn)
    {
        // road will be RoadForSpawn prefab

        top = down = right = left = false;
        countWays = 0;
        roadRotationY = 0;
        for (int i = 0; i < nearRoads.Length; i++)
            nearRoads[i] = null;
        for (int i = 0; i < nearBuildings.Length; i++)
            nearBuildings[i] = null;
        nearRoadsProperties.Clear();

        //find another roads
        for (int i = 0; i < allRoads.Count; i++)
        {
            if (Mathf.Round(allRoads[i].position.x) == Mathf.Round(roadForSpawn.position.x) && Mathf.Round(allRoads[i].position.z) == Mathf.Round(roadForSpawn.position.z - 10))
            {
                top = true;
                nearRoads[0] = allRoads[i];
            }
            if (Mathf.Round(allRoads[i].position.x) == Mathf.Round(roadForSpawn.position.x) && Mathf.Round(allRoads[i].position.z) == Mathf.Round(roadForSpawn.position.z + 10))
            {
                down = true;
                nearRoads[1] = allRoads[i];
            }
            if (Mathf.Round(allRoads[i].position.x) - 10 == Mathf.Round(roadForSpawn.position.x) && Mathf.Round(allRoads[i].position.z) == Mathf.Round(roadForSpawn.position.z))
            {
                right = true;
                nearRoads[2] = allRoads[i];
            }
            if (Mathf.Round(allRoads[i].position.x) + 10 == Mathf.Round(roadForSpawn.position.x) && Mathf.Round(allRoads[i].position.z) == Mathf.Round(roadForSpawn.position.z))
            {
                left = true;
                nearRoads[3] = allRoads[i];
            }
            if (Mathf.Round(allRoads[i].position.x) == Mathf.Round(roadForSpawn.position.x) && allRoads[i].position.z == Mathf.Round(roadForSpawn.position.z))
            {
                return false;
            }
        }

        for (int i = 0; i < cameraController.allBuildings.Count; i++)
        {
            if (Mathf.Round(cameraController.allBuildings[i].transform.position.x) == Mathf.Round(roadForSpawn.position.x) &&
                        Mathf.Round(cameraController.allBuildings[i].transform.position.z) == Mathf.Round(roadForSpawn.position.z))
            {
                return false;
            }
        }

        RotateRoad(); // Updates which prefab newRoad is

        //final procedure
        newRoadType = newRoad;
        newRoad = Instantiate(newRoad, roadForSpawn.position, Quaternion.Euler(roadForSpawn.rotation.eulerAngles.x, roadRotationY, roadForSpawn.rotation.eulerAngles.z), cameraController.roadsParent);
        newRoadSaved = newRoad;
        //nearRoadsProperties.Add(newRoad.GetComponent<RoadProperties>());

        roadForSpawn.GetComponent<BuildingProperties>().TransferBuildingProperties(newRoad.GetComponent<BuildingProperties>());
        allRoads.Remove(roadForSpawn);
        allRoads.Add(newRoad);

        Destroy(roadForSpawn.gameObject);
        //road.gameObject.SetActive(false);

        // CheckCreatedRoads will call rotateRoad()
        // repair near roads
        for (int i = 0; i < nearRoads.Length; i++)
            if (nearRoads[i] != null)
                CheckCreatedRoads(nearRoads[i], i);     // TODO CHECK THIS !! HERE

        CloseOfCloseRoads();
        for (int i = 0; i < nearRoads.Length; i++)
            if (nearRoads[i] != null)
                ConnectRoads(nearRoads[i].GetComponent<RoadProperties>());

        for (int i = 0; i < nearRoadsProperties.Count; i++)
            ConnectElectricalPillar(nearRoadsProperties[i]);

        for (int i = 0; i < allRoads.Count; i++)
            CheckToConnectBuildingToRoad(allRoads[i]);
        for (int u = 0; u < nearBuildings.Length; u++)
            if (nearBuildings[u] != null && nearBuildings[u].name != "BuildingsParent")
                if (nearBuildings[u].GetComponent<BuildingProperties>().buildConstruction == null)
                    ConnectBuildingToRoad(nearBuildings[u]);

        return true;
    }

    void RotateRoad()
    {
        //count near roads
        if (top) countWays += 1;
        if (down) countWays += 1;
        if (right) countWays += 1;
        if (left) countWays += 1;

        if (countWays == 0) newRoad = roadZero;
        if (countWays == 1) newRoad = roadOne;
        if (countWays == 2) newRoad = roadTwo;
        if (countWays == 3) newRoad = roadThree;
        if (countWays == 4) newRoad = roadFour;

        //choose road type
        if (newRoad == roadTwo)
        {
            if (top && down) newRoad = roadTwo;
            if (right && left) newRoad = roadTwo;

            if (right && down) newRoad = roadTwoTurn;
            if (left && down) newRoad = roadTwoTurn;
            if (top && right) newRoad = roadTwoTurn;
            if (top && left) newRoad = roadTwoTurn;
        }

        //rotation
        if (newRoad == roadOne)
        {
            if (right) roadRotationY = -90;
            if (left) roadRotationY = 90;
            if (down) roadRotationY = 180;
        }
        if (newRoad == roadTwo)
        {
            if (left && right) roadRotationY = 90;
        }
        if (newRoad == roadTwoTurn)
        {
            if (top && right) roadRotationY = 180;
            if (top && left) roadRotationY = -90;
            if (down && right) roadRotationY = 90;
            if (down && left) roadRotationY = 0;
        }
        if (newRoad == roadThree)
        {
            if (top && down && right) roadRotationY = 90;
            if (top && down && left) roadRotationY = -90;
            if (top && right && left) roadRotationY = 180;
            if (down && right && left) roadRotationY = 0;
        }
    }

    void CheckCreatedRoads(Transform road, int u)
    {
        top = down = right = left = false;
        countWays = 0;
        roadRotationY = 0;

        //find another roads
        for (int i = 0; i < allRoads.Count; i++)
        {
            if (Mathf.Round(allRoads[i].transform.position.x) == Mathf.Round(road.position.x) && Mathf.Round(allRoads[i].transform.position.z) == Mathf.Round(road.position.z - 10))
            {
                top = true;
                nearRoadsProperties.Add(allRoads[i].GetComponent<RoadProperties>());
            }
            if (Mathf.Round(allRoads[i].transform.position.x) == Mathf.Round(road.position.x) && Mathf.Round(allRoads[i].transform.position.z) == Mathf.Round(road.position.z + 10))
            {
                down = true;
                nearRoadsProperties.Add(allRoads[i].GetComponent<RoadProperties>());
            }
            if (Mathf.Round(allRoads[i].transform.position.x) - 10 == Mathf.Round(road.position.x) && Mathf.Round(allRoads[i].transform.position.z) == Mathf.Round(road.position.z))
            {
                right = true;
                nearRoadsProperties.Add(allRoads[i].GetComponent<RoadProperties>());
            }
            if (Mathf.Round(allRoads[i].transform.position.x) + 10 == Mathf.Round(road.position.x) && Mathf.Round(allRoads[i].transform.position.z) == Mathf.Round(road.position.z))
            {
                left = true;
                nearRoadsProperties.Add(allRoads[i].GetComponent<RoadProperties>());
            }
        }

        RotateRoad();

        //final procedure
        newRoad = Instantiate(newRoad, road.position, Quaternion.Euler(road.rotation.eulerAngles.x, roadRotationY, road.rotation.eulerAngles.z), cameraController.roadsParent);
        nearRoads[u] = newRoad;
        nearRoadsProperties.Add(newRoad.GetComponent<RoadProperties>());

        // Since we are deleting an old road to but in the correct type,
        // We want to transfer all of the metrics to the new road
        road.GetComponent<BuildingProperties>().TransferBuildingProperties(newRoad.GetComponent<BuildingProperties>());

        allRoads.Remove(road);
        allRoads.Add(newRoad);
        Destroy(road.gameObject);
    }

    void ConnectRoads(RoadProperties roadProperties)
    {
        if (newRoadSave = roadProperties)
        {
            for (int pathTargetIndex = 0; pathTargetIndex < roadProperties.pathTargetsLast.Length; pathTargetIndex++)
            {

                float newDistanceStart = 0;
                float distanceStart = 1000;
                PathTarget closerRoadStart = null;

                float newDistanceLast = 0;
                float distanceLast = 1000;
                PathTarget closerRoadLast = null;

                for (int roadIndex = 0; roadIndex < nearRoadsProperties.Count; roadIndex++)
                {

                    //start
                    for (int pathIndex = 0; pathIndex < nearRoadsProperties[roadIndex].pathTargetsStart.Length; pathIndex++)
                    {
                        newDistanceStart = Vector3.Distance(nearRoadsProperties[roadIndex].pathTargetsStart[pathIndex].transform.position,
                            roadProperties.pathTargetsLast[pathTargetIndex].transform.position);
                        if (newDistanceStart < distanceStart)
                        {

                            distanceStart = newDistanceStart;
                            closerRoadStart = nearRoadsProperties[roadIndex].pathTargetsStart[pathIndex];
                        }
                    }
                    //last
                    for (int pathIndex = 0; pathIndex < nearRoadsProperties[roadIndex].pathTargetsLast.Length; pathIndex++)
                    {

                        newDistanceLast = Vector3.Distance(nearRoadsProperties[roadIndex].pathTargetsLast[pathIndex].transform.position,
                            roadProperties.pathTargetsStart[pathTargetIndex].transform.position);
                        if (newDistanceLast < distanceLast)
                        {
                            distanceLast = newDistanceLast;
                            closerRoadLast = nearRoadsProperties[roadIndex].pathTargetsLast[pathIndex];
                        }
                    }

                }

                roadProperties.pathTargetsLast[pathTargetIndex].nextPathTarget = closerRoadStart;
                closerRoadStart.previousPathTarget = roadProperties.pathTargetsLast[pathTargetIndex];

                roadProperties.pathTargetsStart[pathTargetIndex].previousPathTarget = closerRoadLast;
                closerRoadLast.nextPathTarget = roadProperties.pathTargetsStart[pathTargetIndex];

            }

        }
    }

    public void CheckToConnectBuildingToRoad(Transform road)
    {
        for (int i = 0; i < cameraController.allBuildings.Count; i++)
        {
            if (Mathf.Round(cameraController.allBuildings[i].position.x) == Mathf.Round(road.position.x) &&
                Mathf.Round(cameraController.allBuildings[i].position.z) == Mathf.Round(road.position.z - 10))
                nearBuildings[0] = cameraController.allBuildings[i];
            if (Mathf.Round(cameraController.allBuildings[i].position.x) == Mathf.Round(road.position.x) &&
                Mathf.Round(cameraController.allBuildings[i].position.z) == Mathf.Round(road.position.z + 10))
                nearBuildings[1] = cameraController.allBuildings[i];
            if (Mathf.Round(cameraController.allBuildings[i].position.x) - 10 == Mathf.Round(road.position.x) &&
                Mathf.Round(cameraController.allBuildings[i].position.z) == Mathf.Round(road.position.z))
                nearBuildings[2] = cameraController.allBuildings[i];
            if (Mathf.Round(cameraController.allBuildings[i].position.x) + 10 == Mathf.Round(road.position.x) &&
                Mathf.Round(cameraController.allBuildings[i].position.z) == Mathf.Round(road.position.z))
                nearBuildings[3] = cameraController.allBuildings[i];
            if (Mathf.Round(cameraController.allBuildings[i].position.x) == Mathf.Round(road.position.x) &&
                Mathf.Round(cameraController.allBuildings[i].position.z) == Mathf.Round(road.position.z - 20))
                nearBuildings[4] = cameraController.allBuildings[i];
            if (Mathf.Round(cameraController.allBuildings[i].position.x) == Mathf.Round(road.position.x) &&
                Mathf.Round(cameraController.allBuildings[i].position.z) == Mathf.Round(road.position.z + 20))
                nearBuildings[5] = cameraController.allBuildings[i];
            if (Mathf.Round(cameraController.allBuildings[i].position.x) - 20 == Mathf.Round(road.position.x) &&
                Mathf.Round(cameraController.allBuildings[i].position.z) == Mathf.Round(road.position.z))
                nearBuildings[6] = cameraController.allBuildings[i];
            if (Mathf.Round(cameraController.allBuildings[i].position.x) + 20 == Mathf.Round(road.position.x) &&
                Mathf.Round(cameraController.allBuildings[i].position.z) == Mathf.Round(road.position.z))
                nearBuildings[7] = cameraController.allBuildings[i];
        }

        for (int i = 0; i < nearBuildings.Length; i++)
        {
            if (nearBuildings[i] != null)
            {
                if (nearBuildings[i].tag == "Space")
                {
                    nearBuildings[i] = nearBuildings[i].transform.parent.parent;
                }
            }
        }


    }

    public void ConnectBuildingToRoad(Transform building)
    {

        //delete connection to roads
        for (int i = 0; i < nearBuildings.Length; i++)
        {
            if (nearBuildings[i] != null && nearBuildings[i].name != "BuildingsParent")
            {
                for (int u = 0; u < nearBuildings[i].GetComponent<BuildingProperties>().carsPathTargetsToSpawn.Length; u++)
                {
                    spawner.carsSpawnPoints.Remove(nearBuildings[i].GetComponent<BuildingProperties>().carsPathTargetsToSpawn[u]);
                }
                for (int u = 0; u < nearBuildings[i].GetComponent<BuildingProperties>().citizensPathTargetsToSpawn.Length; u++)
                {
                    spawner.citizensSpawnPoints.Remove(nearBuildings[i].GetComponent<BuildingProperties>().citizensPathTargetsToSpawn[u]);
                }
            }
        }

        BuildingProperties buildingPathTarget = building.GetComponent<BuildingProperties>();
        nearRoadsProperties.Clear();
        for (int i = 0; i < nearRoads.Length; i++)
            nearRoads[i] = null;

        for (int y = -1; y < buildingPathTarget.additionalSpace.Length; y++)
        {
            for (int i = 0; i < allRoads.Count; i++)
            {
                if (Mathf.Round(allRoads[i].position.x) == Mathf.Round(building.position.x) && Mathf.Round(allRoads[i].position.z) == Mathf.Round(building.position.z - 10))
                    nearRoadsProperties.Add(allRoads[i].GetComponent<RoadProperties>());
                if (Mathf.Round(allRoads[i].position.x) == Mathf.Round(building.position.x) && Mathf.Round(allRoads[i].position.z) == Mathf.Round(building.position.z + 10))
                    nearRoadsProperties.Add(allRoads[i].GetComponent<RoadProperties>());
                if (Mathf.Round(allRoads[i].position.x) - 10 == Mathf.Round(building.position.x) && Mathf.Round(allRoads[i].position.z) == Mathf.Round(building.position.z))
                    nearRoadsProperties.Add(allRoads[i].GetComponent<RoadProperties>());
                if (Mathf.Round(allRoads[i].position.x) + 10 == Mathf.Round(building.position.x) && Mathf.Round(allRoads[i].position.z) == Mathf.Round(building.position.z))
                    nearRoadsProperties.Add(allRoads[i].GetComponent<RoadProperties>());
            }
            if (y > 0) building = buildingPathTarget.additionalSpace[y];
        }

        //cars roads connect
        bool shoudSpawnCitizens = false;
        bool shoudSpawnCars = false;
        for (int pathTargetIndex = 0; pathTargetIndex < buildingPathTarget.carsPathTargetsToConnect.Length; pathTargetIndex++)
        {

            float newDistance = 0;
            float distance = 1000;
            PathTarget closerTarget = null;

            for (int nearRoadIndex = 0; nearRoadIndex < nearRoadsProperties.Count; nearRoadIndex++)
            {
                for (int pathIndex = 0; pathIndex < nearRoadsProperties[nearRoadIndex].pathToConnectBuilding.Length; pathIndex++)
                {
                    newDistance = Vector3.Distance(nearRoadsProperties[nearRoadIndex].pathToConnectBuilding[pathIndex].transform.position,
                        buildingPathTarget.carsPathTargetsToConnect[pathTargetIndex].transform.position);
                    if (newDistance < distance)
                    {
                        distance = newDistance;
                        closerTarget = nearRoadsProperties[nearRoadIndex].pathToConnectBuilding[pathIndex];
                    }
                }
            }

            buildingPathTarget.carsPathTargetsToConnect[pathTargetIndex].GetComponent<PathTarget>().branches.Clear();
            if (distance < 5.5f)
            {
                if (closerTarget != null)
                {
                    shoudSpawnCars = true;
                    buildingPathTarget.carsPathTargetsToConnect[pathTargetIndex].GetComponent<PathTarget>().branches.Add(closerTarget);
                    buildingPathTarget.carsPathTargetsToConnect[pathTargetIndex].GetComponent<PathTarget>().branchesRatio = 1;
                }
            }
        }

        //citizens roads connect
        for (int pathTargetIndex = 0; pathTargetIndex < buildingPathTarget.citizensPathTargetsToConnect.Length; pathTargetIndex++)
        {
            float newDistance = 0;
            float distance = 1000;
            PathTarget closerTarget = null;

            for (int nearRoadIndex = 0; nearRoadIndex < nearRoadsProperties.Count; nearRoadIndex++)
            {
                for (int pathIndex = 0; pathIndex < nearRoadsProperties[nearRoadIndex].citizensPathToConnectBuilding.Length; pathIndex++)
                {
                    newDistance = Vector3.Distance(nearRoadsProperties[nearRoadIndex].citizensPathToConnectBuilding[pathIndex].transform.position,
                        buildingPathTarget.citizensPathTargetsToConnect[pathTargetIndex].transform.position);
                    if (newDistance < distance)
                    {
                        distance = newDistance;
                        closerTarget = nearRoadsProperties[nearRoadIndex].citizensPathToConnectBuilding[pathIndex];
                    }
                }
            }

            buildingPathTarget.citizensPathTargetsToConnect[pathTargetIndex].GetComponent<PathTarget>().branches.Clear();

            if (distance < 5)
            {
                if (closerTarget != null)
                {
                    shoudSpawnCitizens = true;
                    buildingPathTarget.citizensPathTargetsToConnect[pathTargetIndex].GetComponent<PathTarget>().branches.Add(closerTarget);
                    closerTarget.branches.Add(buildingPathTarget.citizensPathTargetsToConnect[pathTargetIndex].GetComponent<PathTarget>());
                }
            }
        }

        if (shoudSpawnCitizens)
            for (int i = 0; i < buildingPathTarget.citizensPathTargetsToSpawn.Length; i++)
                spawner.citizensSpawnPoints.Add(buildingPathTarget.citizensPathTargetsToSpawn[i]);

        if (shoudSpawnCars)
            for (int i = 0; i < buildingPathTarget.carsPathTargetsToSpawn.Length; i++)
                //if(spawner.carsSpawnPoints.Contains(buildingPathTarget.carsPathTargetsToSpawn[i].transform) == false)
                spawner.carsSpawnPoints.Add(buildingPathTarget.carsPathTargetsToSpawn[i]);

    }

    public void ConnectElectricalPillar(RoadProperties roadProperties)
    {
        for (int connectIndex = 0; connectIndex < roadProperties.startTargetsToConnect.Length; connectIndex++)
        {

            if (roadProperties.startTargetsToConnect[connectIndex].transform.childCount == 0)
            {
                float newDistance = 0;
                float distance = 1000;
                Transform closerLastRoad = null;
                LineRenderer line = Instantiate(electricityLine);

                for (int nearRoadIndex = 0; nearRoadIndex < nearRoadsProperties.Count; nearRoadIndex++)
                {
                    if (nearRoadsProperties[nearRoadIndex] != roadProperties)
                    {
                        for (int targetsIndex = 0; targetsIndex < nearRoadsProperties[nearRoadIndex].targetsToConnect.Length; targetsIndex++)
                        {
                            newDistance = Vector3.Distance(roadProperties.startTargetsToConnect[connectIndex].transform.position,
                                nearRoadsProperties[nearRoadIndex].targetsToConnect[targetsIndex].transform.position);
                            if (newDistance < distance && newDistance > 0.1f)
                            {
                                closerLastRoad = nearRoadsProperties[nearRoadIndex].targetsToConnect[targetsIndex].transform;
                                distance = newDistance;
                            }
                        }
                    }
                }

                line.SetPosition(0, roadProperties.startTargetsToConnect[connectIndex].transform.position);
                line.SetPosition(1, closerLastRoad.position);

                line.transform.parent = roadProperties.startTargetsToConnect[connectIndex].transform;
            }
        }
    }

    public void DeleteRoad(Transform road)
    {
        top = down = right = left = false;
        countWays = 0;
        roadRotationY = 0;
        for (int i = 0; i < nearRoads.Length; i++)
            nearRoads[i] = null;
        for (int i = 0; i < nearBuildings.Length; i++)
            nearBuildings[i] = null;
        nearRoadsProperties.Clear();

        //find another roads
        for (int i = 0; i < allRoads.Count; i++)
        {
            if (Mathf.Round(allRoads[i].position.x) == Mathf.Round(road.position.x) && Mathf.Round(allRoads[i].position.z) == Mathf.Round(road.position.z - 10))
            {
                top = true;
                nearRoads[0] = allRoads[i];
            }
            if (Mathf.Round(allRoads[i].position.x) == Mathf.Round(road.position.x) && Mathf.Round(allRoads[i].position.z) == Mathf.Round(road.position.z + 10))
            {
                down = true;
                nearRoads[1] = allRoads[i];
            }
            if (Mathf.Round(allRoads[i].position.x) - 10 == Mathf.Round(road.position.x) && Mathf.Round(allRoads[i].position.z) == Mathf.Round(road.position.z))
            {
                right = true;
                nearRoads[2] = allRoads[i];
            }
            if (Mathf.Round(allRoads[i].position.x) + 10 == Mathf.Round(road.position.x) && Mathf.Round(allRoads[i].position.z) == Mathf.Round(road.position.z))
            {
                left = true;
                nearRoads[3] = allRoads[i];
            }
        }


        RotateRoad();


        //repair near roads
        for (int i = 0; i < nearRoads.Length; i++)
            if (nearRoads[i] != null)
                CheckCreatedRoads(nearRoads[i], i);

        for (int i = 0; i < nearRoads.Length; i++)
            if (nearRoads[i] != null)
                ConnectRoads(nearRoads[i].GetComponent<RoadProperties>());

        //buidings connections
        for (int i = 0; i < allRoads.Count; i++)
            CheckToConnectBuildingToRoad(allRoads[i]);
        for (int u = 0; u < nearBuildings.Length; u++)
            if (nearBuildings[u] != null)
                ConnectBuildingToRoad(nearBuildings[u]);
    }

    void CloseOfCloseRoads()
    {
        for (int i = 0; i < allRoads.Count; i++)
        {
            for (int y = 0; y < nearRoadsProperties.Count; y++)
            {
                if (Mathf.Round(allRoads[i].position.x) == Mathf.Round(nearRoadsProperties[y].transform.position.x) && Mathf.Round(allRoads[i].position.z) == Mathf.Round(nearRoadsProperties[y].transform.position.z - 10))
                    if (nearRoadsProperties.Contains(allRoads[i].GetComponent<RoadProperties>()) == false)
                        nearRoadsProperties.Add(allRoads[i].GetComponent<RoadProperties>());
                if (Mathf.Round(allRoads[i].position.x) == Mathf.Round(nearRoadsProperties[y].transform.position.x) && Mathf.Round(allRoads[i].position.z) == Mathf.Round(nearRoadsProperties[y].transform.position.z + 10))
                    if (nearRoadsProperties.Contains(allRoads[i].GetComponent<RoadProperties>()) == false)
                        nearRoadsProperties.Add(allRoads[i].GetComponent<RoadProperties>());
                if (Mathf.Round(allRoads[i].position.x) - 10 == Mathf.Round(nearRoadsProperties[y].transform.position.x) && Mathf.Round(allRoads[i].position.z) == Mathf.Round(nearRoadsProperties[y].transform.position.z))
                    if (nearRoadsProperties.Contains(allRoads[i].GetComponent<RoadProperties>()) == false)
                        nearRoadsProperties.Add(allRoads[i].GetComponent<RoadProperties>());
                if (Mathf.Round(allRoads[i].position.x) + 10 == Mathf.Round(nearRoadsProperties[y].transform.position.x) && Mathf.Round(allRoads[i].position.z) == Mathf.Round(nearRoadsProperties[y].transform.position.z))
                    if (nearRoadsProperties.Contains(allRoads[i].GetComponent<RoadProperties>()) == false)
                        nearRoadsProperties.Add(allRoads[i].GetComponent<RoadProperties>());
            }
        }
    }

}

