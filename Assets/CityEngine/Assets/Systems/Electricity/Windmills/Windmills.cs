using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Windmills : MonoBehaviour
{
    public Transform[] blades;


    private void Start()
    {
        WindmillsController windmillsController = FindObjectOfType<WindmillsController>();

        for (int i = 0; i < blades.Length; i++)
            windmillsController.blades.Add(blades[i]);
    }

}
