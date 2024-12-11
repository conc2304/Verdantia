using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/**
Part of the "City Engine" Asset from the Unity Asset store (unchanged)

Manages a day-night cycle with smooth transitions in a Unity environment. 
It toggles between day and night states based on a timer, adjusting lighting and activating or deactivating specific lights accordingly. 
The directional light's intensity is animated to simulate gradual changes, while a global shader property, _NightEmission, 
is adjusted to represent the lighting conditions. 
The day-night cycle duration and the option to disable the night mode are configurable.
**/

public class Lightning : MonoBehaviour
{

    public Light directionalLight;

    public static bool night = false;

    public List<GameObject> lights = new List<GameObject>();

    public bool switchDayNight = false;

    public float dayTime = 30;
    public bool disableNight = true;

    float timer;

    void Start()
    {
        Shader.SetGlobalFloat("_NightEmission", 0);
    }


    void Update()
    {
        if (disableNight) return;

        timer += Time.deltaTime;
        if (timer > dayTime)
            switchDayNight = true;

        if (switchDayNight)
        {
            night = !night;

            if (night)
            {
                //Shader.SetGlobalFloat("_NightEmission", 5);
                StartCoroutine(DissableLight());
            }
            else
            {
                //Shader.SetGlobalFloat("_NightEmission", 0);
                StartCoroutine(ActivateLight());
            }
            StartNight();

            timer = 0;
            switchDayNight = false;
        }
    }

    IEnumerator DissableLight()
    {
        float timerActivate = Mathf.FloorToInt(directionalLight.intensity);

        while (directionalLight.intensity > 0.4f)
        {
            timerActivate -= Time.deltaTime;
            directionalLight.intensity = timerActivate;

            if (Shader.GetGlobalFloat("_NightEmission") < 1)
                Shader.SetGlobalFloat("_NightEmission", 1);

            yield return new WaitForSeconds(.1f);
        }

    }

    IEnumerator ActivateLight()
    {

        float timerActivate = Mathf.FloorToInt(directionalLight.intensity);

        while (directionalLight.intensity < 1)
        {
            timerActivate += Time.deltaTime;
            directionalLight.intensity += timerActivate;

            if (Shader.GetGlobalFloat("_NightEmission") > 0)
                Shader.SetGlobalFloat("_NightEmission", 0);

            yield return new WaitForSeconds(.1f);
        }
    }

    void StartNight()
    {
        lights.RemoveAll(GameObject => GameObject == null);

        for (int i = 0; i < lights.Count; i++)
            lights[i].SetActive(night);
    }

}


