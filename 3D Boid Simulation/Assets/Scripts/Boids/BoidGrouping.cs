using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidGrouping : MonoBehaviour
{

    // Defines boid's and necessary weights and larger-scale things, like what their target is.
    // Boids will query their tied BoidLaws script for information regarding weights and targets

    /*
     * GENERAL NOTES:
     * 
     * The parent with the BoidLaws script attaches a Boid script to each child object if the BoidSpawning enum
     * is Existing_Children.
     * 
     * Another BoidSpawning enum is invoked upon creation by the ScenarioManager. A bool on the boid script, spawning,
     * notifies its parent's BoidLaws it belongs to.
     * 
     * This could be expanded to randomly generate Boid prefabs instead in the future
     * 
     * Currently, every boid law is applied to each boid. If a function is not desired, set its weight to 0.
     * 
     */

    // If you have a better name for this please pleeeeeeease suggest it to me
    enum BoidSpawning
    {
        Existing_Children, // Find the children objects to the BoidLaws GO
        Spawned_Children, // If drones are spawned from the Scenario and ScenarioManager
        Generated // Not supported currently
    }
    [SerializeField]
    BoidSpawning spawns = BoidSpawning.Spawned_Children;

    [SerializeField] [Range(-1f, 1f)] [Tooltip("Adjust how boids travel to the center of a local group")]
    private float coherencePercentage = 0.005f;

    #region Separation Params
    [SerializeField] [Range(-1f, 1f)] [Tooltip("Adjust how close boids are to each other")]
    private float separationPercentage = 0.05f;
    [Tooltip("The distance to stay away from other boids")]
    public float separationDistance = 0.05f;
    #endregion

    [SerializeField] [Range(-1f, 1f)] [Tooltip("Adjust how boids travel together")]
    private float alignmentPercentage = 0.05f;

    [Tooltip("How fast boids can go before having their velocity capped")]
    public float speedLimit = 5f;

    [Tooltip("What distance boids can see neighborring boids")]
    public float visualDistance = 2f;

    #region Avoidance Params
    public float sphereRad = 1f; // For spherecasting instead of raycasting
    public float avoidDist = 1f;
    [SerializeField] [Range(0f, 20f)] [Tooltip("How tightly to avoid other colliders")]
    private float avoidancePercentage = 0.5f;
    #endregion 

    [Range(0f, 5f)]
    public float containWeight = 1f;
    public float containDist = 15f;

    #region Targetting Params
    public GameObject target; // Used only for TrackTarget
    [SerializeField] [Range(0f, 0.5f)] [Tooltip("How favored boids will travel towards a target")]
    private float targetFactor = 0.0f;
    public float distFromTarget = 0f;
    #endregion

    public List<Boid> boidSet;

    private Dictionary<int, WeightContainer> weights = new Dictionary<int, WeightContainer>();

    // Start is called before the first frame update
    void Start()
    {
        //m_Collider = GetComponent<Collider>();
        if (spawns == BoidSpawning.Existing_Children)
            PopulateBoidSet();
        GenerateWeights();
        
    }

    private void PopulateBoidSet()
    {
        foreach (Transform child in transform)
        {
            if (child.gameObject != this.gameObject && child.gameObject != target)
            {
                Boid boid = child.GetComponent<Boid>();
                if (boid == null)
                    boid = child.gameObject.AddComponent<Boid>();
                boidSet.Add(boid);
            }
        }
    }

    private void GenerateWeights()
    {
        // Order of weights/percentages and lawId is as follows: 
        // Coherence, Separation, Alignment, Avoidance, Targetting, Contain
        // 0,         1,          2,         3,         4,          5

        // Enum in Boids for states are as follows:
        // Wander, Targetting, Avoidance, Custom
        // 0,       1,          2,          3

        WeightContainer wanderWeights = new WeightContainer(new float[] { 1f, 0.2f, 0.8f, 0.0f, 0f, 1f });
        weights.Add(0, wanderWeights);

        WeightContainer targettingWeights = new WeightContainer(new float[] { 2f, 0.8f, 1.5f, 0.2f, 0.5f, 0f });
        weights.Add(1, targettingWeights);

        WeightContainer avoidWeights = new WeightContainer(new float[] { -1f, 0.2f, 0.7f, 0.9f, 0f, 0.01f });
        weights.Add(2, avoidWeights);

        WeightContainer customWeights = new WeightContainer(new float[] { coherencePercentage, separationPercentage, alignmentPercentage, avoidancePercentage, targetFactor, containWeight });
        weights.Add(3, customWeights);
    }

    // Called by the Inspector GUI button to apply custom weights to the dictionary
    public void UpdateCustomWeights ()
    {
        WeightContainer customWeights = new WeightContainer(new float[] { coherencePercentage, separationPercentage, alignmentPercentage, avoidancePercentage, targetFactor, containWeight });
        weights[3] = customWeights;
    }

    public void AddBoid(Boid boid)
    {
        boidSet.Add(boid);
    }

    public void RemoveBoid(Boid boid)
    {
        boidSet.Remove(boid);
    }

    public float GetWeight(int boidState, int lawId)
    {
        return weights[boidState].Weights[lawId];
    }

    // Helper Classes

    // Contains the weights for each boid state. Should probably just be a direct float[], but I might change it later
    private class WeightContainer
    {
        // Order of weights/percentages is as follows: 
        // Coherence, Separation, Alignment, Avoidance, Contain, Targetting
        private float[] _weights = new float[6];
        public float[] Weights { 
            get { return _weights; } 
            set { _weights = value; }
        }

        public WeightContainer(float[] weights)
        {
            _weights = weights;
        }

        public override string ToString()
        {
            string output = "";
            foreach (float wei in _weights)
                output += wei + ", ";
            return output;
        }
    }

}

// "Source code"
/* 
 * I based general code structure and implementation of the 3 boid laws based on this, found at:
 * 
 * https://eater.net/boids
 * 
*/