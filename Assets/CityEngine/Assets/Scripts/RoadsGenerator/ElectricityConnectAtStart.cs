using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
