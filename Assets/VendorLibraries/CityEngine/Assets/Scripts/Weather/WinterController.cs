
using UnityEngine;

/**
controls a winter weather simulation in Unity. 
When the "Y" key is pressed, it toggles the winterWeather state, activating or deactivating snowfall by adjusting the particle system's emission rate.
 Additionally, it smoothly modifies a shader parameter (_Winter) over time, creating a gradual transition between winter and non-winter states. 
The timer variable ensures the effect transitions smoothly at a set pace (adjusted by dividing Time.deltaTime by 4).
**/
public class WinterController : MonoBehaviour
{

    public bool winterWeather;

    float timer;

    public ParticleSystem snow;

    private void Start()
    {
        var em = snow.emission;
        em.rateOverTime = 0;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Y))
        {
            winterWeather = !winterWeather;
            if (winterWeather)
            {
                var em = snow.emission;
                em.rateOverTime = 100;
            }
            else
            {
                var em = snow.emission;
                em.rateOverTime = 0;
            }
        }

        if (winterWeather)
        {
            if (timer <= 1)
            {
                timer += Time.deltaTime / 4;
                Shader.SetGlobalFloat("_Winter", timer);
            }
        }
        else
        {
            if (timer >= 0)
            {
                timer -= Time.deltaTime / 4;
                Shader.SetGlobalFloat("_Winter", timer);
            }
        }
    }
}
