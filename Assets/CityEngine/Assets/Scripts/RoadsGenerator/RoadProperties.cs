using UnityEngine;

/**
Part of the "City Engine" Asset from the Unity Asset store (unchanged)
Defines properties for roads in a city-building game. It includes indices for associated buildings, 
arrays for path targets to handle vehicle and citizen movement, and connection points for electric pillars. 
This setup helps manage road and infrastructure connectivity within the game environment.
**/

public class RoadProperties : MonoBehaviour
{

    public int buildingIndex = -1;

    public PathTarget[] pathTargetsStart;
    public PathTarget[] pathTargetsLast;

    public PathTarget[] pathToConnectBuilding;

    public PathTarget[] citizensPathToConnectBuilding;

    // electric pillars
    public Transform[] startTargetsToConnect;
    public Transform[] targetsToConnect;
}
