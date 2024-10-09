using System.Collections;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Linq;

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

    public Dictionary<string, (int min, int max)> propertyRanges = new Dictionary<string, (int, int)>();


    public RectTransform trackpadRect; // The rectangular UI element acting as the trackpad

    private Rotate rotScript;

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
                isDragging = true; // Start dragging
                dragStartPos = Input.mousePosition;
            }
        }

        if (Input.GetMouseButton(0) && isDragging)
        {
            // Continue the drag if dragging was initiated within the bounds
            dragTargetPos = Input.mousePosition;
            Vector3 dragDelta = dragTargetPos - dragStartPos;

            toPos = dragDelta * dragMultiplier;
            posX = -toPos.x;
            print("Drag PosX: " + posX);

            dragStartPos = dragTargetPos; // Update the start position for smooth dragging
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false; // Stop dragging when the mouse is released
        }
        else
        {
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
            // Building Types/Categories

            GameObject tempObj = new GameObject("new");
            GameObject typeParent = Instantiate(tempObj, new Vector3(0, 0, 0), Quaternion.identity, types);
            typeParent.transform.localPosition = new Vector3(-posType, 0, 0);
            typeParent.transform.localScale = new Vector3(9, 9, 9);


            GameObject type = Instantiate(
                buildings[i].type,
                new Vector3(0, 0, 0),
                Quaternion.identity,
                typeParent.transform
            );

            typeParent.name = buildings[i].type.name + "";
            Destroy(tempObj.gameObject);




            type.transform.localPosition = new Vector3(0, 0, 0);
            // type.transform.localScale = new Vector3(9, 9, 9);

            type.transform.localRotation = Quaternion.Euler(-30, 0, 0);

            type.name = buildings[i].type.name;

            foreach (Transform trans in type.GetComponentsInChildren<Transform>(true))
                trans.gameObject.layer = 5;

            buildingsTypes.Add(typeParent);

            TextMeshProUGUI text = Instantiate(textPRO, new Vector3(0, 0, 0), Quaternion.identity, typeParent.transform);
            text.transform.localPosition = new Vector3(0, textPosY, 0);
            text.text = buildings[i].name;

            Button button = Instantiate(typeButton, new Vector3(0, 0, 0), Quaternion.identity, typeParent.transform);
            button.transform.localPosition = new Vector3(0, 2, 0);
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
            parent.name = buildings[i].type.name + "_buildings";    // Wrapper
            Destroy(newObj.gameObject);
            buildingsParents.Add(parent);

            int posBuild = 0;
            previousX = 0;
            nextX = 0;
            changePos = true;

            for (int u = 0; u < buildings[i].buildings.Length; u++)
            {
                GameObject build = Instantiate(buildings[i].buildings[u], new Vector3(0, 0, 0), Quaternion.identity, parent.transform);
                build.transform.localPosition = new Vector3(-posBuild, 0, 0);
                build.transform.localScale = new Vector3(9, 9, 9);
                build.name = buildings[i].buildings[u].name;

                foreach (Transform trans in build.GetComponentsInChildren<Transform>(true))
                    trans.gameObject.layer = 5;
                parent.SetActive(false);

                Button buttonBuild = Instantiate(buildingButton, new Vector3(0, 0, 0), Quaternion.identity, build.transform);
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
                    }
                    for (int y = 0; y < buildings[i].buildings[u].GetComponent<BuildingProperties>().spaceWidth; y++)
                    {
                        add += -90;
                    }

                }
                catch
                {

                }
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
                catch
                {

                }
            }
        }

        //delete buildings button
        GameObject typeDel = Instantiate(deleteBuilding, new Vector3(0, 0, 0), Quaternion.identity, types);
        typeDel.transform.localPosition = new Vector3(-posType, 0, 0);
        typeDel.transform.localScale = new Vector3(9, 9, 9);
        typeDel.name = deleteBuilding.name;
        foreach (Transform trans in typeDel.GetComponentsInChildren<Transform>(true))
            trans.gameObject.layer = 5;
        buildingsTypes.Add(typeDel);

        TextMeshProUGUI textDel = Instantiate(textPRO, new Vector3(0, 0, 0), Quaternion.identity, typeDel.transform);
        textDel.transform.localPosition = new Vector3(0, textPosY, 0);
        textDel.text = deleteBuilding.name;

        Button buttonDel = Instantiate(typeButton, new Vector3(0, 0, 0), Quaternion.identity, typeDel.transform);
        buttonDel.transform.localPosition = new Vector3(0, 2, 0);
        buttonDel.onClick.AddListener(DeleteBuilding);
        buttonDel.gameObject.name = deleteBuilding.name;

    }


    public void OnHomeButton()
    {
        CloseBuildMenu();
        CloseHeatMap();
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

    public void OpenHeatMap()
    {
        cameraController.SetHeatMapView(true);
        homeButton.SetActive(false);
    }

    public void CloseHeatMap()
    {
        cameraController.SetHeatMapView(false);
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

            activateMenu.SetActive(!activateMenu.activeSelf);

            minPos = minTypePos;
            maxPos = maxTypePos;
        }
    }


    public void DeleteBuilding()
    {
        print("Delete Building");
        cameraController.moveTarget = true;
        Transform target = Instantiate(deleteBuilding, new Vector3(0, 0, 0), Quaternion.identity).transform;
        target.transform.GetChild(0).localPosition = new Vector3(0, 6, 0);
        cameraController.target = target;
        activateMenu.SetActive(false);
    }

    Dictionary<string, (int min, int max)> UpdatePropertyRanges()
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

}
