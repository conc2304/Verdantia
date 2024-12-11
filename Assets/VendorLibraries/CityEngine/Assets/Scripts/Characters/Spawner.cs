using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
Part of the "City Engine" Asset from the Unity Asset store (unchanged)
Handles the spawning of citizens and vehicles (including ambulances and industrial cars) in the city simulation. 
It maintains lists of spawn points and dynamically adds objects to the scene at regular intervals.
**/
public class Spawner : MonoBehaviour
{

    public GameObject[] citizensCarsToSpawn;
    public GameObject[] ambulanceСarsToSpawn;
    public GameObject[] industrialСarsToSpawn;
    public GameObject[] citizenToSpawn;
    public int carsCount, citizensCount;

    PathTarget[] pathTargets;
    GameObject[] spawns;

    public List<Transform> carsSpawnPoints = new List<Transform>();
    public List<Transform> citizensSpawnPoints = new List<Transform>();
    public static List<Transform> cars = new List<Transform>();
    public static List<Transform> citizens = new List<Transform>();

    int carPriority = 0;

    CameraController cameraController;

    private void Start()
    {
        cameraController = FindObjectOfType<CameraController>();
        cars.Clear();
        StartCoroutine(SpawnCars());
        StartCoroutine(SpawnCitizens());
    }

    public IEnumerator SpawnCitizens()
    {
        while (true)
        {
            if (citizensSpawnPoints.Count != 0)
            {
                while (citizens.Count < citizensCount)
                {
                    if (citizensSpawnPoints.Count != 0)
                    {
                        Transform currentPath = citizensSpawnPoints[UnityEngine.Random.Range(0, citizensSpawnPoints.Count)];
                        GameObject obj = Instantiate(citizenToSpawn[Random.Range(0, citizenToSpawn.Length)], currentPath.transform.position,
                            Quaternion.Euler(0, currentPath.transform.eulerAngles.y - 180, 0), cameraController.citizensParent);
                        citizens.Add(obj.transform);
                        obj.GetComponent<HumansNav>().currentPathTarget = currentPath.GetComponent<PathTarget>();
                    }
                    yield return new WaitForSeconds(UnityEngine.Random.Range(3.0f, 6.0f));
                }
            }

            yield return new WaitForSeconds(UnityEngine.Random.Range(2.0f, 6.0f));
        }
    }

    public IEnumerator SpawnCars()
    {
        while (true)
        {
            if (carsSpawnPoints.Count != 0)
            {
                while (cars.Count < carsCount)
                {
                    if (carsSpawnPoints.Count != 0)
                    {
                        Transform currentPath = carsSpawnPoints[UnityEngine.Random.Range(0, carsSpawnPoints.Count)];

                        GameObject objToSpawn = null;
                        if (currentPath.tag == "AmbulanceCarsSpawn")
                            objToSpawn = ambulanceСarsToSpawn[Random.Range(0, ambulanceСarsToSpawn.Length)];
                        if (currentPath.tag == "IndustrialCarsSpawn")
                            objToSpawn = industrialСarsToSpawn[Random.Range(0, industrialСarsToSpawn.Length)];
                        if (currentPath.tag == "Untagged")
                            objToSpawn = citizensCarsToSpawn[Random.Range(0, citizensCarsToSpawn.Length)];

                        GameObject obj = Instantiate(objToSpawn, currentPath.transform.position,
                            Quaternion.Euler(0, currentPath.transform.eulerAngles.y - 180, 0), cameraController.carsParent);
                        cars.Add(obj.transform);
                        obj.GetComponent<CarNav>().currentPathTarget = currentPath.GetComponent<PathTarget>();
                        carPriority += 1;
                        obj.GetComponent<CarNav>().priority = carPriority;
                    }
                    yield return new WaitForSeconds(UnityEngine.Random.Range(3.0f, 6.0f));
                }
            }

            yield return new WaitForSeconds(UnityEngine.Random.Range(2.0f, 6.0f));
        }

    }


}
