using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScenarioManager : MonoBehaviour
{

    [SerializeField] private bool enableScenario;

    [SerializeField] private Scenario currentScenario;

    [SerializeField] private BoidGrouping group;


    // Start is called before the first frame update
    void Start()
    {

        if (enableScenario == true) {

            StartCoroutine(SpawnDrones());

        }

        //for (int i = 0; i < currentScenario.droneGroupings.Length; i++)
        //{

        //    for (int j = 0; j < currentScenario.droneGroupings[i].numDrones; j++)
        //    {

        //        //StartCoroutine(SpawnDrone(i, j));

        //        //Vector3 pos = DroneSpawnPoints._instance.GetDroneSpawnPoint(currentScenario.droneGroupings[i].spawnPointNum).position;

        //        //Instantiate(currentScenario.droneGroupings[i].dronePrefab, pos, Quaternion.identity);

        //    }

        //}


    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator SpawnDrones() {

        for (int i = 0; i < currentScenario.droneGroupings.Length; i++)
        {

            for (int j = 0; j < currentScenario.droneGroupings[i].numDrones; j++)
            {
                Transform spawnPoint = DroneSpawnPoints._instance.GetDroneSpawnPoint(currentScenario.droneGroupings[i].spawnPointNum);
                Vector3 pos = spawnPoint.position;

                GameObject drone = Instantiate(currentScenario.droneGroupings[i].dronePrefab, pos, Quaternion.identity);

                drone.transform.parent = spawnPoint;

                yield return new WaitForSeconds(currentScenario.droneGroupings[i].timeBetweenSpawningDrone);

                //Vector3 pos = DroneSpawnPoints._instance.GetDroneSpawnPoint(currentScenario.droneGroupings[i].spawnPointNum).position;

                //Instantiate(currentScenario.droneGroupings[i].dronePrefab, pos, Quaternion.identity);



            }

            yield return new WaitForSeconds(currentScenario.timeBetweenSpawningGrouping);

        }

    }
}
