using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

/**
script manages the camera system and core interactions in the city builder game, 
enabling movement, zoom, rotation, and interaction with objects like buildings and roads.

Key features include:

Camera Control:
Smooth Movement and Rotation:

Allows players to pan, zoom, and rotate the camera smoothly for optimal city-building visibility.
Clamps camera movement to a predefined area, preventing navigation outside the playable zone.
Dynamic Zoom:
Adjusts the zoom level for detailed building placement or city-wide views.
Includes a slider UI for intuitive zoom control.

Interaction with Objects:
Building and Road Placement:
Supports placing buildings and roads with placement restrictions (e.g., proximity to roads or other chainable buildings).
Ensures no overlaps with existing objects like roads or buildings.
Handles snapping objects to the grid for precise alignment.
Demolition:
Allows players to demolish roads or buildings and updates metrics accordingly (e.g., deducting demolition costs, removing proximity effects).

Heatmap Integration:
Heatmap View:
Toggles between standard and heatmap camera views for analyzing city metrics like temperature or pollution.
Automatically adjusts the camera's angle and zoom for heatmap visualization.

City State Management:
Game Reset:
Resets the city by clearing all objects (e.g., buildings, roads, citizens) and resetting metrics for a clean slate.
Restores the background forest to its initial state.
Metrics and Proximity Effects:
Updates city-wide metrics based on building placements or demolitions.
Manages proximity effects, such as boosts or penalties applied to neighboring structures.

Trackpad Integration:
Placement Cursor:
Displays a cursor for precise placement or demolition of objects, linked to the trackpad input.
**/
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
    public Vector3 toZoom;
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
    private HeatMapOverlay heatMap;
    public bool cityChanged = false;

    public GameObject groundPlane;

    public bool heatmapActive = false;

    public string heatmapMetric = "cityTemperature";

    public FixedJoystick fixedJoystick;
    public TrackPad placementTrackpad;
    public CityMetricsManager cityMetricsManager;
    private CityTemperatureController cityTemperatureController;



    void Start()
    {
        zoomSlider.onValueChanged.AddListener(OnZoomSliderChanged);
    }


    void Awake()
    {
        buildingMenu = FindObjectOfType<BuildingsMenuNew>();
        roadGenerator = FindObjectOfType<RoadGenerator>();
        spawner = FindObjectOfType<Spawner>();
        saveDataTrigger = FindObjectOfType<SaveDataTrigger>();
        cityTemperatureController = FindObjectOfType<CityTemperatureController>();

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

        if (!heatMap) heatMap = FindObjectOfType<HeatMapOverlay>();

        heatmapMetric = "cityTemperature";
    }


    void Update()
    {
        TouchInput();
        SetPosition();

        if (cityChanged)
        {
            if (heatmapMetric != "cityTemperature")
            {
                UpdateHeatMap(heatmapMetric);
            }
            else
            {
                // DO NOTHING :: handled by City Temp Manager
            }


            cityMetricsManager.UpdateCityMetrics();
            if (saveDataTrigger.cityLoadInitialized)
            {
                cityTemperatureController.StepSimulation();
            }
        }

        cityChanged = false; // reset for next run
    }

    public void UpdateHeatMap(string metricName)
    {
        if (buildingMenu.propertyRanges.Count < 1) buildingMenu.UpdatePropertyRanges();

        heatmapMetric = metricName;

        float metricMin = buildingMenu.propertyRanges[heatmapMetric.ToString()].min;
        float metricMax = buildingMenu.propertyRanges[heatmapMetric.ToString()].max;
        heatMap.UpdateHeatMap(GetAllBuildings(true), metricName, metricMin, metricMax);
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
                target.TryGetComponent(out BuildingProperties buildingProps);
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

        // Calculate the aspect ratios
        float trackpadAspectRatio = trackpadWidth / trackpadHeight;
        float screenAspectRatio = 1920f / (1080f - 100f); // Assuming 1920x1080 resolution minus the top bar height

        // Offset X position based on size difference
        // Adjust normalizedX to account for the aspect ratio difference
        if (trackpadAspectRatio < screenAspectRatio)
        {
            // Trackpad is taller relative to the screen; map X proportionally
            float xPadding = (1 - (trackpadAspectRatio / screenAspectRatio)) / 2; // Add padding to both sides
            normalizedX = Mathf.Clamp01((normalizedX * (1 - 2 * xPadding)) + xPadding);
        }

        // Map the normalized trackpad position to the main display's screen resolution
        float topBarHeight = 100; // Adjust for the top bar
        float screenX = normalizedX * 1920f;
        float screenY = normalizedY * (1080f - topBarHeight);

        Vector3 screenPos = new Vector3(screenX, screenY, 0);
        return screenPos;
    }



    public Dictionary<string, object> SpawnRoad(Transform targetNew, bool isInitializing = false)
    {
        // TargetNew will be the RoadForSpawnPrefab, and it will be replaced by a new road for the correct type
        targetNew.parent = roadsParent;
        targetNew.position = new Vector3((Mathf.Round(targetNew.position.x / 10)) * 10, 0, (Mathf.Round(targetNew.position.z / 10)) * 10);

        // road was already hadd here if successfull
        // TargetNew is the RoadForSpawn prefab
        bool roadSpawnWasSuccessful = roadGenerator.CheckRoadType(targetNew);
        if (roadSpawnWasSuccessful)
        {
            // get the last road added to allRoads

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

            if (!isInitializing)
            {
                float constructionCost = targetNew.gameObject.GetComponent<BuildingProperties>().constructionCost;
                cityMetricsManager.DeductExpenses(constructionCost);
            };

            // Apply Proximity Boosts to neighbors and to self from neighbors
            // Check for proximity boosts from new ROAD on surrounding buildings
            Transform newestRoad = roadGenerator.newRoadSaved;
            BuildingProperties roadProps = newestRoad.GetComponent<BuildingProperties>();
            float lastPopupDelay = roadProps.ApplyProximityEffects();
            // Check for proximity boosts from existing buildings onto this new ROAD
            roadProps.ApplyNeighborEffectsToSelf(lastPopupDelay);

            cityChanged = true;
            return new Dictionary<string, object> { { "status", true }, { "msg", "Road added! You can add more." } };

        }
        return new Dictionary<string, object> { { "status", false }, { "msg", "Unable to place road here." } };
    }


    public Dictionary<string, object> SpawnBuilding(Transform targetNew, bool isInitializing = false)
    {
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
            // Check for new building's additionalSpace overlapping with existing roads
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

            spawner.carsCount += 1;
            spawner.citizensCount += 2;
            allBuildings.Add(targetNew);

            if (!isInitializing)
            {
                float constructionCost = targetNew.gameObject.GetComponent<BuildingProperties>().constructionCost;
                cityMetricsManager.DeductExpenses(constructionCost);
            }

            for (int i = 0; i < targetBuildProp.additionalSpace.Length; i++)
                allBuildings.Add(targetBuildProp.additionalSpace[i].transform);

            targetNew.position = new Vector3(
                Mathf.Round(targetNew.position.x / 10) * 10,
                0,
                Mathf.Round(targetNew.position.z / 10) * 10
            );


            //building animation
            BuildConstruction buildConstProp = targetBuildProp.buildConstruction.GetComponent<BuildConstruction>();
            buildConstProp.buildingProperties = targetBuildProp;
            buildConstProp.roadGenerator = roadGenerator;
            buildConstProp.target = targetBuildProp;
            buildConstProp.StartBuild();
            buildConstProp.cameraController = this;

            if (isInitializing)
            {
                targetBuildProp.isBuilt = true;
                buildConstProp.buildTime = 0.00001f;
                buildConstProp.isInitialLoad = true;
                // NOTE if we wait for building construction to apply effects of buildings loaded from memory,
                // then we risk completing the missions before they even start

                // Apply Proximity Effects
                // Check for proximity boosts from new building on surrounding buildings
                float lastPopupDelay = 0;
                // first display from new building on surrounding buildings
                lastPopupDelay += targetBuildProp.ApplyProximityEffects();
                // then show surrounding buildings on new building
                targetBuildProp.ApplyNeighborEffectsToSelf(lastPopupDelay);
            }

            StartCoroutine(UpdateMetricsOnBuilt(buildConstProp.buildTime));

            //menu
            buildingMenu.grid.enabled = false;
            moveTarget = false;
            target = null;
            cityChanged = true;

            return new Dictionary<string, object> { { "status", true }, { "msg", "New building added!" } };
        }

        return new Dictionary<string, object> { { "status", false }, { "msg", "Unable to add building." } };
    }

    private IEnumerator UpdateMetricsOnBuilt(float buildTime)
    {
        yield return new WaitForSeconds(buildTime);
        cityMetricsManager.UpdateCityMetrics();
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
            // if (!buildingNames.Contains(other.name)) continue;

            if (IsAdjacent(targetNew.position, other.position))
            {
                return true;
            }

            // Check if building is next to any of the existing building's additional slaces
            other.TryGetComponent(out BuildingProperties buildingProps);
            if (buildingProps == null || buildingProps.additionalSpace == null) continue;
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
        bool removeEffect = true;

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
                float demolitionCost = demolisionTargetProps.demolitionCost;
                demolishedBuildingName = demolisionTargetProps.buildingName;
                cityMetricsManager.DeductExpenses(demolitionCost);

                // Check for and remove proximity boosts from demolished building on surrounding buildings
                demolisionTargetProps.ApplyProximityEffects(removeEffect);

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
        Mission currentMission = FindObjectOfType<MissionManager>().currentMission;

        if (saveDataTrigger.doSave && currentMission != null && currentMission.missionName != "Free Play Mode")
        {
            string missionFile = SaveSystem.FormatFileName(currentMission.missionCityFileName);
            saveDataTrigger.BuildingDataSave(missionFile);
        }
        else if (saveDataTrigger.doSave)
        {
            saveDataTrigger.BuildingDataSave();

        }
    }

    public List<Transform> GetAllBuildings(bool includeSpaces = true)
    {
        List<Transform> cityBuildings = new List<Transform>();

        foreach (Transform road in roadGenerator.allRoads)
        {
            if (road.name.ToLower().Contains("spawn")) continue;
            cityBuildings.Add(road);
        }

        // allBuildings contians buildings, spaces,
        foreach (Transform building in allBuildings)
        {

            if (!(building.CompareTag("Building") || (building.CompareTag("Space") && includeSpaces))) continue;
            if (building.name.ToLower().Contains("spawn")) continue;

            building.TryGetComponent(out BuildingProperties buildingProps);
            if (!buildingProps)
            {
                Debug.LogError("Building Props is Null for : " + building.name);
                continue;
            };

            // only count buildings if they are built
            if (buildingProps.isBuilt == true)
            {
                cityBuildings.Add(building);
            }

        }

        return cityBuildings;
    }

    public void ActivateBackgroundForest()
    {
        for (int i = 0; i < forestObj.Count; i++)
        {
            forestObj[i].gameObject.SetActive(true);
        }
    }


    public IEnumerator ResetGameField()
    {
        // Clear windmill blades
        WindmillsController windmillsController = FindObjectOfType<WindmillsController>();
        windmillsController.blades.Clear();

        // Clear tracking lists
        allBuildings.Clear();
        roadGenerator.allRoads.Clear();

        // Reset metrics and counters
        spawner.carsCount = 0;
        spawner.citizensCount = 0;
        spawner.citizensSpawnPoints.Clear();
        spawner.carsSpawnPoints.Clear();
        cityMetricsManager.ResetMetrics();

        // Destroy child objects in batches
        yield return DestroyChildrenInBatches(buildingsParent);
        yield return DestroyChildrenInBatches(roadsParent);
        yield return DestroyChildrenInBatches(citizensParent);
        yield return DestroyChildrenInBatches(carsParent);

        // Reset forest objects
        forestObj.ForEach(forest => forest.gameObject.SetActive(true));

        cityChanged = false;

        Debug.Log("Playing field has been reset.");
    }

    private IEnumerator DestroyChildrenInBatches(Transform parent)
    {
        int counter = 0;
        int destructionBatchSize = 20; // Number of objects to destroy per frame

        // Store children in a temporary list to avoid modifying the collection while iterating
        List<Transform> children = new List<Transform>();
        foreach (Transform child in parent)
        {
            children.Add(child);
        }

        // Iterate through the list and destroy the objects
        foreach (Transform child in children)
        {
            if (child && child.gameObject) Destroy(child.gameObject);
            counter++;

            // After destroying a batch, wait for the next frame
            if (counter >= destructionBatchSize)
            {
                counter = 0;
                yield return null;
            }
        }

        // Ensure all children are cleared
        yield return null;
    }

}

