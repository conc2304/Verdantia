using System.Collections;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;


public class BuildingsMenuNew : MonoBehaviour
{
    [System.Serializable]
    public class BuildingsAsset
    {
        public string name;
        public GameObject type;
        public GameObject[] buildings;
        public int minPos, maxPos;
    }
    private List<GameObject> buildingsCategoryTypes = new List<GameObject>();
    private List<GameObject> buildingsCategories = new List<GameObject>();

    public int menuBuildingsTilt = -30;
    public Camera menuCamera;
    public Grid grid;
    public Canvas canvas;
    public TextMeshProUGUI textPRO;
    public Button typeButton;
    public Button buildingButton;
    public GameObject activateMenu;
    public RectTransform buildMenuTrackpad; // The rectangular UI element acting as the trackpad

    public GameObject mainMenu;
    public GameObject navigationGui;
    public GameObject homeButton;
    public Toggle heatmapToggle;
    public Color heatmapOffColor = Color.white;
    public Color heatmapOnColor = new(1f, 0.498f, 0.055f, 1f);
    public TMP_Dropdown heatmapDropdown;
    private static string drowpDownLabel = "Heat Map Type";
    private List<string> heatmapOptionsList = new List<string> { drowpDownLabel };

    public GameObject buildingStats;


    public Transform types;
    public BuildingsAsset[] buildings;
    public GameObject deleteBuilding;

    int minTypePos, maxTypePos;
    private int minPos, maxPos;
    int previousX, nextX;

    bool changePos = false;

    private Vector3 toPos;
    private float posX;

    private Vector3 dragStartPos;
    private Vector3 dragTargetPos;
    public float dragMultiplier = 1.25f;

    private bool isDragging = false;
    private float timeOfLastDrag = 0;

    private CameraController cameraController;
    private RoadGenerator roadGenerator;

    // public GameObject holdToSelect;

    public Dictionary<string, (int min, int max)> propertyRanges = new();


    public GameObject placementUI;
    public TrackPad trackPad;

    public GameObject navInfoToggleParent;
    public Button NavToggleBtn;
    public Button InfoToggleBtn;
    public GameObject cityMetricsBtnGO;

    public GameObject cityMetricsDisplay;



    private void Start()
    {
        cameraController = FindObjectOfType<CameraController>();
        roadGenerator = FindObjectOfType<RoadGenerator>();

        CreateTypes();

        minPos = minTypePos;
        maxPos = maxTypePos;

        activateMenu.SetActive(false);
        grid.enabled = false;

        UpdatePropertyRanges();
        InitializeTouchGui();
        InitializeHeatmapDropdownList();
        heatmapDropdown.onValueChanged.AddListener(OnDropdownValueChanged);
    }

    private void InitializeHeatmapDropdownList()
    {
        heatmapDropdown.ClearOptions();
        heatmapOptionsList.AddRange(propertyRanges.Keys.Select(key => StringsUtils.ConvertToLabel(key)).ToList());
        heatmapDropdown.AddOptions(heatmapOptionsList);
    }

    private void InitializeTouchGui()
    {
        // initial state
        navigationGui.SetActive(true);
        mainMenu.SetActive(true);
        homeButton.SetActive(false);
        buildingStats.SetActive(false);
        activateMenu.SetActive(false);
        placementUI.SetActive(false);
        navInfoToggleParent.SetActive(false);
        cityMetricsBtnGO.SetActive(true);
        cityMetricsDisplay.SetActive(false);
    }

    public void OnDropdownValueChanged(int index)
    {
        if (index == 0) return;

        string hmLabel = heatmapOptionsList[index];
        string hmValue = StringsUtils.ConvertToCamelCase(hmLabel);

        cameraController.heatmapMetric = hmValue;
        cameraController.cityChanged = true;
        cameraController.UpdateHeatMapCamera();
    }


    private void Update()
    {
        if (activateMenu.activeSelf)
        {
            MouseInput();

            // Clamp scroll to bounds
            if (types.localPosition.x > maxPos)
            {
                types.localPosition = new Vector3(Mathf.Lerp(types.localPosition.x, maxPos, Time.deltaTime), 0, 0);
                if (posX < 0)
                {
                    posX = posX / 2;
                }
            }
            if (types.localPosition.x < minPos)
            {
                types.localPosition = new Vector3(Mathf.Lerp(types.localPosition.x, minPos, Time.deltaTime), 0, 0);
                if (posX > 0)
                {
                    posX = posX / 2;
                }
            }

            types.localPosition = new Vector3(Mathf.Lerp(types.localPosition.x, types.localPosition.x - posX, Time.deltaTime * 5), 0, 0);

            // Slow to a Stop
            if (Math.Abs(posX) < 0.5) { posX = 0; }
            else if (!isDragging && Time.time - timeOfLastDrag < 2f)
            {
                posX -= Time.deltaTime * 10;
            }
        }


    }



    void MouseInput()
    {
        // Handle mouse or touch input
        if (Input.GetMouseButtonDown(0))
        {
            // Check if the drag started within the trackpad bounds
            if (RectTransformUtility.RectangleContainsScreenPoint(buildMenuTrackpad, Input.mousePosition, menuCamera))
            {
                isDragging = true;
                dragStartPos = Input.mousePosition;
            }
        }

        if (Input.GetMouseButton(0) && isDragging)
        {
            // Continue the drag if dragging was initiated within the bounds
            dragTargetPos = Input.mousePosition;
            Vector3 dragDelta = dragTargetPos - dragStartPos;

            toPos = dragDelta * dragMultiplier;
            posX = -toPos.x; // Invert direction

            dragStartPos = dragTargetPos; // Update the start position for smooth dragging
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
            timeOfLastDrag = Time.time;
        }
        else
        {
            // Add friction to drag
            if (posX > 0)
                posX -= Time.deltaTime * 100;
            if (posX < 0)
                posX += Time.deltaTime * 100;
        }
    }


    void CreateTypes()
    {
        int textPosY = -6;
        int posType = 0;

        for (int i = 0; i < buildings.Length; i++)
        {
            // Loop over Building Types/Categories

            // Wrapper
            GameObject tempObj = new GameObject("new");
            GameObject typeParent = Instantiate(tempObj, new Vector3(0, 0, 0), Quaternion.identity, types);
            typeParent.transform.localPosition = new Vector3(-posType, 0, 0);
            typeParent.transform.localScale = new Vector3(9, 9, 9);
            typeParent.name = buildings[i].type.name + "";

            // building type model
            GameObject type = Instantiate(
                buildings[i].type,
                new Vector3(0, 0, 0),
                Quaternion.identity,
                typeParent.transform
            );

            Destroy(tempObj.gameObject);

            type.transform.localPosition = new Vector3(0, 0, 0);
            type.transform.localRotation = Quaternion.Euler(menuBuildingsTilt, 0, 0);
            type.transform.localScale = new Vector3(1, 1, 1);

            type.name = buildings[i].type.name;

            foreach (Transform trans in type.GetComponentsInChildren<Transform>(true))
                trans.gameObject.layer = 5;

            buildingsCategories.Add(typeParent);

            TextMeshProUGUI text = Instantiate(textPRO, new Vector3(0, 0, 0), Quaternion.identity, typeParent.transform);
            text.transform.localPosition = new Vector3(0, textPosY, 0);
            text.text = buildings[i].name;

            Button button = Instantiate(typeButton, new Vector3(0, 0, 0), Quaternion.identity, typeParent.transform);
            button.transform.localPosition = new Vector3(0, 2, -8);
            button.onClick.AddListener(OnBuildingTypeClicked);
            button.gameObject.name = buildings[i].type.name;

            //find min and max position of type menu
            if (posType > maxTypePos)
                maxTypePos = posType;
            if (posType < minTypePos)
                minTypePos = posType;

            if (posType >= 0)
                posType += 165;
            posType *= -1;


            // Individual Buildings
            GameObject newObj = new GameObject("new");
            GameObject parent = Instantiate(newObj, new Vector3(0, 0, 0), Quaternion.identity, types);
            parent.transform.localPosition = new Vector3(0, 0, 0);
            parent.name = buildings[i].type.name + "_buildings";    // Collection of building types: ie all the houses
            Destroy(newObj.gameObject);
            buildingsCategoryTypes.Add(parent);

            int posBuild = 0;
            previousX = 0;
            nextX = 0;
            changePos = true;

            // Loop over Buildings in THIS category
            for (int u = 0; u < buildings[i].buildings.Length; u++)
            {
                // Create wrapper/parent to hold model, text and button
                GameObject tempCategory = new GameObject("new");
                GameObject categoryParent = Instantiate(tempObj, new Vector3(0, 0, 0), Quaternion.identity, parent.transform);
                categoryParent.transform.localPosition = new Vector3(-posBuild, 0, 0);
                categoryParent.transform.localScale = new Vector3(9, 9, 9);
                categoryParent.name = buildings[i].buildings[u].name;
                Destroy(tempCategory.gameObject);

                // Create building and add to building parent
                GameObject build = Instantiate(buildings[i].buildings[u], new Vector3(0, 0, 0), Quaternion.identity, categoryParent.transform);
                build.transform.localPosition = new Vector3(0, 0, 0); // add rotation to the building to see it better
                build.transform.localRotation = Quaternion.Euler(menuBuildingsTilt, 0, 0);
                build.transform.localScale = Vector3.one;
                build.name = buildings[i].buildings[u].name;

                foreach (Transform trans in build.GetComponentsInChildren<Transform>(true))
                    trans.gameObject.layer = 5;     // set all to UI layer for camera culling

                parent.SetActive(false);

                // Add text to building - add to build parent
                text = Instantiate(textPRO, new Vector3(0, 0, 0), Quaternion.identity, categoryParent.transform);
                text.transform.localPosition = new Vector3(0, textPosY, 0);
                text.text = buildings[i].buildings[u].GetComponent<BuildingProperties>().buildingName;

                // Add button to building - add to build parent
                Button buttonBuild = Instantiate(buildingButton, new Vector3(0, 0, 0), Quaternion.identity, categoryParent.transform);
                buttonBuild.transform.localPosition = new Vector3(0, 2, 0);
                buttonBuild.onClick.AddListener(OnBuildingClicked);
                buttonBuild.gameObject.name = buildings[i].buildings[u].name + "_btn";
                buttonBuild.transform.localPosition = new Vector3(0, 2, -8);


                //find min and max position of each type
                if (posBuild > buildings[i].maxPos)
                    buildings[i].maxPos = posBuild;
                if (posBuild < buildings[i].minPos)
                    buildings[i].minPos = posBuild;

                int itemPadding = -105;
                try
                {
                    for (int y = 1; y < buildings[i].buildings[u].GetComponent<BuildingProperties>().spaceWidth; y++)
                    {
                        buttonBuild.transform.localPosition = new Vector3(buttonBuild.transform.localPosition.x + 5, buttonBuild.transform.localPosition.y, buttonBuild.transform.localPosition.z);
                        // buttonBuild.transform.localScale = new Vector3(buttonBuild.transform.localScale.x + 1, 1, 1);
                        RectTransform rectTransform = buttonBuild.GetComponent<RectTransform>();
                        rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x + 10, rectTransform.sizeDelta.y);

                        text.transform.localPosition = new Vector3(text.transform.localPosition.x + 5, textPosY, 0); // center the building text
                    }
                    for (int y = 0; y < buildings[i].buildings[u].GetComponent<BuildingProperties>().spaceWidth; y++)
                    {
                        itemPadding += -90;
                    }

                }
                catch { }

                if (changePos)
                {
                    nextX += itemPadding;
                    posBuild = nextX;
                }
                else
                {
                    previousX -= itemPadding;
                    posBuild = previousX;
                    changePos = true;
                }


                try
                {
                    build.GetComponent<BuildingProperties>().buildConstruction.enabled = false;
                }
                catch { }
            }
        }

        //delete buildings button
        // Create wrapper/parent to hold model, text and button
        GameObject tempDel = new GameObject("new");
        GameObject typeDelParent = Instantiate(tempDel, new Vector3(0, 0, 0), Quaternion.identity, types);


        typeDelParent.transform.localPosition = new Vector3(-posType, 0, 0);
        typeDelParent.transform.localScale = new Vector3(9, 9, 9);
        typeDelParent.name = deleteBuilding.name;

        GameObject typeDel = Instantiate(deleteBuilding, new Vector3(0, 0, 0), Quaternion.identity, typeDelParent.transform);

        Destroy(tempDel.gameObject);

        typeDel.transform.localPosition = new Vector3(0, 0, 0);
        typeDel.transform.localRotation = Quaternion.Euler(menuBuildingsTilt, 0, 0);
        typeDel.transform.localScale = Vector3.one;
        typeDel.name = deleteBuilding.name;


        foreach (Transform trans in typeDel.GetComponentsInChildren<Transform>(true))
            trans.gameObject.layer = 5;

        buildingsCategories.Add(typeDelParent);

        TextMeshProUGUI textDel = Instantiate(textPRO, new Vector3(0, 0, 0), Quaternion.identity, typeDelParent.transform);
        textDel.transform.localPosition = new Vector3(0, textPosY, 0);
        textDel.text = deleteBuilding.name;

        Button buttonDel = Instantiate(typeButton, new Vector3(0, 0, 0), Quaternion.identity, typeDelParent.transform);
        buttonDel.transform.localPosition = new Vector3(0, 2, -8);
        buttonDel.onClick.AddListener(DeleteBuilding);
        buttonDel.gameObject.name = deleteBuilding.name;
    }

    public void CenterBuildingType(GameObject clickedBuildingBtn)
    {
        // The parent of the button has the placement position via CreateTypes, 
        // so scroll inversly to that position
        Transform clickedTransform = clickedBuildingBtn.transform.parent.GetComponent<Transform>();
        float clickedPosX = clickedTransform.localPosition.x;
        float distanceToTarget = (types.localPosition.x + clickedPosX) / 2.5f;
        posX = distanceToTarget;
    }


    public void OnHomeButton()
    {
        InitializeTouchGui();
    }

    public void OpenBuildMenu()
    {
        ActivateMenu();

        activateMenu.SetActive(true);

        // Aux Menus 
        mainMenu.SetActive(false);
        homeButton.SetActive(true);
        buildingStats.SetActive(false);
        navInfoToggleParent.SetActive(false);
        cityMetricsBtnGO.SetActive(false);

    }

    public void CloseBuildMenu()
    {
        ActivateMenu();
        activateMenu.SetActive(false);

        // Aux Menus 
        mainMenu.SetActive(true);
        homeButton.SetActive(false);
        buildingStats.SetActive(false);
        placementUI.SetActive(false);
        navInfoToggleParent.SetActive(false);
        cityMetricsBtnGO.SetActive(true);

    }

    public void OnHeatMapToggle()
    {
        cameraController.ToggleHeatMapView();

        ColorBlock colorBlock = heatmapToggle.colors;
        colorBlock.normalColor = cameraController.heatmapActive ? heatmapOnColor : heatmapOffColor;
        colorBlock.selectedColor = cameraController.heatmapActive ? heatmapOnColor : heatmapOffColor;
        heatmapToggle.colors = colorBlock;
    }


    public void ActivateMenu()
    {
        types.localPosition = new Vector3(0, 0, types.localPosition.z);

        if (cameraController.target != null)
            Destroy(cameraController.target.gameObject);
        else
            grid.enabled = !grid.isActiveAndEnabled;

        cameraController.target = null;
        cameraController.moveTarget = false;

        for (int i = 0; i < buildingsCategoryTypes.Count; i++)
            buildingsCategoryTypes[i].SetActive(false);
        for (int i = 0; i < buildingsCategories.Count; i++)
            buildingsCategories[i].SetActive(true);

        activateMenu.SetActive(!activateMenu.activeSelf);

        minPos = minTypePos;
        maxPos = maxTypePos;
    }


    public void OnBuildingTypeClicked()
    {
        // Handle Building Type Selection

        HoldToSelect holdToSelect = EventSystem.current.currentSelectedGameObject.GetComponent<HoldToSelect>();
        if (!holdToSelect.hasSelected) return;
        holdToSelect.ResetState();

        for (int i = 0; i < buildingsCategoryTypes.Count; i++)
        {
            //buildingsTypes[i].SetActive(false);

            // On Type/Category click activate the building category group (ie residential buildings)
            if (EventSystem.current.currentSelectedGameObject.name + "_buildings" == buildingsCategoryTypes[i].name)
            {
                buildingsCategoryTypes[i].SetActive(true);
                minPos = buildings[i].minPos;
                maxPos = buildings[i].maxPos;
            }
        }
        for (int i = 0; i < buildingsCategories.Count; i++)
        {
            buildingsCategories[i].SetActive(false);
        }

    }


    public void OnBuildingClicked()
    {
        HoldToSelect holdToSelect = EventSystem.current.currentSelectedGameObject.GetComponent<HoldToSelect>();
        if (holdToSelect.hasSelected)
        {
            CreateBuilding();
            OpenPlacementGUI(TrackpadTargetType.Build);
            navigationGui.SetActive(true);
            buildingStats.SetActive(false);
            navInfoToggleParent.SetActive(false);
        }
        else
        {
            SelectBuilding();
        }


    }

    public void OpenPlacementGUI(TrackpadTargetType targetType)
    {
        placementUI.SetActive(true);
        trackPad.SetTarget(targetType);

        TextMeshProUGUI textLabel = placementUI.transform.Find("ConfirmBtn").Find("Label").GetComponent<TextMeshProUGUI>();
        textLabel.text = targetType.ToString();

        homeButton.SetActive(true);
        activateMenu.SetActive(false);
        mainMenu.SetActive(false);
        buildingStats.SetActive(false);
        navInfoToggleParent.SetActive(false);
        cityMetricsBtnGO.SetActive(false);
        navigationGui.SetActive(true);
    }

    private void CreateBuilding()
    {
        UnsetTarget();

        for (int i = 0; i < buildings.Length; i++)
        {
            for (int u = 0; u < buildings[i].buildings.Length; u++)
            {
                if (buildings[i].buildings[u].name + "_btn" == EventSystem.current.currentSelectedGameObject.name)
                {
                    cameraController.moveTarget = true;
                    Transform target = Instantiate(buildings[i].buildings[u], new Vector3(0, 0, 0), Quaternion.identity).transform;
                    cameraController.target = target;
                }
            }
        }

        for (int i = 0; i < buildingsCategoryTypes.Count; i++)
        {
            buildingsCategories[i].SetActive(true);
            buildingsCategoryTypes[i].SetActive(false);
        }

        minPos = minTypePos;
        maxPos = maxTypePos;
    }

    private void SelectBuilding()
    {
        GameObject clickedBuildingBtn = EventSystem.current.currentSelectedGameObject;
        GameObject selectedBuilding = GetSelectedBuildingGO(clickedBuildingBtn);
        CenterBuildingType(clickedBuildingBtn);
        ApplySelectionScale(clickedBuildingBtn);

        BuildingProperties buildingProps = selectedBuilding.GetComponent<BuildingProperties>();
        if (buildingProps != null)
        {
            BuildingInfoDisplay displayData = GetComponent<BuildingInfoDisplay>();
            displayData.DisplayBuildingData(buildingProps);
            buildingStats.SetActive(true);
            navigationGui.SetActive(false);
            navInfoToggleParent.SetActive(true);
            NavToggleBtn.interactable = true;
            InfoToggleBtn.interactable = false;
        }
    }

    private void ResetSelectionScale(int scaleSize = 9)
    {
        for (int i = 0; i < buildingsCategoryTypes.Count; i++)
        {
            if (buildingsCategoryTypes[i].activeSelf)
            {
                foreach (Transform building in buildingsCategoryTypes[i].transform)
                {
                    building.localScale = new Vector3(scaleSize, scaleSize, scaleSize);
                }
            };
        }
    }

    private void ApplySelectionScale(GameObject selectedBuildingBtn)
    {
        ResetSelectionScale(9);
        float s = 11;
        selectedBuildingBtn.transform.parent.transform.localScale = new Vector3(s, s, s);
    }

    private GameObject GetSelectedBuildingGO(GameObject selectedBuildingBtn)
    {
        for (int i = 0; i < buildings.Length; i++)
        {
            for (int u = 0; u < buildings[i].buildings.Length; u++)
            {
                if (buildings[i].buildings[u].name + "_btn" == selectedBuildingBtn.name)
                {
                    GameObject selectedBuilding = buildings[i].buildings[u];
                    return selectedBuilding;
                }
            }
        }

        return null;
    }

    public void DeleteBuilding()
    {
        print("DeleteBuilding");

        // There is only a 
        HoldToSelect holdToSelect;
        EventSystem.current.currentSelectedGameObject.TryGetComponent<HoldToSelect>(out holdToSelect);
        if (holdToSelect != null)
        {
            if (!holdToSelect.hasSelected) return;
            holdToSelect.ResetHold();
        }


        // Update UI
        OpenPlacementGUI(TrackpadTargetType.Demolish);

        UnsetTarget();
        cameraController.moveTarget = true;
        Transform target = Instantiate(deleteBuilding, new Vector3(0, 0, 0), Quaternion.identity).transform;
        target.transform.GetChild(0).localPosition = new Vector3(0, 6, 0);
        cameraController.target = target;
    }

    public Dictionary<string, (int min, int max)> UpdatePropertyRanges()
    {
        // Loop over Buildings Categorys
        foreach (var buildingCategory in buildings)
        {
            // Loop over each building in the category
            foreach (var building in buildingCategory.buildings)
            {
                building.TryGetComponent<BuildingProperties>(out var properties);

                if (properties != null)
                {
                    // Loop over every property
                    FieldInfo[] fields = typeof(BuildingProperties).GetFields(BindingFlags.Public | BindingFlags.Instance);
                    foreach (FieldInfo field in fields)
                    {
                        // Get the name of the field (e.g., "constructionCost")
                        string fieldName = field.Name;


                        if (!properties.dataProps.Contains(fieldName)) continue;

                        // Get the value of the field for this specific building
                        Type fieldType = field.FieldType;
                        if (fieldType == typeof(int) || fieldType == typeof(float) || fieldType == typeof(double) || fieldType == typeof(long))
                        {

                            int value = (int)field.GetValue(properties);

                            // Check if we've already tracked this property
                            if (!propertyRanges.ContainsKey(fieldName))
                            {
                                propertyRanges[fieldName] = (int.MaxValue, int.MinValue); // Initialize min/max values
                            }

                            // Update the min and max for the property
                            propertyRanges[fieldName] = (
                                Mathf.Min(propertyRanges[fieldName].min, value),
                                Mathf.Max(propertyRanges[fieldName].max, value)
                            );
                        }
                    }
                }
            }
        }

        return propertyRanges;
    }

    public Dictionary<string, (int min, int max)> GetPropertyRanges()
    {
        if (propertyRanges.Count > 0) return propertyRanges;
        else return UpdatePropertyRanges();
    }



    public void OnPlacementCancel()
    {
        if (trackPad.isTracking) return; // prevent accidental click

        HoldToSelect holdToSelect = EventSystem.current.currentSelectedGameObject.GetComponent<HoldToSelect>();
        if (!holdToSelect.hasSelected) return;

        UnsetTarget();
        InitializeTouchGui();
    }

    public void OnPlacementConfirm()
    {
        if (trackPad.isTracking) return; // prevent accidental click
        HoldToSelect holdToSelect = EventSystem.current.currentSelectedGameObject.GetComponent<HoldToSelect>();
        if (!holdToSelect.hasSelected) return;


        Transform target = cameraController.target;
        if (target != null)
        {
            if (target.CompareTag("Road"))              // spawn if road
            {
                cameraController.SpawnRoad(target);
            }
            else if (target.CompareTag("Building"))      //spawn if building
            {
                cameraController.SpawnBuilding(target);
            }
            else if (target.CompareTag("DeleteTool"))
            {
                cameraController.DeleteTarget(target);
            }
        }

        // Delete and Roads let you keep going, but Buildings have to be reselected
        if (cameraController.target == null || !cameraController.moveTarget)
        {
            InitializeTouchGui();
        }
    }

    private void UnsetTarget()
    {
        if (cameraController.target != null && cameraController.target.gameObject != null) Destroy(cameraController.target.gameObject);
    }

    public void OnInfoTabClick()
    {
        buildingStats.SetActive(true);
        navigationGui.SetActive(false);

        NavToggleBtn.interactable = true;
        InfoToggleBtn.interactable = false;
    }

    public void OnNavTabClick()
    {
        buildingStats.SetActive(false);
        navigationGui.SetActive(true);

        NavToggleBtn.interactable = false;
        InfoToggleBtn.interactable = true;
    }

    public void OnCityStatsClick()
    {
        // just overlays the whole thing on top of everything like a modal
        cityMetricsDisplay.SetActive(true);
    }
}
