using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeleteUnnecessary : MonoBehaviour
{

    public GameObject unnecessary;

    private void Awake()
    {
        Destroy(unnecessary);
    }
}
