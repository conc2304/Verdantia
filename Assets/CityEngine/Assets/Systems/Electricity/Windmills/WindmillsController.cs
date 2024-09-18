using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindmillsController : MonoBehaviour
{

    public float windSpeed = 3;

    public List<Transform> blades = new List<Transform>();


    void Update()
    {
        if (blades.Count != 0)
        {
            blades[0].Rotate(Vector3.forward, Time.deltaTime * 10 * windSpeed);


            for (int i = 1; i < blades.Count; i++)
                blades[i].localRotation = blades[0].localRotation;
        }
    }

}
