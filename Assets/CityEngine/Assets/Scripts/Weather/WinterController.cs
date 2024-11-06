
using UnityEngine;

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
