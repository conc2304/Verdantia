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
    int previousY, nextY;

    bool changePos = false;

    private Vector3 toPos;
    public float posY;

    private Vector3 dragStartPos;
    private Vector3 dragTargetPos;

    private CameraController cameraController;
    private RoadGenerator roadGenerator;

    bool doubleClick;

    public Dictionary<string, (int min, int max)> propertyRanges = new Dictionary<string, (int, int)>();


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
            // MouseInput();
            InputHandler();

            if (types.localPosition.y > maxPos)
            {
                print("types pos over max");
                types.localPosition = new Vector3(Mathf.Lerp(types.localPosition.y, maxPos, Time.deltaTime), 0, 0);
                if (posY < 0)
                    posY = posY / 2;
            }
            if (types.localPosition.y < minPos)
            {
                print("types pos under min");

                types.localPosition = new Vector3(Mathf.Lerp(types.localPosition.y, minPos, Time.deltaTime), 0, 0);
                if (posY > 0)
                    posY = posY / 2;
            }

            types.localPosition = new Vector3(0, Mathf.Lerp(types.localPosition.y, types.localPosition.x + posY * 5, Time.deltaTime), 0);
        }
    }



    void InputHandler()
    {
        // Handle both mouse and touch input
        if (Input.touchCount > 0) // Handle touch input
        {
            Touch touch = Input.GetTouch(0); // Get the first touch

            if (touch.phase == TouchPhase.Began)
            {
                HandleDragStart(touch.position);
            }
            else if (touch.phase == TouchPhase.Moved)
            {
                HandleDragMove(touch.position);
            }
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                ResetDrag();
            }
        }
        else if (Input.GetMouseButtonDown(0)) // Handle mouse input
        {
            HandleDragStart(Input.mousePosition);
        }
        else if (Input.GetMouseButton(0))
        {
            HandleDragMove(Input.mousePosition);
        }
        else
        {
            ResetDrag();
        }
    }

    void HandleDragStart(Vector3 inputPosition)
    {
        // Print "Mouse Clicked" or "Touch Began"
        Plane plane = new Plane(Vector3.up, Vector3.zero);
        Ray ray = menuCamera.ScreenPointToRay(inputPosition);

        float entry;
        if (plane.Raycast(ray, out entry))
        {
            dragStartPos = ray.GetPoint(entry);
            // print("DragStart: " + dragStartPos);
        }

    }

    void HandleDragMove(Vector3 inputPosition)
    {
        // Print "Mouse Down / Drag" or "Touch Moved"
        Plane plane = new Plane(Vector3.up, Vector3.zero);
        Ray ray = menuCamera.ScreenPointToRay(inputPosition);

        float entry;
        if (plane.Raycast(ray, out entry))
        {
            dragTargetPos = ray.GetPoint(entry);
            // print("Drag Distance_New = " + (dragStartPos - dragTargetPos));
            toPos = transform.position + dragStartPos - dragTargetPos;
            // print("To Pos NEW : " + toPos);

            if (Mathf.Abs(toPos.y) > Mathf.Abs(toPos.z))
                if (dragStartPos.z < 750)
                    posY = toPos.y;
        }
    }

    void ResetDrag()
    {
        // Reset drag position when there is no input
        if (posY > 0)
            posY -= Time.deltaTime * 100;
        if (posY < 0)
            posY += Time.deltaTime * 100;
    }
    void MouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // print("Mouse Clicked");

            Plane plane = new Plane(Vector3.up, Vector3.zero);
            Ray ray = menuCamera.ScreenPointToRay(Input.mousePosition);

            float entry;
            if (plane.Raycast(ray, out entry))
            {
                dragStartPos = ray.GetPoint(entry);
                print("DragStart: " + dragStartPos);
            }
        }
        if (Input.GetMouseButton(0))
        {
            // print("Mouse Down / Drag");
            Plane plane = new Plane(Vector3.up, Vector3.zero);
            Ray ray = menuCamera.ScreenPointToRay(Input.mousePosition);

            float entry;
            if (plane.Raycast(ray, out entry))
            {
                dragTargetPos = ray.GetPoint(entry);
                print("Drag Distance_New = " + (dragStartPos - dragTargetPos));
                toPos = transform.position + dragStartPos - dragTargetPos;
                print("To Pos NEW : " + toPos);
                if (Mathf.Abs(toPos.y) > Mathf.Abs(toPos.z))
                    if (dragStartPos.z < 750)
                        posY = toPos.y;
            }
        }
        else
        {
            // print("No Input / Reset");
            if (posY > 0)
                posY -= Time.deltaTime * 100;
            if (posY < 0)
                posY += Time.deltaTime * 100;
        }
    }

    void CreateTypes_OLD()
    {
        int posType = 0;
        for (int i = 0; i < buildings.Length; i++)
        {
            //types
            GameObject type = Instantiate(buildings[i].type, new Vector3(0, 0, 0), Quaternion.identity, types);
            type.transform.localPosition = new Vector3(0, -posType, 0);
            type.transform.localScale = new Vector3(9, 9, 9);
            type.name = buildings[i].type.name;
            foreach (Transform trans in type.GetComponentsInChildren<Transform>(true))
                trans.gameObject.layer = 5; // set to UI layer
            buildingsTypes.Add(type);

            TextMeshProUGUI text = Instantiate(textPRO, new Vector3(0, 0, 0), Quaternion.identity, type.transform);
            text.transform.localPosition = new Vector3(8, 0, 0);
            text.text = buildings[i].name;

            Button button = Instantiate(typeButton, new Vector3(0, 0, 0), Quaternion.identity, type.transform);
            button.transform.localPosition = new Vector3(2, 0, 0);
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


            //buildings
            GameObject newObj = new GameObject("new");
            GameObject parent = Instantiate(newObj, new Vector3(0, 0, 0), Quaternion.identity, types);
            parent.transform.localPosition = new Vector3(0, 0, 0);
            parent.name = buildings[i].type.name + "_buildings";
            Destroy(newObj.gameObject);
            buildingsParents.Add(parent);

            int posBuild = 0;
            previousY = 0;
            nextY = 0;
            changePos = true;
            for (int u = 0; u < buildings[i].buildings.Length; u++)
            {
                GameObject build = Instantiate(buildings[i].buildings[u], new Vector3(0, 0, 0), Quaternion.identity, parent.transform);
                build.transform.localPosition = new Vector3(0, -posBuild, 0);
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
                        buttonBuild.transform.localPosition = new Vector3(2, buttonBuild.transform.localPosition.y + 5, 0);
                        buttonBuild.transform.localScale = new Vector3(1, buttonBuild.transform.localScale.y + 1, 1);
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
                    nextY += add;
                    posBuild = nextY;
                    //changePos = false;
                }
                else
                {
                    previousY -= add;
                    posBuild = previousY;
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
        typeDel.transform.localPosition = new Vector3(0, -posType, 0);
        typeDel.transform.localScale = new Vector3(9, 9, 9);
        typeDel.name = deleteBuilding.name;
        foreach (Transform trans in typeDel.GetComponentsInChildren<Transform>(true))
            trans.gameObject.layer = 5;
        buildingsTypes.Add(typeDel);

        TextMeshProUGUI textDel = Instantiate(textPRO, new Vector3(0, 0, 0), Quaternion.identity, typeDel.transform);
        textDel.transform.localPosition = new Vector3(8, 0, 0);
        textDel.text = deleteBuilding.name;

        Button buttonDel = Instantiate(typeButton, new Vector3(0, 0, 0), Quaternion.identity, typeDel.transform);
        buttonDel.transform.localPosition = new Vector3(2, 0, 0);
        buttonDel.onClick.AddListener(DeleteBuilding);
        buttonDel.gameObject.name = deleteBuilding.name;

    }

    void CreateTypes()
    {
        int posTypeY = 0; // Adjust Y-axis for vertical list
        for (int i = 0; i < buildings.Length; i++)
        {
            //types
            GameObject type = Instantiate(buildings[i].type, new Vector3(0, 0, 0), Quaternion.identity, types);
            type.transform.localPosition = new Vector3(0, -posTypeY, 0); // Adjust the Y-axis for vertical alignment
            type.transform.localScale = new Vector3(9, 9, 9);
            type.name = buildings[i].type.name;
            foreach (Transform trans in type.GetComponentsInChildren<Transform>(true))
                trans.gameObject.layer = 5;
            buildingsTypes.Add(type);

            TextMeshProUGUI text = Instantiate(textPRO, new Vector3(0, 0, 0), Quaternion.identity, type.transform);
            text.transform.localPosition = new Vector3(0, 8, 0);
            text.text = buildings[i].name;

            Button button = Instantiate(typeButton, new Vector3(0, 0, 0), Quaternion.identity, type.transform);
            button.transform.localPosition = new Vector3(0, 2, 0);
            button.onClick.AddListener(ClickCheck);
            button.gameObject.name = buildings[i].type.name;

            // Increment Y-axis for the vertical position
            posTypeY += 125; // Adjust this value for spacing between items

            //buildings
            GameObject newObj = new GameObject("new");
            GameObject parent = Instantiate(newObj, new Vector3(0, 0, 0), Quaternion.identity, types);
            parent.transform.localPosition = new Vector3(0, -posTypeY, 0); // Vertical alignment for the parent
            parent.name = buildings[i].type.name + "_buildings";
            Destroy(newObj.gameObject);
            buildingsParents.Add(parent);

            int posBuildY = 0;
            previousY = 0;
            nextY = 0;
            changePos = true;
            for (int u = 0; u < buildings[i].buildings.Length; u++)
            {
                GameObject build = Instantiate(buildings[i].buildings[u], new Vector3(0, 0, 0), Quaternion.identity, parent.transform);
                build.transform.localPosition = new Vector3(0, -posBuildY, 0); // Adjust for vertical list inside parent
                build.transform.localScale = new Vector3(9, 9, 9);
                build.name = buildings[i].buildings[u].name;
                foreach (Transform trans in build.GetComponentsInChildren<Transform>(true))
                    trans.gameObject.layer = 5;
                parent.SetActive(false);

                Button buttonBuild = Instantiate(buildingButton, new Vector3(0, 0, 0), Quaternion.identity, build.transform);
                buttonBuild.transform.localPosition = new Vector3(0, 2, 0);
                buttonBuild.onClick.AddListener(CreateBuilding);
                buttonBuild.gameObject.name = buildings[i].buildings[u].name;

                // Increment Y-axis for vertical positioning within the parent
                posBuildY += 125; // Adjust this for spacing between buildings

                try
                {
                    build.GetComponent<BuildingProperties>().buildConstruction.enabled = false;
                }
                catch
                {

                }
            }
        }

        // delete buildings button
        GameObject typeDel = Instantiate(deleteBuilding, new Vector3(0, 0, 0), Quaternion.identity, types);
        typeDel.transform.localPosition = new Vector3(0, -posTypeY, 0); // Adjust to add this button to the vertical list
        typeDel.transform.localScale = new Vector3(9, 9, 9);
        typeDel.name = deleteBuilding.name;
        foreach (Transform trans in typeDel.GetComponentsInChildren<Transform>(true))
            trans.gameObject.layer = 5;
        buildingsTypes.Add(typeDel);

        TextMeshProUGUI textDel = Instantiate(textPRO, new Vector3(0, 0, 0), Quaternion.identity, typeDel.transform);
        textDel.transform.localPosition = new Vector3(0, 8, 0);
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
        types.localPosition = new Vector3(0, 0, 0);

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

        print("Minpos: " + minPos);
        print("MaxPos: " + maxPos);

    }


    public void ClickCheck()
    {
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
        activateMenu.SetActive(!activateMenu.activeSelf);
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
