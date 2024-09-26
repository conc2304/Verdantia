using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CameraController : MonoBehaviour
{
    public Transform cameraHolder;
    public Transform cameraTransform;
    // public Camera cameraComponent;

    public Transform buildingsParent;
    public Transform roadsParent;
    public Transform citizensParent;
    public Transform carsParent;

    public bool moveTarget = false;
    [HideInInspector]
    public Transform target;
    public int gridSize = 2;

    public float normalSpeed;
    public float fastSpeed;
    public float movSpeed;
    private Vector3 toPos;

    private Vector3 dragStartPos;
    private Vector3 dragTargetPos;

    public float rotationScale;
    private Vector3 rotateStartPosition;
    private Vector3 rotateTargetPosition;
    private Quaternion toRot;
    private Quaternion mainCamtoRot;

    public Vector3 zoomScale;
    private Vector3 toZoom;
    public float minZoom;
    public float maxZoom;

    private BuildingsMenu buildingMenu;
    private RoadGenerator roadGenerator;
    private SaveDataTrigger saveDataTrigger;
    Spawner spawner;

    public List<Transform> allBuildings = new List<Transform>();

    private bool dontBuild;

    public Transform forest;
    List<Transform> forestObj = new List<Transform>();

    public bool doubleClick;
    public float lastClickTime;
    float catchTime = 0.3f;

    bool findPositionAfterMuiltyIputs;

    public GameObject activateMenu;

    private HeatMap heatMap;
    private bool cityChanged = false;

    public GameObject groundPlane;

    private float distanceToGround = 0f;
    private bool heatmapActive = false;


    void Awake()
    {
        buildingMenu = FindObjectOfType<BuildingsMenu>();
        roadGenerator = FindObjectOfType<RoadGenerator>();
        spawner = FindObjectOfType<Spawner>();
        saveDataTrigger = FindObjectOfType<SaveDataTrigger>();

        toPos = cameraHolder.transform.position;
        toRot = cameraHolder.transform.rotation;
        toZoom = cameraTransform.localPosition;

        for (int i = 0; i < forest.childCount; i++)
            forestObj.Add(forest.GetChild(i));

        heatMap = FindObjectOfType<HeatMap>();
        heatMap.UpdateHeatMap(allBuildings);
    }


    void Update()
    {
        MouseInput();
        KeyboardInput();
        SetPosition();

        if (Input.GetMouseButtonDown(0))
        {
            if (Time.time - lastClickTime < catchTime)
            {
                doubleClick = true;
            }
            else
            {
                doubleClick = false;
            }
            lastClickTime = Time.time;
        }
        if (heatMap != null && cityChanged) heatMap.UpdateHeatMap(allBuildings);

        cityChanged = false;
    }

    private void LateUpdate()
    {

        if (moveTarget)
        {
            float planeY = 0;
            Plane plane = new Plane(Vector3.up, Vector3.up * planeY); // ground plane
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            float distance; // the distance from the ray origin to the ray intersection of the plane
            if (plane.Raycast(ray, out distance))
            {
                // target.position = ray.GetPoint(distance).x;
                target.position = Vector3.Lerp(target.position, new Vector3(Mathf.Floor((ray.GetPoint(distance).x + gridSize / 2) / gridSize) * gridSize,
                   0, Mathf.Floor((ray.GetPoint(distance).z + gridSize / 2) / gridSize) * gridSize), Time.deltaTime * 50);
            }
        }

    }


    public void SpawnRoad(Transform targetNew)
    {
        targetNew.parent = roadsParent;
        targetNew.position = new Vector3((Mathf.Round(targetNew.position.x / 10)) * 10, 0, (Mathf.Round(targetNew.position.z / 10)) * 10);
        bool roadSpawnWasSuccessful = true;
        roadSpawnWasSuccessful = roadGenerator.CheckRoadType(targetNew);
        if (roadSpawnWasSuccessful)
        {

            for (int i = 0; i < forestObj.Count; i++)
            {
                if (forestObj[i].position == targetNew.position)
                {
                    forestObj[i].gameObject.SetActive(false);
                    break;
                }
            }

            Transform target1 = Instantiate(targetNew, new Vector3(0, 0, 0), Quaternion.identity).transform;
            target = target1;
            cityChanged = true;
            lastClickTime = 0;
        }
        doubleClick = false;
    }

    public void SpawnBuilding(Transform targetNew)
    {
        // Get Building Properties
        BuildingProperties targetBuildProp = targetNew.GetComponent<BuildingProperties>();


        doubleClick = false;
        lastClickTime = 0;

        targetNew.parent = buildingsParent;
        for (int i = 0; i < allBuildings.Count; i++)
        {
            if (Mathf.Round(allBuildings[i].position.x / 10) * 10 == Mathf.Round(targetNew.position.x / 10) * 10 &&
                Mathf.Round(allBuildings[i].position.z / 10) * 10 == Mathf.Round(targetNew.position.z / 10) * 10)
            {
                dontBuild = true;
                break;
            }

            for (int u = 0; u < targetBuildProp.additionalSpace.Length; u++)
            {
                if (Mathf.Round(allBuildings[i].position.x / 10) * 10 == Mathf.Round(targetBuildProp.additionalSpace[u].position.x / 10) * 10 &&
                Mathf.Round(allBuildings[i].position.z / 10) * 10 == Mathf.Round(targetBuildProp.additionalSpace[u].position.z / 10) * 10)
                {
                    dontBuild = true;
                    break;
                }
            }
        }
        for (int i = 0; i < roadGenerator.allRoads.Count; i++)
        {
            if (Mathf.Round(roadGenerator.allRoads[i].position.x / 10) * 10 == Mathf.Round(targetNew.position.x) &&
                Mathf.Round(roadGenerator.allRoads[i].position.z / 10) * 10 == Mathf.Round(targetNew.position.z))
            {
                dontBuild = true;
                break;
            }

            for (int u = 0; u < targetBuildProp.additionalSpace.Length; u++)
            {
                if (Mathf.Round(roadGenerator.allRoads[i].position.x / 10) * 10 == Mathf.Round(targetBuildProp.additionalSpace[u].position.x / 10) * 10 &&
                Mathf.Round(roadGenerator.allRoads[i].position.z / 10) * 10 == Mathf.Round(targetBuildProp.additionalSpace[u].position.z / 10) * 10)
                {
                    dontBuild = true;
                    break;
                }
            }
        }

        if (dontBuild == false)
        {
            for (int i = 0; i < forestObj.Count; i++)
            {
                if (forestObj[i].position == targetNew.position)
                {
                    forestObj[i].gameObject.SetActive(false);
                }

                for (int u = 0; u < targetBuildProp.additionalSpace.Length; u++)
                {

                    if (forestObj[i].position == targetBuildProp.additionalSpace[u].position)
                    {
                        forestObj[i].gameObject.SetActive(false);
                    }
                }
            }

            spawner.carsCount += 1;
            spawner.citizensCount += 2;
            allBuildings.Add(targetNew);

            for (int i = 0; i < targetBuildProp.additionalSpace.Length; i++)
                allBuildings.Add(targetBuildProp.additionalSpace[i].transform);

            targetNew.position = new Vector3((Mathf.Round(targetNew.position.x / 10)) * 10, 0, (Mathf.Round(targetNew.position.z / 10)) * 10);

            //if (target.GetComponent<BuildingProperties>().connectToRoad)
            //   roadGenerator.ConnectBuildingToRoad(target);

            //building animation
            BuildConstruction buildConstProp = targetBuildProp.buildConstruction.GetComponent<BuildConstruction>();
            buildConstProp.buildingProperties = targetBuildProp;
            buildConstProp.roadGenerator = roadGenerator;
            buildConstProp.target = targetBuildProp;
            buildConstProp.StartBuild();
            buildConstProp.cameraController = this;

            //menu
            buildingMenu.grid.enabled = false;
            moveTarget = false;
            target = null;
            cityChanged = true;
        }
    }



    void OnGUI()
    {
        if (doubleClick)
        {
            dontBuild = false;
            if (target != null)
            {
                if (target.CompareTag("Road"))              // spawn if road
                {
                    SpawnRoad(target);
                }
                else if (target.CompareTag("Building"))      //spawn if building
                {
                    SpawnBuilding(target);
                }
                else if (target.CompareTag("DeleteTool"))
                {
                    DeleteTarget(target);
                }
            }
        }
    }

    void DeleteTarget(Transform target)
    {
        doubleClick = false;
        lastClickTime = 0;

        for (int i = 0; i < allBuildings.Count; i++)
        {
            if (Mathf.Round(allBuildings[i].position.x / 10) * 10 == Mathf.Round(target.position.x / 10) * 10 &&
                Mathf.Round(allBuildings[i].position.z / 10) * 10 == Mathf.Round(target.position.z / 10) * 10)
            {
                cityChanged = true;

                for (int pathTargetIndex = 0; pathTargetIndex < allBuildings[i].GetComponent<BuildingProperties>().carsPathTargetsToConnect.Length; pathTargetIndex++)
                    spawner.carsSpawnPoints.Remove(allBuildings[i].GetComponent<BuildingProperties>().carsPathTargetsToConnect[pathTargetIndex].GetComponent<PathTarget>().previousPathTarget.transform);

                for (int pathTargetIndex = 0; pathTargetIndex < allBuildings[i].GetComponent<BuildingProperties>().citizensPathTargetsToSpawn.Length; pathTargetIndex++)
                    spawner.citizensSpawnPoints.Remove(allBuildings[i].GetComponent<BuildingProperties>().citizensPathTargetsToSpawn[pathTargetIndex]);

                if (allBuildings[i].CompareTag("Space"))
                {
                    BuildingProperties spaceBuildingProperty = allBuildings[i].parent.parent.GetComponent<BuildingProperties>();
                    for (int y = 0; y < spaceBuildingProperty.additionalSpace.Length; y++)
                    {
                        Destroy(spaceBuildingProperty.additionalSpace[y].gameObject);
                        allBuildings.Remove(spaceBuildingProperty.additionalSpace[y]);
                    }

                    Destroy(spaceBuildingProperty.gameObject);
                    allBuildings.Remove(spaceBuildingProperty.transform);
                    cityChanged = true;

                }
                else
                {
                    for (int y = 0; y < allBuildings[i].GetComponent<BuildingProperties>().additionalSpace.Length; y++)
                        allBuildings.Remove(allBuildings[i].GetComponent<BuildingProperties>().additionalSpace[y]);

                    Destroy(allBuildings[i].gameObject);
                    allBuildings.Remove(allBuildings[i]);
                    cityChanged = true;

                }

                break;
            }
        }
        for (int i = 0; i < roadGenerator.allRoads.Count; i++)
        {
            if (Mathf.Round(roadGenerator.allRoads[i].position.x / 10) * 10 == Mathf.Round(target.position.x) &&
                Mathf.Round(roadGenerator.allRoads[i].position.z / 10) * 10 == Mathf.Round(target.position.z))
            {
                Destroy(roadGenerator.allRoads[i].gameObject);
                roadGenerator.allRoads.Remove(roadGenerator.allRoads[i]);
                roadGenerator.DeleteRoad(target.transform);
                cityChanged = true;
                break;
            }
        }
    }

    void SetPosition()
    {
        if (activateMenu.activeSelf == false)
        {
            cameraHolder.transform.position = Vector3.Lerp(cameraHolder.transform.position, toPos, Time.deltaTime * 5);
        }

        toZoom.y = Mathf.Clamp(toZoom.y, -minZoom, !heatmapActive ? maxZoom : maxZoom * 2.5f);
        toZoom.z = Mathf.Clamp(toZoom.z, -maxZoom, minZoom);

        cameraHolder.transform.rotation = Quaternion.Lerp(cameraHolder.transform.rotation, toRot, Time.deltaTime * 5);
        cameraTransform.localRotation = Quaternion.Lerp(cameraTransform.localRotation, mainCamtoRot, Time.deltaTime * 5);
        cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition, toZoom, Time.deltaTime * 5);
    }

    void MouseInput()
    {
        //Scrolling
        if (Input.mouseScrollDelta.y != 0)
        {
            if (!heatmapActive)
            {
                toZoom += Input.mouseScrollDelta.y * zoomScale;
            }
            else
            {
                toZoom.y += Input.mouseScrollDelta.y * zoomScale.y;
            }
        }

        //Mouse movement
        if (Input.touchCount != 2 && (Mathf.FloorToInt(toRot.eulerAngles.y) - Mathf.FloorToInt(cameraHolder.transform.eulerAngles.y) < 8 && Mathf.FloorToInt(toRot.eulerAngles.y) - Mathf.FloorToInt(cameraHolder.transform.eulerAngles.y) > -10))
        {
            if (findPositionAfterMuiltyIputs)
            {
                Plane plane = new Plane(Vector3.up, Vector3.zero);
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                float entry;
                if (plane.Raycast(ray, out entry))
                {
                    dragStartPos = ray.GetPoint(entry);
                }

                findPositionAfterMuiltyIputs = false;
            }


            if (Input.GetMouseButtonDown(0))
            {
                Plane plane = new Plane(Vector3.up, Vector3.zero);
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                float entry;
                if (plane.Raycast(ray, out entry))
                {
                    dragStartPos = ray.GetPoint(entry);
                }
            }
            if (Input.GetMouseButton(0))
            {
                Plane plane = new Plane(Vector3.up, Vector3.zero);
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                float entry;
                if (plane.Raycast(ray, out entry))
                {
                    dragTargetPos = ray.GetPoint(entry);
                    toPos = cameraHolder.transform.position + dragStartPos - dragTargetPos;
                }
            }
        }

        //Mouse rotation
        if (Input.GetMouseButtonDown(2))
        {
            rotateStartPosition = Input.mousePosition;
        }
        if (Input.GetMouseButton(2))
        {
            rotateTargetPosition = Input.mousePosition;
            Vector3 difference = rotateStartPosition - rotateTargetPosition;
            rotateStartPosition = rotateTargetPosition;

            // When heatmapActive is false, apply the regular rotation (affecting all axes).
            // When heatmapActive is true, modify only the Y rotation.
            if (!heatmapActive)
            {
                // Regular rotation affecting all axes.
                toRot *= Quaternion.Euler(Vector3.up * (-difference.x / 5f));
            }
            else
            {
                // Only change the Y rotation when the heatmap is active.
                // Keep the X and Z rotation intact.
                float currentY = toRot.eulerAngles.y; // Get the current Y rotation
                float newYRotation = currentY + (-difference.x / 5f); // Calculate new Y rotation based on input

                // Set the rotation with the new Y value, and keep the other axes unchanged.
                toRot = Quaternion.Euler(toRot.eulerAngles.x, newYRotation, toRot.eulerAngles.z);
            }
        }

        //rotate building
        if (Input.GetMouseButtonDown(1))
        {
            if (target != null)
            {
                float y = target.rotation.eulerAngles.y + 90;
                target.rotation = Quaternion.Euler(0, y, 0);
            }
        }
    }

    void KeyboardInput()
    {
        //Shifting
        if (Input.GetKey(KeyCode.LeftShift))
            movSpeed = fastSpeed;
        else
            movSpeed = normalSpeed;

        //Movement
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            Vector3 forwardMovement = cameraHolder.transform.forward * movSpeed;
            if (heatmapActive) forwardMovement.y = 0; // Prevent Y-axis movement
            toPos += forwardMovement;
        }

        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            Vector3 backwardMovement = cameraHolder.transform.forward * -movSpeed;
            if (heatmapActive) backwardMovement.y = 0; // Prevent Y-axis movement
            toPos += backwardMovement;
        }

        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            Vector3 rightMovement = cameraHolder.transform.right * movSpeed;
            if (heatmapActive) rightMovement.y = 0; // Prevent Y-axis movement
            toPos += rightMovement;
        }

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            Vector3 leftMovement = cameraHolder.transform.right * -movSpeed;
            if (heatmapActive) leftMovement.y = 0; // Prevent Y-axis movement
            toPos += leftMovement;
        }

        //Rotation
        if (Input.GetKey(KeyCode.Q))
        {
            if (!heatmapActive)
            {
                // Regular rotation affecting all axes
                toRot *= Quaternion.Euler(Vector3.up * rotationScale);
            }
            else
            {
                // Only modify the Y-axis when the heatmap is active
                float currentY = toRot.eulerAngles.y; // Get current Y rotation
                float newYRotation = currentY + rotationScale; // Calculate new Y rotation
                toRot = Quaternion.Euler(toRot.eulerAngles.x, newYRotation, toRot.eulerAngles.z); // Apply new Y rotation
            }
        }


        if (Input.GetKey(KeyCode.E))
        {
            if (!heatmapActive)
            {
                // Regular rotation affecting all axes
                toRot *= Quaternion.Euler(Vector3.up * -rotationScale);
            }
            else
            {
                // Only modify the Y-axis when the heatmap is active
                float currentY = toRot.eulerAngles.y; // Get current Y rotation
                float newYRotation = currentY - rotationScale; // Calculate new Y rotation
                toRot = Quaternion.Euler(toRot.eulerAngles.x, newYRotation, toRot.eulerAngles.z); // Apply new Y rotation
            }
        }

        //Zooming
        if (Input.GetKey(KeyCode.R))
        {// in
            toZoom += zoomScale;
        }
        if (Input.GetKey(KeyCode.F))
            toZoom -= zoomScale;


        if (Input.GetKeyDown(KeyCode.Space))
        {
            ToggleHeatMapView();
        }
    }

    private void ToggleHeatMapView()
    {
        Debug.Log("ToggleHeatMapView");

        heatmapActive = !heatmapActive;
        heatMap.heatMapPlane.SetActive(heatmapActive);

        float topViewAngle = 90;
        float defaultAngle = 45;
        float nextRotAngle = heatmapActive ? topViewAngle : defaultAngle;
        print(nextRotAngle);
        mainCamtoRot = Quaternion.Euler(nextRotAngle, 0, 0); // Looking straight down

        // calculate distance from camera to ground plane
        // Assumes a 45 degree right angle triangle, distance from camera to ground will be the distance from the camera to the view target
        float d = distanceToGround = Math.Abs(cameraTransform.position.y - groundPlane.transform.position.y);
        print(distanceToGround);
        float hypotenuse = (float)Math.Sqrt(d * d + d * d);
        float diff = Math.Abs(hypotenuse - distanceToGround);
        // if going to active move forward        
        toPos += (heatmapActive ? 1f : -1f) * distanceToGround * cameraHolder.transform.forward;
        toPos += (heatmapActive ? 1f : -1f) * diff * cameraHolder.transform.up;

        // toPos.z += (hypotenuse - distanceToGround) * (heatmapActive ? 1f : -1f);
    }




    private void OnApplicationQuit()
    {
        saveDataTrigger.BuildingDataSave();
    }


}

