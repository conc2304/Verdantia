using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class PathTargetManager : EditorWindow
{

    [MenuItem("Tools/Path Target Manager")]
    public static void Open()
    {
        GetWindow<PathTargetManager>();
    }

    public Transform pathTargetRoot;

    GameObject pathTargetObj;
    PathTarget pathTarget;
    PathTarget newPathTarget;
    PathTarget selectedPathTarget;
    PathTarget branchesFrom;

    private void OnGUI()
    {
        SerializedObject obj = new SerializedObject(this);

        EditorGUILayout.PropertyField(obj.FindProperty("pathTargetRoot"));

        if (pathTargetRoot == null)
        {
            EditorGUILayout.HelpBox("No root transform", MessageType.Warning);
        }
        else
        {
            EditorGUILayout.BeginVertical("box");
            DrawButtons();
            EditorGUILayout.EndVertical();
        }

        obj.ApplyModifiedProperties();
    }

    void DrawButtons()
    {
        if (GUILayout.Button("Create Path Target"))
        {
            CreatePathTarget();
        }
        if (Selection.activeGameObject != null && Selection.activeGameObject.GetComponent<PathTarget>())
        {
            if (GUILayout.Button("Add Branch"))
                AddBranch();
            if (GUILayout.Button("PathTarget Before"))
                PathTargetBefore();
            if (GUILayout.Button("PathTarget After"))
                PathTargetAfter();
            if (GUILayout.Button("PathTarget Delete"))
                PathTargetDelete();
        }
    }

    void AddBranch()
    {
        pathTargetObj = new GameObject("PathTarget " + pathTargetRoot.childCount, typeof(PathTarget));
        pathTargetObj.transform.SetParent(pathTargetRoot, false);

        pathTarget = pathTargetObj.GetComponent<PathTarget>();

        branchesFrom = Selection.activeGameObject.GetComponent<PathTarget>();
        branchesFrom.branches.Add(pathTarget);

        pathTarget.transform.position = branchesFrom.transform.position;
        pathTarget.transform.forward = branchesFrom.transform.forward;

        Selection.activeGameObject = pathTarget.gameObject;
    }

    void PathTargetBefore()
    {
        pathTargetObj = new GameObject("PathTarget " + pathTargetRoot.childCount, typeof(PathTarget));
        pathTargetObj.transform.SetParent(pathTargetRoot, false);

        newPathTarget = pathTargetObj.GetComponent<PathTarget>();

        selectedPathTarget = Selection.activeGameObject.GetComponent<PathTarget>();

        pathTargetObj.transform.position = selectedPathTarget.transform.position;
        pathTargetObj.transform.forward = selectedPathTarget.transform.forward;

        if (selectedPathTarget.previousPathTarget != null)
        {
            newPathTarget.previousPathTarget = selectedPathTarget.previousPathTarget;
            selectedPathTarget.previousPathTarget.nextPathTarget = newPathTarget;
        }

        newPathTarget.nextPathTarget = selectedPathTarget;

        selectedPathTarget.previousPathTarget = newPathTarget;

        newPathTarget.transform.SetSiblingIndex(selectedPathTarget.transform.GetSiblingIndex());

        Selection.activeGameObject = newPathTarget.gameObject;
    }

    void PathTargetAfter()
    {
        pathTargetObj = new GameObject("PathTarget " + pathTargetRoot.childCount, typeof(PathTarget));
        pathTargetObj.transform.SetParent(pathTargetRoot, false);

        newPathTarget = pathTargetObj.GetComponent<PathTarget>();

        selectedPathTarget = Selection.activeGameObject.GetComponent<PathTarget>();

        pathTargetObj.transform.position = selectedPathTarget.transform.position;
        pathTargetObj.transform.forward = selectedPathTarget.transform.forward;

        newPathTarget.previousPathTarget = selectedPathTarget;

        if (selectedPathTarget.nextPathTarget != null)
        {
            selectedPathTarget.nextPathTarget.previousPathTarget = newPathTarget;
            newPathTarget.nextPathTarget = selectedPathTarget.nextPathTarget;
        }

        selectedPathTarget.nextPathTarget = newPathTarget;

        newPathTarget.transform.SetSiblingIndex(selectedPathTarget.transform.GetSiblingIndex());

        Selection.activeGameObject = newPathTarget.gameObject;
    }

    void PathTargetDelete()
    {
        selectedPathTarget = Selection.activeGameObject.GetComponent<PathTarget>();

        if (selectedPathTarget.nextPathTarget != null)
        {
            selectedPathTarget.nextPathTarget.previousPathTarget = selectedPathTarget.previousPathTarget;
        }
        if (selectedPathTarget.previousPathTarget != null)
        {
            selectedPathTarget.previousPathTarget.nextPathTarget = selectedPathTarget.nextPathTarget;
            Selection.activeGameObject = selectedPathTarget.previousPathTarget.gameObject;
        }

        DestroyImmediate(selectedPathTarget.gameObject);
    }

    void CreatePathTarget()
    {
        pathTargetObj = new GameObject("PathTarget " + pathTargetRoot.childCount, typeof(PathTarget));
        pathTargetObj.transform.SetParent(pathTargetRoot, false);

        pathTarget = pathTargetObj.GetComponent<PathTarget>();
        if (pathTargetRoot.childCount > 1)
        {
            pathTarget.previousPathTarget = pathTargetRoot.GetChild(pathTargetRoot.childCount - 2).GetComponent<PathTarget>();
            pathTarget.previousPathTarget.nextPathTarget = pathTarget;

            pathTarget.transform.position = pathTarget.previousPathTarget.transform.position;
            pathTarget.transform.forward = pathTarget.previousPathTarget.transform.position;
        }

        Selection.activeGameObject = pathTarget.gameObject;
    }

}
