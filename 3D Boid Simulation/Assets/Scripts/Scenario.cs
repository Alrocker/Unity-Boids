using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Scenario", menuName = "Scenarios/Scenario")]
public class Scenario : ScriptableObject
{

    [System.Serializable]
    public struct DroneGrouping {

        public GameObject dronePrefab;

        public int numDrones;

        public int spawnPointNum;

        public float timeBetweenSpawningDrone;

    }

    public DroneGrouping[] droneGroupings;

    public float timeBetweenSpawningGrouping;

}
