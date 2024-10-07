using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CameraController : MonoBehaviour
{
    public Transform cameraHolder;
    public Transform cameraTransform;

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
    public Slider zoomSlider;

    public int[] xRange = new int[2] { 0, 1000 };
    public int[] zRange = new int[2] { 0, 1000 };

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

    public Grid grid;
    private HeatMap heatMap;
    private bool cityChanged = false;

    public GameObject groundPlane;

    private bool heatmapActive = false;

    private string heatmapMetric = "heatContribution";

    public FixedJoystick fixedJoystick;

    private bool skipMouseInput = false;


    void Start()
    {
        // Set the callback for when the slider value changes
        zoomSlider.onValueChanged.AddListener(OnSliderValueChanged);

    }


    void Awake()
    {
        buildingMenu = FindObjectOfType<BuildingsMenu>();
        roadGenerator = FindObjectOfType<RoadGenerator>();
        spawner = FindObjectOfType<Spawner>();
        saveDataTrigger = FindObjectOfType<SaveDataTrigger>();

        toPos = cameraHolder.transform.position;
        toRot = cameraHolder.transform.rotation;
        toZoom = cameraTransform.localPosition;
        zoomSlider.minValue = minZoom;
        zoomSlider.maxValue = maxZoom;
        zoomSlider.value = toZoom.y;

        for (int i = 0; i < forest.childCount; i++)
            forestObj.Add(forest.GetChild(i));

        heatMap = FindObjectOfType<HeatMap>();

        if (grid != null)
        {
            xRange = new int[2] { 0, grid.gridSizeX };
            zRange = new int[] { 0, grid.gridSizeZ };
        }
    }


    void Update()
    {
        TouchInput();

        if (!skipMouseInput) MouseInput();
        KeyboardInput();
        SetPosition();


        // Check for double click 
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

        // handle heat map updates on city change 
        if (heatMap != null && cityChanged)
        {
            int metricMin = buildingMenu.propertyRanges[heatmapMetric].min;
            int metricMax = buildingMenu.propertyRanges[heatmapMetric].max;
            heatMap.UpdateHeatMap(allBuildings, heatmapMetric, metricMin, metricMax);
        }

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
        // Limit how far a user can scroll 
        toPos.x = Mathf.Clamp(toPos.x, xRange[0], xRange[1]);
        toPos.z = Mathf.Clamp(toPos.z, zRange[0], zRange[1]);

        if (activateMenu.activeSelf == false)
        {
            cameraHolder.transform.position = Vector3.Lerp(
                cameraHolder.transform.position,
                toPos,
                Time.deltaTime * 5
            );
        }


        toZoom.y = Mathf.Clamp(toZoom.y, -minZoom, !heatmapActive ? maxZoom : maxZoom + 200);
        toZoom.z = Mathf.Clamp(toZoom.z, -maxZoom, minZoom);

        // Rotates the camera holder so that its always pointed at the target
        cameraHolder.transform.rotation = Quaternion.Lerp(
            cameraHolder.transform.rotation,
            toRot,
            Time.deltaTime * 5
        );

        // Use this to point the camera downward for heat map view and back
        cameraTransform.localRotation = Quaternion.Lerp(
            cameraTransform.localRotation,
            mainCamtoRot,
            Time.deltaTime * 5
        );

        // Positions the main camera locally inside the cameraHolder
        cameraTransform.localPosition = Vector3.Lerp(
            cameraTransform.localPosition,
            toZoom,
            Time.deltaTime * 5
        );
    }

    private void TouchInput()
    {
        // Handle Joystick input for camera movement L/R/U/D
        Vector3 direction = cameraHolder.transform.forward * fixedJoystick.Vertical + cameraHolder.transform.right * fixedJoystick.Horizontal;
        float magnitude = direction.magnitude * 60; // the further the joy stick is the faster they move
        // skipMouseInput = Input.touchCount >= 1;
        Vector3 movement = magnitude * Time.deltaTime * direction;
        if (heatmapActive) movement.y = 0; // Prevent Y-axis movement
        toPos += movement;
    }

    void MouseInput()
    {
        if (Input.touchCount >= 1) return;

        //Scrolling
        if (Input.mouseScrollDelta.y != 0) // Vector3(0, -10, 10)
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
        if (Input.touchCount != 2 && Mathf.FloorToInt(toRot.eulerAngles.y) - Mathf.FloorToInt(cameraHolder.transform.eulerAngles.y) < 8 && Mathf.FloorToInt(toRot.eulerAngles.y) - Mathf.FloorToInt(cameraHolder.transform.eulerAngles.y) > -10)
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

            // When heatmap is Not Active, apply the regular rotation (affecting all axes).
            // When heatmap IS Active, modify only the Y rotation.
            if (!heatmapActive)
            {
                // Regular rotation affecting Y and Z
                toRot *= Quaternion.Euler(Vector3.up * (-difference.x / 5f));
            }
            else
            {
                Vector3 rot = mainCamtoRot.eulerAngles;
                float currentY = rot.y;
                float newYRotation = currentY + (-difference.x / 5f);
                toRot *= Quaternion.Euler(0, newYRotation, 0);
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
        if (Input.GetKey(KeyCode.Q)) RotateCamera(-rotationScale);
        if (Input.GetKey(KeyCode.E)) RotateCamera(rotationScale);

        // Zooming
        if (Input.GetKey(KeyCode.R)) ZoomIn(1);
        if (Input.GetKey(KeyCode.F)) ZoomOut(1); ;


        if (Input.GetKeyDown(KeyCode.Space))
        {
            ToggleHeatMapView();
        }
    }

    private Quaternion RotateCamera(float rotAmount)
    {
        if (!heatmapActive)
        {
            // Regular rotation affecting all axrotAmount
            toRot *= Quaternion.Euler(Vector3.up * rotAmount);
        }
        else
        {
            // Only modify the Y-axis when the heatmap is active
            float currentY = toRot.eulerAngles.y; // Get current Y rotation
            float newYRotation = currentY + rotAmount; // Calculate new Y rotation
            toRot = Quaternion.Euler(toRot.eulerAngles.x, newYRotation, toRot.eulerAngles.z); // Apply new Y rotation
        }

        return toRot;
    }


    // Public Methods for the onClick() button handlers on the touch screen display
    public void RotateCameraLeft() { RotateCamera(rotationScale * 10); }
    public void RotateCameraRight() { RotateCamera(-rotationScale * 10); }
    public void ZoomIn(float multiplier = 1)
    {
        toZoom += zoomScale * multiplier;
        SetSliderValueWithoutCallback(toZoom.y);
    }
    public void ZoomOut(float multiplier = 1)
    {
        toZoom -= zoomScale * multiplier;
        SetSliderValueWithoutCallback(toZoom.y);
    }
    public void OnSliderValueChanged(float zoomAmount)
    {
        toZoom.y = zoomAmount;
        toZoom.z = -zoomAmount;
    }

    public void SetSliderValueWithoutCallback(float newValue)
    {
        zoomSlider.onValueChanged.RemoveListener(OnSliderValueChanged);
        zoomSlider.value = newValue;
        zoomSlider.onValueChanged.AddListener(OnSliderValueChanged);
    }

    public void SetHeatMapView(bool isActive)
    {
        heatmapActive = isActive;
        UpdateHeatMapView();
    }

    public void ToggleHeatMapView()
    {
        heatmapActive = !heatmapActive;
        UpdateHeatMapView();

    }
    public void UpdateHeatMapView()
    {
        // Updates the camera angle to point down at 90 or out at 45
        // Update the Y and Z position of the camera to keep the current view in focus 
        // TODO - handle with a fade in or something
        heatMap.heatMapPlane.SetActive(heatmapActive);

        float topViewAngle = 90;
        float defaultAngle = 45;
        float nextRotAngle = heatmapActive ? topViewAngle : defaultAngle;
        // Update X angle to point either straight down or out at a 45 angle
        mainCamtoRot = Quaternion.Euler(nextRotAngle, 0, 0);

        // Assumes a 45 degree right angle triangle based on the initial camera angle of x = 45, 
        // In Default Mode : The distance from camera to ground is the leg of a right triangle to calculate the hypotenuse
        // In Heatmap Mode : The distance from the camera to the ground is the future hypotenuse of the triangle to calculate the leg/height
        // Then the vertical difference is how much to move the camera upwards so that we maintain the same distance to the target
        float distanceToGround;
        float d = distanceToGround = Math.Abs(cameraTransform.position.y - groundPlane.transform.position.y);
        float hypotenuse = heatmapActive ? (float)Math.Sqrt(d * d + d * d) : distanceToGround;
        d = hypotenuse == distanceToGround ? (float)Math.Sqrt(Math.Pow(hypotenuse, 2) / 2) : d;
        float vertDiff = Math.Abs(hypotenuse - d);

        Vector3 localPos = cameraTransform.localPosition;

        if (heatmapActive)
        {
            toZoom = new Vector3(localPos.x, localPos.y + vertDiff, 0);
        }
        else
        {
            // Going into default mode where pos Y and Z are inverses of eachother based on the zoomScale
            float scaleConverter = zoomScale.y / zoomScale.z;
            float zPos = localPos.y * scaleConverter;
            toZoom = new Vector3(localPos.x, localPos.y, zPos);
        }

    }


    private void OnApplicationQuit()
    {
        saveDataTrigger.BuildingDataSave();
    }



}

