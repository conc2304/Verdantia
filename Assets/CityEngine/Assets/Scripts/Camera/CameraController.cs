using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
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
    public GameObject placementCursor;

    [HideInInspector]
    public Transform target;
    public int gridSize = 10;

    public float normalSpeed;
    public float fastSpeed;
    public float movSpeed;
    private Vector3 toPos;

    public float rotationScale;
    private Quaternion toRot;
    private Quaternion mainCamtoRot;

    public Vector3 zoomScale;
    private Vector3 toZoom;
    public float minZoom;
    public float maxZoom;
    private float defaultMaxZoom;
    public Slider zoomSlider;

    private int[] xRange = new int[2] { 0, 1000 };
    private int[] zRange = new int[2] { 0, 1000 };

    private BuildingsMenuNew buildingMenu;
    private RoadGenerator roadGenerator;
    private SaveDataTrigger saveDataTrigger;
    Spawner spawner;

    public List<Transform> allBuildings = new List<Transform>();

    private bool dontBuild;

    public Transform forest;
    readonly List<Transform> forestObj = new List<Transform>();

    public GameObject activateMenu;

    public Grid grid;
    private HeatMap heatMap;
    public bool cityChanged = false;

    public GameObject groundPlane;

    public bool heatmapActive = false;

    public string heatmapMetric = "cityTemperature";

    public FixedJoystick fixedJoystick;
    public TrackPad placementTrackpad;
    public CityMetricsManager cityMetricsManager;

    public float cityTempUpdateInterval = 6;
    public int temperatureMapTimeSteps = 5;
    private float cityTempUpdateRate;
    private float cityTempTimer = 0f;
    public bool toggleRestartTemp = false;
    public bool playTemp = false;



    void Start()
    {
        zoomSlider.onValueChanged.AddListener(OnZoomSliderChanged);
        cityTempUpdateRate = cityMetricsManager.monthDuration / cityTempUpdateInterval;
    }


    void Awake()
    {
        buildingMenu = FindObjectOfType<BuildingsMenuNew>();
        roadGenerator = FindObjectOfType<RoadGenerator>();
        spawner = FindObjectOfType<Spawner>();
        saveDataTrigger = FindObjectOfType<SaveDataTrigger>();

        toPos = cameraHolder.transform.position;
        toRot = cameraHolder.transform.rotation;
        mainCamtoRot = cameraTransform.localRotation;
        defaultMaxZoom = maxZoom;
        toZoom = cameraTransform.localPosition;
        zoomSlider.minValue = minZoom;
        zoomSlider.maxValue = maxZoom;
        zoomSlider.value = toZoom.y;


        for (int i = 0; i < forest.childCount; i++)
            forestObj.Add(forest.GetChild(i));

        if (!heatMap) heatMap = FindObjectOfType<HeatMap>();


    }


    void Update()
    {
        TouchInput();
        SetPosition();


        // if (toggleRestartTemp || (playTemp && cityTempTimer >= cityTempUpdateRate))      // TODO remove this after testing

        cityTempTimer += Time.deltaTime;
        // handle heat map updates on city change on fixed monthly intervals
        if (cityTempTimer >= cityTempUpdateRate)
        {
            for (int i = 0; i < temperatureMapTimeSteps; i++)
            {
                cityMetricsManager.GetCityTemperatures();
            }

            if (heatmapMetric == "cityTemperature")
            {
                // Make sure we have a valid temperature matrix
                float[,] cityTemps = (cityMetricsManager.temps != null && cityMetricsManager.temps.Length != 0) ?
                    cityMetricsManager.temps :
                    cityMetricsManager.GetCityTemperatures();

                int minTemp = 50;
                int maxTemp = 85;
                // cityTemps = cityMetricsManager.RemovePaddingFromMatrix(cityTemps);
                heatMap.RenderCityTemperatureHeatMap(cityTemps, minTemp, maxTemp);  // TODO investigate if this needs to change 

                return; // city temp is computed and rendered differently
            }
            cityTempTimer = 0f;
            // toggleRestartTemp = false; // Reset the toggle if you want it to trigger only once
        }



        if (cityChanged)
        {
            if (heatmapMetric != "cityTemperature")
            {
                UpdateHeatMap(heatmapMetric);
            }
        }

        if (cityChanged && saveDataTrigger.cityLoadInitialized)
        {
            cityMetricsManager.UpdateCityMetrics();
        } // TODO is this correct
        cityChanged = false; // reset for next run
    }

    public void UpdateHeatMap(string metricName)
    {
        if (buildingMenu.propertyRanges.Count < 1) buildingMenu.UpdatePropertyRanges();

        heatmapMetric = metricName;

        int metricMin = buildingMenu.propertyRanges[heatmapMetric].min;
        int metricMax = buildingMenu.propertyRanges[heatmapMetric].max;
        heatMap.UpdateHeatMap(GetAllBuildings(true), heatmapMetric, metricMin, metricMax);
    }

    private void LateUpdate()
    {

        if (moveTarget && target != null)
        {
            Vector3 trackpadPos = TrackpadToMainCamera();

            float planeY = 0;
            Plane plane = new Plane(Vector3.up, Vector3.up * planeY); // ground plane
            Ray ray = Camera.main.ScreenPointToRay(trackpadPos);

            float distance; // the distance from the ray origin to the ray intersection of the plane
            if (plane.Raycast(ray, out distance))
            {
                target.position = Vector3.Lerp(
                    target.position,
                    new Vector3(Mathf.Floor((ray.GetPoint(distance).x + gridSize / 2) / gridSize) * gridSize,
                    0,
                    Mathf.Floor((ray.GetPoint(distance).z + gridSize / 2) / gridSize) * gridSize),
                    Time.deltaTime * 50
                );

                // Place a cursor for easy location
                target.TryGetComponent<BuildingProperties>(out BuildingProperties buildingProps);
                if (!target.CompareTag("DeleteTool") && buildingProps != null)
                {
                    placementCursor.transform.position = buildingProps.GetBuildingPopUpPlacement();
                }

            }
        }
    }



    public Vector3 TrackpadToMainCamera()
    {

        Vector2 trackpadPos = placementTrackpad.GetTargetPosition();

        // Get the trackpad's size
        float trackpadWidth = placementTrackpad.trackpadRect.sizeDelta.x;
        float trackpadHeight = placementTrackpad.trackpadRect.sizeDelta.y;

        // Normalize the trackpad position to values between 0 and 1
        float normalizedX = (trackpadPos.x + trackpadWidth / 2) / trackpadWidth; // Local position ranges from -width/2 to +width/2
        float normalizedY = (trackpadPos.y + trackpadHeight / 2) / trackpadHeight; // Local position ranges from -height/2 to +height/2

        // Map the normalized trackpad position to the main display's screen resolution
        // Hardcoded for now
        float topBarHight = 100;
        float screenX = normalizedX * 1920;
        float screenY = normalizedY * (1080 - topBarHight);

        Vector3 screenPos = new Vector3(screenX, screenY, 0);
        return screenPos;
    }


    public Dictionary<string, object> SpawnRoad(Transform targetNew, bool isInitializing = false)
    {
        targetNew.parent = roadsParent;
        targetNew.position = new Vector3((Mathf.Round(targetNew.position.x / 10)) * 10, 0, (Mathf.Round(targetNew.position.z / 10)) * 10);

        bool roadSpawnWasSuccessful = roadGenerator.CheckRoadType(targetNew);
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
            // lastClickTime = 0;

            if (!isInitializing)
            {
                int constructionCost = targetNew.gameObject.GetComponent<BuildingProperties>().constructionCost;
                cityMetricsManager.DeductExpenses(constructionCost);
            };

            foreach (Transform existingBuildingTransform in allBuildings)
            {
                if (!existingBuildingTransform.CompareTag("Building")) continue;

                BuildingProperties existingBuilding = existingBuildingTransform.GetComponent<BuildingProperties>();
                if (existingBuilding != null && targetNew.GetComponent<BuildingProperties>().IsWithinProximity(existingBuilding))
                {
                    // Apply proximity effects from the existing building to the new building
                    foreach (MetricBoost boost in existingBuilding.proximityEffects)
                    {
                        existingBuilding.ApplyBoost(targetNew.GetComponent<BuildingProperties>(), boost);
                    }
                }
            }

            cityChanged = true;
            return new Dictionary<string, object> { { "status", true }, { "msg", "Road added! You can add more." } };

        }
        return new Dictionary<string, object> { { "status", false }, { "msg", "Unable to place road here." } };
    }

    public Dictionary<string, object> SpawnBuilding(Transform targetNew, bool isInitializing = false)
    {
        // Get Building Properties
        BuildingProperties targetBuildProp = targetNew.GetComponent<BuildingProperties>();

        dontBuild = false;
        targetNew.parent = buildingsParent;

        string msg = "";

        for (int i = 0; i < allBuildings.Count; i++)
        {
            // Check for new building overlapping existing building
            if (Mathf.Round(allBuildings[i].position.x / 10) * 10 == Mathf.Round(targetNew.position.x / 10) * 10 &&
                Mathf.Round(allBuildings[i].position.z / 10) * 10 == Mathf.Round(targetNew.position.z / 10) * 10)
            {
                msg = "Can't build over an existing building";
                dontBuild = true;
                return new Dictionary<string, object> { { "status", false }, { "msg", msg } };
            }

            // Check for additionalSpace overlap with existing building
            for (int u = 0; u < targetBuildProp.additionalSpace.Length; u++)
            {
                if (Mathf.Round(allBuildings[i].position.x / 10) * 10 == Mathf.Round(targetBuildProp.additionalSpace[u].position.x / 10) * 10 &&
                Mathf.Round(allBuildings[i].position.z / 10) * 10 == Mathf.Round(targetBuildProp.additionalSpace[u].position.z / 10) * 10)
                {
                    msg = "Can't build over an existing building";
                    dontBuild = true;
                    return new Dictionary<string, object> { { "status", false }, { "msg", msg } };
                }
            }
        }

        // if we are initializing the game, then we are loading the city from memory
        // and we dont care about placement restrictions
        // otherwise the user is placing builinds so then check placement restrictions
        if (!isInitializing)
        {
            // some buildings can be placed anywhere as long as they do not over lap
            // if they have do not unrestrictedPlacement then check the restrictions below
            if (targetBuildProp.unrestrictedPlacement == false)
            {
                // If target cannot chain, then it must be built by a road, 
                // otherwise it can be placed by a chainable building
                bool targetCanChain = CanBuildingChain(targetNew);
                if (!targetCanChain)
                {
                    // if the target is not chainable, then it must be placed by road
                    if (!IsBuildingNextToRoad(targetNew))
                    {
                        dontBuild = true;
                        msg = "Must be placed next to a road" + (targetCanChain ? " or by similar building" : "");
                        return new Dictionary<string, object> { { "status", false }, { "msg", msg } };
                    }
                }
            }
        }


        // Check for new building overlapping with existing roads
        for (int i = 0; i < roadGenerator.allRoads.Count; i++)
        {
            if (Mathf.Round(roadGenerator.allRoads[i].position.x / 10) * 10 == Mathf.Round(targetNew.position.x) &&
                Mathf.Round(roadGenerator.allRoads[i].position.z / 10) * 10 == Mathf.Round(targetNew.position.z))
            {
                dontBuild = true;
                msg = "Can't build over an existing road";

                return new Dictionary<string, object> { { "status", false }, { "msg", msg } };
            }
            // Check foir new building's additionalSpace overlapping with existing roads
            for (int u = 0; u < targetBuildProp.additionalSpace.Length; u++)
            {
                if (Mathf.Round(roadGenerator.allRoads[i].position.x / 10) * 10 == Mathf.Round(targetBuildProp.additionalSpace[u].position.x / 10) * 10 &&
                Mathf.Round(roadGenerator.allRoads[i].position.z / 10) * 10 == Mathf.Round(targetBuildProp.additionalSpace[u].position.z / 10) * 10)
                {
                    dontBuild = true;
                    msg = "Can't build over an existing road";
                    return new Dictionary<string, object> { { "status", false }, { "msg", msg } };
                }
            }
        }

        //  Add/Build the building if conditions met
        if (dontBuild == false)
        {
            // Clear Background Forest
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


            // Check for proximity boosts from existing buildings
            foreach (Transform existingBuildingTransform in allBuildings)
            {
                if (!existingBuildingTransform.CompareTag("Building")) continue;

                BuildingProperties existingBuilding = existingBuildingTransform.GetComponent<BuildingProperties>();
                if (existingBuilding != null && targetBuildProp.IsWithinProximity(existingBuilding))
                {
                    // Apply proximity effects from the existing building to the new building
                    foreach (MetricBoost boost in existingBuilding.proximityEffects)
                    {
                        existingBuilding.ApplyBoost(targetBuildProp, boost);
                    }
                }
            }

            // Check for proximity boosts from new building on surrounding buildings
            targetBuildProp.ApplyProximityEffects();

            spawner.carsCount += 1;
            spawner.citizensCount += 2;
            allBuildings.Add(targetNew);

            if (!isInitializing)
            {
                int constructionCost = targetNew.gameObject.GetComponent<BuildingProperties>().constructionCost;
                cityMetricsManager.DeductExpenses(constructionCost);
            }

            for (int i = 0; i < targetBuildProp.additionalSpace.Length; i++)
                allBuildings.Add(targetBuildProp.additionalSpace[i].transform);

            targetNew.position = new Vector3(
                Mathf.Round(targetNew.position.x / 10) * 10,
                0,
                Mathf.Round(targetNew.position.z / 10) * 10
            );


            // if (target.GetComponent<BuildingProperties>().connectToRoad)
            // //   roadGenerator.ConnectBuildingToRoad(target);

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

            return new Dictionary<string, object> { { "status", true }, { "msg", "New building added!" } };
        }

        return new Dictionary<string, object> { { "status", false }, { "msg", "Unable to add building." } };
    }

    // Check if building is next to any chainable buildings, chainable buildings can chain off of themselves
    public bool CanBuildingChain(Transform targetNew)
    {
        BuildingProperties targetBuildProp = targetNew.GetComponent<BuildingProperties>();

        if (targetBuildProp.allowChaining == false) return false;

        string[] gameObjectNames = targetBuildProp.chainableTypes.Select(transform => transform.gameObject.name).ToArray();
        gameObjectNames.Append(target.name);

        return IsNextToBuildingsOfType(targetNew, gameObjectNames);
    }

    public bool IsBuildingNextToRoad(Transform targetNew)
    {
        BuildingProperties targetBuildProp = targetNew.GetComponent<BuildingProperties>();

        // Check if the building itself is next to a road
        foreach (Transform road in roadGenerator.allRoads)
        {
            if (IsAdjacent(targetNew.position, road.position))
            {
                return true;
            }
        }

        // Check if any additional spaces of the building are next to a road
        foreach (Transform space in targetBuildProp.additionalSpace)
        {
            foreach (Transform road in roadGenerator.allRoads)
            {
                if (IsAdjacent(space.position, road.position))
                {
                    return true;
                }
            }
        }

        // No adjacent road found for the building or additional spaces
        return false;
    }

    public bool IsNextToBuildingsOfType(Transform targetNew, string[] buildingNames)
    {
        BuildingProperties targetBuildProp = targetNew.GetComponent<BuildingProperties>();

        // Check if the target is next to a building of a type
        foreach (Transform other in allBuildings)
        {
            if (!buildingNames.Contains(other.name)) continue;

            if (IsAdjacent(targetNew.position, other.position))
            {
                return true;
            }

            // Check if building is next to any of the existing building's additional slaces
            other.TryGetComponent(out BuildingProperties buildingProps);
            foreach (Transform additionalSpace in buildingProps.additionalSpace)
            {
                if (IsAdjacent(targetNew.position, additionalSpace.position))
                {
                    return true;
                }
            }
        }

        // Check if any THIS building's additional spaces of the new building are next to existing buildings of type
        foreach (Transform space in targetBuildProp.additionalSpace)
        {
            foreach (Transform other in allBuildings)
            {

                if (!buildingNames.Contains(other.name)) continue;

                if (IsAdjacent(space.position, other.position))
                {
                    return true;
                }

                // Check if any of THIS building's spaces are adjacent to spaces of existing building
                other.TryGetComponent(out BuildingProperties buildingProps);
                foreach (Transform additionalSpace in buildingProps.additionalSpace)
                {
                    if (IsAdjacent(targetNew.position, additionalSpace.position))
                    {
                        return true;
                    }
                }
            }
        }

        // No adjacent road found for the building or additional spaces
        return false;
    }

    // Helper function to determine if two positions are adjacent (10 units apart on either x or z axis)
    private bool IsAdjacent(Vector3 posA, Vector3 posB, int tolerance = 10)
    {
        return (Mathf.Abs(posA.x - posB.x) <= tolerance && posA.z == posB.z) || (Mathf.Abs(posA.z - posB.z) <= tolerance && posA.x == posB.x);
    }


    public Dictionary<string, object> DeleteTarget(Transform target)
    {

        string demolishedBuildingName = "";
        // Checck all buildings to see if it intersects demolition target and remove building 
        for (int i = 0; i < allBuildings.Count; i++)
        {
            if (Mathf.Round(allBuildings[i].position.x / 10) * 10 == Mathf.Round(target.position.x / 10) * 10 &&
                Mathf.Round(allBuildings[i].position.z / 10) * 10 == Mathf.Round(target.position.z / 10) * 10)
            {
                cityChanged = true;

                BuildingProperties demolisionTargetProps = allBuildings[i].GetComponent<BuildingProperties>();
                // deduct demolition cost from budget 
                int demolitionCost = demolisionTargetProps.demolitionCost;
                demolishedBuildingName = demolisionTargetProps.buildingName;
                cityMetricsManager.DeductExpenses(demolitionCost);

                // Check for and remove proximity boosts from demolished building on surrounding buildings
                demolisionTargetProps.RemoveProximityEffects();

                for (int pathTargetIndex = 0; pathTargetIndex < demolisionTargetProps.carsPathTargetsToConnect.Length; pathTargetIndex++)
                    spawner.carsSpawnPoints.Remove(demolisionTargetProps.carsPathTargetsToConnect[pathTargetIndex].GetComponent<PathTarget>().previousPathTarget.transform);

                for (int pathTargetIndex = 0; pathTargetIndex < demolisionTargetProps.citizensPathTargetsToSpawn.Length; pathTargetIndex++)
                    spawner.citizensSpawnPoints.Remove(demolisionTargetProps.citizensPathTargetsToSpawn[pathTargetIndex]);

                // if delete overlap is "space" destory all sibling spaces from game
                if (allBuildings[i].CompareTag("Space"))
                {
                    BuildingProperties spaceBuildingProperty = allBuildings[i].parent.parent.GetComponent<BuildingProperties>();
                    demolishedBuildingName = spaceBuildingProperty.buildingName;
                    for (int y = 0; y < spaceBuildingProperty.additionalSpace.Length; y++)
                    {
                        Destroy(spaceBuildingProperty.additionalSpace[y].gameObject);
                        allBuildings.Remove(spaceBuildingProperty.additionalSpace[y]);
                    }

                    Destroy(spaceBuildingProperty.gameObject);
                    allBuildings.Remove(spaceBuildingProperty.transform);
                    cityChanged = true;
                }
                else // is not "Space" so delete all child additional Spaces
                {
                    for (int y = 0; y < demolisionTargetProps.additionalSpace.Length; y++)
                    {
                        allBuildings.Remove(demolisionTargetProps.additionalSpace[y]);
                    }

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
                demolishedBuildingName = "Road";
                cityChanged = true;
                break;
            }
        }

        if (cityChanged)
        {
            return new Dictionary<string, object> { { "status", true }, { "msg", "Demolished " + demolishedBuildingName } };
        }
        else
        {
            return new Dictionary<string, object> { { "status", false }, { "msg", "Nothing to demolish here." } };
        }
    }

    void SetPosition()
    {
        // Limit how far a user can scroll 
        toPos.x = Mathf.Clamp(toPos.x, xRange[0], xRange[1]);
        toPos.z = Mathf.Clamp(toPos.z, zRange[0], zRange[1]);


        cameraHolder.transform.position = Vector3.Lerp(
            cameraHolder.transform.position,
            toPos,
            Time.deltaTime * 5
        );


        toZoom.y = Mathf.Clamp(toZoom.y, -minZoom, !heatmapActive ? maxZoom : maxZoom);
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
        float joystickSpeed = NumbersUtils.Remap(minZoom, maxZoom, 60, 120, toZoom.y); // move faster at futher zooms
        // Handle Joystick input for camera movement L/R/U/D
        Vector3 direction = cameraHolder.transform.forward * fixedJoystick.Vertical + cameraHolder.transform.right * fixedJoystick.Horizontal;
        float magnitude = direction.magnitude * joystickSpeed; // the further the joy stick is the faster they move
        Vector3 movement = magnitude * Time.deltaTime * direction;
        if (heatmapActive) movement.y = 0; // Prevent Y-axis movement
        toPos += movement;
    }

    public void RotateBuilding()
    {
        if (target != null)
        {
            float y = target.rotation.eulerAngles.y + 90;
            target.rotation = Quaternion.Euler(0, y, 0);
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
        if (!heatmapActive)
        {
            toZoom += multiplier * zoomScale;
        }
        else
        {
            toZoom.y += multiplier * zoomScale.y;
        }

        SetSliderValueWithoutCallback(toZoom.y);
    }

    public void ZoomOut(float multiplier = 1)
    {
        if (!heatmapActive)
        {
            toZoom -= multiplier * zoomScale;
        }
        else
        {
            toZoom.y -= multiplier * zoomScale.y;
        }

        SetSliderValueWithoutCallback(toZoom.y);
    }

    public void OnZoomSliderChanged(float zoomAmount)
    {
        if (!heatmapActive) toZoom.z = -zoomAmount;
        toZoom.y = zoomAmount;
    }

    public void SetSliderValueWithoutCallback(float newValue)
    {
        zoomSlider.onValueChanged.RemoveListener(OnZoomSliderChanged);
        zoomSlider.value = newValue;
        zoomSlider.onValueChanged.AddListener(OnZoomSliderChanged);
    }

    public void SetHeatMapView(bool isActive, string metric)
    {
        heatmapActive = isActive;
        UpdateHeatMapCamera();
    }

    public void ToggleHeatMapView()
    {
        heatmapActive = !heatmapActive;
        heatMap.heatMapPlane.SetActive(heatmapActive);
        FindObjectOfType<HeatMapLegend>().SetVisibility(heatmapActive);
        UpdateHeatMapCamera();
    }

    public void UpdateHeatMapCamera()
    {
        // Updates the camera angle to point down at 90 or out at 45
        // Update the Y and Z position of the camera to keep the current view in focus 

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
            // maxZoom = Math.Max(defaultMaxZoom, localPos.y + vertDiff);
        }
        else
        {
            // Going into default mode where pos Y and Z are inverses of eachother based on the zoomScale
            float scaleConverter = zoomScale.y / zoomScale.z;
            float zPos = localPos.y * scaleConverter;
            toZoom = new Vector3(localPos.x, localPos.y - vertDiff, zPos);
        }
    }


    private void OnApplicationQuit()
    {
        saveDataTrigger.BuildingDataSave();
    }

    public List<Transform> GetAllBuildings(bool includeSpaces = true)
    {
        List<Transform> cityBuildings = new List<Transform>();
        cityBuildings.AddRange(roadGenerator.allRoads);


        // allBuildings contians buildings, spaces,
        foreach (Transform building in allBuildings)
        {

            if (!(building.CompareTag("Building") || (building.CompareTag("Space") && includeSpaces))) continue;

            building.TryGetComponent(out BuildingProperties buildingProps);
            if (!buildingProps)
            {
                Debug.LogError("Building Props is Null for : " + building.name);
                continue;
            };

            cityBuildings.Add(building);
        }

        return cityBuildings;
    }
}

