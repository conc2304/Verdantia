using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightConnect : MonoBehaviour
{
    
    void Start()
    {
        this.gameObject.SetActive(Lightning.night);
        FindObjectOfType<Lightning>().lights.Add(this.gameObject);
    }

}
