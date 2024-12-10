using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

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

        //directionalLight.gameObject.SetActive(false);
    }

    IEnumerator ActivateLight()
    {
        //directionalLight.gameObject.SetActive(true);

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


//yield return new WaitForEndOfFrame();
