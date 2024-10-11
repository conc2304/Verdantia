using System.Collections;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using Unity.VisualScripting;
using System.Globalization;


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
    private List<GameObject> buildingsParents = new List<GameObject>();
    private List<GameObject> buildingsTypes = new List<GameObject>();

    public int menuBuildingsTilt = -30;
    public Camera menuCamera;
    public Grid grid;
    public Canvas canvas;
    public TextMeshProUGUI textPRO;
    public Button typeButton;
    public Button buildingButton;
    public GameObject activateMenu;
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
    public int minPos, maxPos;
    int previousX, nextX;

    bool changePos = false;

    private Vector3 toPos;
    public float posX;

    private Vector3 dragStartPos;
    private Vector3 dragTargetPos;
    public float dragMultiplier = 1.25f;

    private bool isDragging = false;


    private CameraController cameraController;
    private RoadGenerator roadGenerator;

    bool doubleClick;

    public Dictionary<string, (int min, int max)> propertyRanges = new();


    public RectTransform trackpadRect; // The rectangular UI element acting as the trackpad


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
        heatmapOptionsList.AddRange(propertyRanges.Keys.Select(key => ConvertToLabel(key)).ToList());
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
    }

    public void OnDropdownValueChanged(int index)
    {
        if (index == 0) return;

        string hmLabel = heatmapOptionsList[index];
        string hmValue = ConvertToCamelCase(hmLabel);

        print(hmValue + index);

        cameraController.heatmapMetric = hmValue;
        cameraController.cityChanged = true;
        // cameraController.UpdateHeatMapCamera();
    }


    private void Update()
    {
        if (activateMenu.activeSelf)
        {
            MouseInput();

            if (types.localPosition.x > maxPos)
            {
                types.localPosition = new Vector3(Mathf.Lerp(types.localPosition.x, maxPos, Time.deltaTime), 0, 0);
                if (posX < 0)
                    posX = posX / 2;
            }
            if (types.localPosition.x < minPos)
            {
                types.localPosition = new Vector3(Mathf.Lerp(types.localPosition.x, minPos, Time.deltaTime), 0, 0);
                if (posX > 0)
                    posX = posX / 2;
            }

            types.localPosition = new Vector3(Mathf.Lerp(types.localPosition.x, types.localPosition.x + -posX * 5, Time.deltaTime), 0, 0);
        }
    }



    void MouseInput()
    {
        // Handle mouse or touch input
        if (Input.GetMouseButtonDown(0))
        {
            // Check if the drag started within the trackpad bounds
            if (RectTransformUtility.RectangleContainsScreenPoint(trackpadRect, Input.mousePosition, menuCamera))
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
            type.name = buildings[i].type.name;

            foreach (Transform trans in type.GetComponentsInChildren<Transform>(true))
                trans.gameObject.layer = 5;

            buildingsTypes.Add(typeParent);

            TextMeshProUGUI text = Instantiate(textPRO, new Vector3(0, 0, 0), Quaternion.identity, typeParent.transform);
            text.transform.localPosition = new Vector3(0, textPosY, 0);
            text.text = buildings[i].name;

            Button button = Instantiate(typeButton, new Vector3(0, 0, 0), Quaternion.identity, typeParent.transform);
            button.transform.localPosition = new Vector3(0, 2, -8);
            button.onClick.AddListener(ClickCheck);
            button.gameObject.name = buildings[i].type.name;

            //find min and max position of type menu
            if (posType > maxTypePos)
                maxTypePos = posType;
            if (posType < minTypePos)
                minTypePos = posType;

            if (posType >= 0)
                posType += 125;
            posType *= -1;


            // Individual Buildings
            GameObject newObj = new GameObject("new");
            GameObject parent = Instantiate(newObj, new Vector3(0, 0, 0), Quaternion.identity, types);
            parent.transform.localPosition = new Vector3(0, 0, 0);
            parent.name = buildings[i].type.name + "_buildings";    // Collection of building types: ie all the houses
            Destroy(newObj.gameObject);
            buildingsParents.Add(parent);

            int posBuild = 0;
            previousX = 0;
            nextX = 0;
            changePos = true;

            // Loop over Buildings in THIS type/category
            for (int u = 0; u < buildings[i].buildings.Length; u++)
            {
                // Create wrapper/parent to hold model, text and button
                GameObject tempBuild = new GameObject("new");
                GameObject buildParent = Instantiate(tempObj, new Vector3(0, 0, 0), Quaternion.identity, parent.transform);
                buildParent.transform.localPosition = new Vector3(-posBuild, 0, 0);
                buildParent.transform.localScale = new Vector3(9, 9, 9);
                buildParent.name = buildings[i].buildings[u].name;
                Destroy(tempBuild.gameObject);

                // Create building and add to building parent
                GameObject build = Instantiate(buildings[i].buildings[u], new Vector3(0, 0, 0), Quaternion.identity, buildParent.transform);
                build.transform.localPosition = new Vector3(0, 0, 0); // add rotation to the building to see it better
                build.transform.localRotation = Quaternion.Euler(menuBuildingsTilt, 0, 0);
                build.name = buildings[i].buildings[u].name;

                foreach (Transform trans in build.GetComponentsInChildren<Transform>(true))
                    trans.gameObject.layer = 5;     // set all to UI layer for camera culling
                parent.SetActive(false);

                // Add text to building - add to build parent
                text = Instantiate(textPRO, new Vector3(0, 0, 0), Quaternion.identity, buildParent.transform);
                text.transform.localPosition = new Vector3(0, textPosY, 0);
                text.text = buildings[i].buildings[u].GetComponent<BuildingProperties>().buildingName;

                // Add button to building - add to build parent
                Button buttonBuild = Instantiate(buildingButton, new Vector3(0, 0, 0), Quaternion.identity, buildParent.transform);
                buttonBuild.transform.localPosition = new Vector3(0, 2, 0);
                buttonBuild.onClick.AddListener(CreateBuilding);
                buttonBuild.gameObject.name = buildings[i].buildings[u].name;

                //find min and max position of each type
                if (posBuild > buildings[i].maxPos)
                    buildings[i].maxPos = posBuild;
                if (posBuild < buildings[i].minPos)
                    buildings[i].minPos = posBuild;

                int add = -35;
                try
                {
                    for (int y = 1; y < buildings[i].buildings[u].GetComponent<BuildingProperties>().spaceWidth; y++)
                    {
                        buttonBuild.transform.localPosition = new Vector3(buttonBuild.transform.localPosition.x + 5, 2, 0);
                        buttonBuild.transform.localScale = new Vector3(buttonBuild.transform.localScale.x + 1, 1, 1);
                        text.transform.localPosition = new Vector3(text.transform.localPosition.x + 5, textPosY, 0); // center the building text

                    }
                    for (int y = 0; y < buildings[i].buildings[u].GetComponent<BuildingProperties>().spaceWidth; y++)
                    {
                        add += -90;
                    }

                }
                catch { }

                if (changePos)
                {
                    nextX += add;
                    posBuild = nextX;
                    //changePos = false;
                }
                else
                {
                    previousX -= add;
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
        typeDel.name = deleteBuilding.name;


        foreach (Transform trans in typeDel.GetComponentsInChildren<Transform>(true))
            trans.gameObject.layer = 5;

        buildingsTypes.Add(typeDelParent);

        TextMeshProUGUI textDel = Instantiate(textPRO, new Vector3(0, 0, 0), Quaternion.identity, typeDelParent.transform);
        textDel.transform.localPosition = new Vector3(0, textPosY, 0);
        textDel.text = deleteBuilding.name;

        Button buttonDel = Instantiate(typeButton, new Vector3(0, 0, 0), Quaternion.identity, typeDelParent.transform);
        buttonDel.transform.localPosition = new Vector3(0, 2, 0);
        buttonDel.onClick.AddListener(DeleteBuilding);
        buttonDel.gameObject.name = deleteBuilding.name;

    }


    public void OnHomeButton()
    {
        CloseBuildMenu();
    }

    public void OpenBuildMenu()
    {
        ActivateMenu();

        activateMenu.SetActive(true);

        // Aux Menus 
        mainMenu.SetActive(false);
        homeButton.SetActive(true);
    }

    public void CloseBuildMenu()
    {
        ActivateMenu();

        activateMenu.SetActive(false);

        // Aux Menus 
        mainMenu.SetActive(true);
        homeButton.SetActive(false);
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

        print("Activate Menu");
        types.localPosition = new Vector3(0, 0, types.localPosition.z);

        if (cameraController.target != null)
            Destroy(cameraController.target.gameObject);
        else
            grid.enabled = !grid.isActiveAndEnabled;

        cameraController.target = null;
        cameraController.moveTarget = false;

        for (int i = 0; i < buildingsParents.Count; i++)
            buildingsParents[i].SetActive(false);
        for (int i = 0; i < buildingsTypes.Count; i++)
            buildingsTypes[i].SetActive(true);

        activateMenu.SetActive(!activateMenu.activeSelf);

        minPos = minTypePos;
        maxPos = maxTypePos;

    }


    public void ClickCheck()
    {
        print("CLICKCHECK");
        print(EventSystem.current.currentSelectedGameObject.name);
        if (cameraController.doubleClick)
        {
            cameraController.doubleClick = false;
            cameraController.lastClickTime = 0;
            for (int i = 0; i < buildingsParents.Count; i++)
            {
                //buildingsTypes[i].SetActive(false);
                if (EventSystem.current.currentSelectedGameObject.name + "_buildings" == buildingsParents[i].name)
                {
                    buildingsParents[i].SetActive(true);
                    minPos = buildings[i].minPos;
                    maxPos = buildings[i].maxPos;
                }
            }
            for (int i = 0; i < buildingsTypes.Count; i++)
            {
                buildingsTypes[i].SetActive(false);
            }
            //set active delete tool
        }
    }


    public void CreateBuilding()
    {
        if (cameraController.doubleClick)
        {
            cameraController.doubleClick = false;
            cameraController.lastClickTime = 0;
            if (cameraController.target != null && cameraController.target.gameObject != null) Destroy(cameraController.target.gameObject);


            for (int i = 0; i < buildings.Length; i++)
            {
                for (int u = 0; u < buildings[i].buildings.Length; u++)
                {
                    if (buildings[i].buildings[u].name == EventSystem.current.currentSelectedGameObject.name)
                    {
                        cameraController.moveTarget = true;
                        Transform target = Instantiate(buildings[i].buildings[u], new Vector3(0, 0, 0), Quaternion.identity).transform;
                        cameraController.target = target;
                    }
                }
            }

            for (int i = 0; i < buildingsParents.Count; i++)
            {
                buildingsTypes[i].SetActive(true);
                buildingsParents[i].SetActive(false);
            }

            // activateMenu.SetActive(!activateMenu.activeSelf);

            minPos = minTypePos;
            maxPos = maxTypePos;
        }
    }


    public void DeleteBuilding()
    {
        print("Delete Building");

        if (cameraController.target != null && cameraController.target.gameObject != null) Destroy(cameraController.target.gameObject);
        cameraController.moveTarget = true;


        Transform target = Instantiate(deleteBuilding, new Vector3(0, 0, 0), Quaternion.identity).transform;
        target.transform.GetChild(0).localPosition = new Vector3(0, 6, 0);
        cameraController.target = target;
        activateMenu.SetActive(false);
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

    public static string ConvertToLabel(string camelCaseString)
    {
        // Add a space before each uppercase letter except the first one
        string spacedString = Regex.Replace(camelCaseString, "(\\B[A-Z])", " $1");

        // Capitalize the first letter
        return char.ToUpper(spacedString[0]) + spacedString.Substring(1);
    }

    public static string ConvertToCamelCase(string label)
    {
        // Split the string by spaces into words
        string[] words = label.Split(' ');

        // Lowercase the first word
        string camelCaseString = words[0].ToLower();

        // Capitalize the first letter of the remaining words and append them
        for (int i = 1; i < words.Length; i++)
        {
            camelCaseString += CultureInfo.CurrentCulture.TextInfo.ToTitleCase(words[i].ToLower());
        }

        return camelCaseString;
    }

}
