using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
Part of the "City Engine" Asset from the Unity Asset store (unchanged)
onnects a light object to the night-time lighting system. On start, 
it activates or deactivates the light based on whether it is currently night.
It also registers the light with the Lightning system by adding it to the lights list in the Lightning script, 
enabling it to be toggled during day-night transitions.
**/

public class LightConnect : MonoBehaviour
{

    void Start()
    {
        this.gameObject.SetActive(Lightning.night);
        FindObjectOfType<Lightning>().lights.Add(this.gameObject);
    }

}
