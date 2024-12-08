using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildConstruction : MonoBehaviour
{

    public float buildTime = 10;
    float buildingHigh = 1;

    Renderer[] rend;
    public float timer;

    public bool builded;
    public bool isInitialLoad = false;  // for buildings that are being loaded from memory, aka not spawned by player action

    public ParticleSystem startBuildPS;
    public ParticleSystem finishBuildPS;

    public GameObject environment;
    public Transform renderers;

    [HideInInspector]
    public BuildingProperties target;
    [HideInInspector]
    public RoadGenerator roadGenerator;
    [HideInInspector]
    public BuildingProperties buildingProperties;
    [HideInInspector]
    public CameraController cameraController;

    void Awake()
    {
        environment.SetActive(false);

        rend = new Renderer[this.transform.GetChild(0).childCount];
        for (int i = 0; i < this.transform.GetChild(0).childCount; i++)
        {
            rend[i] = this.transform.GetChild(0).GetChild(i).GetComponent<Renderer>();
            rend[i].material.SetFloat("_Build", 0);
            rend[i].gameObject.SetActive(false);
        }

    }

    public void StartBuild()
    {
        environment.SetActive(true);

        buildingHigh = buildingProperties.buildingHigh;
        buildTime = buildingProperties.buildingTime;
        StartCoroutine(BuildConstructionCorutine());

        ParticleSystem ps = startBuildPS;
        ps.Stop();
        var main = ps.main;
        main.duration = buildTime;
        startBuildPS.Play();

        buildingProperties.environment.SetActive(false);
        for (int i = 0; i < this.transform.GetChild(0).childCount; i++)
        {
            rend[i].gameObject.SetActive(true);
        }

    }

    public IEnumerator BuildConstructionCorutine()
    {
        while (builded == false)
        {
            timer += Time.deltaTime / buildTime;

            if (renderers.gameObject.activeSelf)
            {
                for (int i = 0; i < this.transform.GetChild(0).childCount; i++)
                {
                    rend[i].material.SetFloat("_Build", timer);
                    rend[i].material.SetFloat("_MaxHigh", buildingHigh);
                }
            }

            if (timer > 1)
            {

                // NOTE if we wait for building construction to apply effects of buildings loaded from memory,
                // then we risk completing the missions before they even start
                if (!isInitialLoad)
                {
                    // Apply Proximity Effects
                    // Check for proximity boosts from new building on surrounding buildings
                    float lastPopupDelay = 0;
                    // first display from new building on surrounding buildings
                    lastPopupDelay += buildingProperties.ApplyProximityEffects();
                    // then show surrounding buildings on new building
                    buildingProperties.ApplyNeighborEffectsToSelf(lastPopupDelay);
                }

                Destroy(this.gameObject);
                buildingProperties.buildConstruction = null;
                buildingProperties.environment.SetActive(true);
                builded = true;
                finishBuildPS.transform.parent = null;
                finishBuildPS.Play();

                if (target.connectToRoad)
                    roadGenerator.ConnectBuildingToRoad(target.transform);
            }

            yield return new WaitForEndOfFrame();
        }
    }
}
