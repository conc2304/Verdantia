
using UnityEngine;

/**
Part of the "City Engine" Asset from the Unity Asset store (unchanged)
Uses a LineRenderer to visually connect pairs of points with electricity lines when the scene starts. 
It iterates through arrays of firstPoints and secondPoints, instantiating a line for each pair and setting their positions accordingly. 
The created lines are parented to the respective firstPoints for organization.
**/
public class ElectricityConnectAtStart : MonoBehaviour
{

    public LineRenderer electricityLine;

    public Transform[] firstPoints;
    public Transform[] secondPoints;

    void Start()
    {
        for (int i = 0; i < firstPoints.Length; i++)
        {
            LineRenderer line = Instantiate(electricityLine);

            line.SetPosition(0, firstPoints[i].transform.position);
            line.SetPosition(1, secondPoints[i].transform.position);

            line.transform.parent = firstPoints[i].transform;
        }
    }
}
