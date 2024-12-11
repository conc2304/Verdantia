using UnityEngine;
using UnityEditor;
/**
Part of the "City Engine" Asset from the Unity Asset store (unchanged and unused)
**/

public class Example
{

    [MenuItem("Tools/Create Forest (select prefabs)")]
    static void InstantiatePrefab()
    {
        int width = 100;
        int length = 100;
        int widthPos = 0;
        int lengthPos = 0;
        GameObject obj;
        for (int i = 0; i < width; i++)
        {
            widthPos += 10;
            for (int u = 0; u < length; u++)
            {
                lengthPos += 10;

                int rot = 0;
                int randomRot = Random.Range(0, 4);
                if (randomRot == 0) rot = 0;
                if (randomRot == 1) rot = 90;
                if (randomRot == 2) rot = -90;
                if (randomRot == 3) rot = 180;

                obj = PrefabUtility.InstantiatePrefab(Selection.objects[Random.Range(0, Selection.objects.Length)]) as GameObject;
                obj.transform.position = new Vector3(widthPos, 0, lengthPos);
                obj.transform.rotation = Quaternion.Euler(0, rot, 0);
            }
            lengthPos = 0;
        }
    }
}

