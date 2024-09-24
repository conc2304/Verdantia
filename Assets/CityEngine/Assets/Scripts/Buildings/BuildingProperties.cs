using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingProperties : MonoBehaviour
{

    public int buildingIndex = -1;

    public GameObject environment;

    public bool connectToRoad;
    public PathTarget[] carsPathTargetsToConnect;
    public Transform[] carsPathTargetsToSpawn;
    public PathTarget[] citizensPathTargetsToConnect;
    public Transform[] citizensPathTargetsToSpawn;

    public Transform[] additionalSpace;

    public BuildConstruction buildConstruction;
    public float buildingHigh = 1;
    public float buildingTime = 10;

    public int spaceWidth = 1;

    // Heat Contribution Props
    public int heatContribution = 50;

    void Start()
    {
        // heatContribution = Random.Range(1, 101);

        if (gameObject.CompareTag("Space"))
        {
            heatContribution = transform.parent.parent.GetComponent<BuildingProperties>().heatContribution;
            print("Space heat = " + heatContribution);
        }
    }
}
