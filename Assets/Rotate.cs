using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour
{
    // Start is called before the first frame update
    public int rotateXSpeed = 0;
    public int rotateYSpeed = 0;
    public int rotateZSpeed = 0;

    private int multiplier = 1;
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(rotateXSpeed * Time.deltaTime * multiplier, rotateYSpeed * Time.deltaTime * multiplier, rotateZSpeed * Time.deltaTime * multiplier);
    }
}
