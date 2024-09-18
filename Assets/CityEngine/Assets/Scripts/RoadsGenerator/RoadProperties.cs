using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
