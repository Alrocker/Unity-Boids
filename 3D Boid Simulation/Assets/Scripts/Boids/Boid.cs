using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boid : Combatant_ABS
{

    // Boid updates its own position via the BoidLaws update loop
    // Each boid's velocity is calculated and applied via BoidLaws

    public enum BoidState
    {
        Wander,
        Targetting,
        Avoidance,
        Custom
    }
    [SerializeField]
    public BoidState state = BoidState.Custom; // Weights determined by BoidGrouping dictionary

    private Renderer rend;

    private BoidGrouping group;
    public bool spawned = false;

    public Vector3 velocity = Vector3.zero;

    public bool useInitVel = false;
    public Vector3 initVel = Vector3.zero;
    public float randStart = 1.5f;

    // Start is called before the first frame update
    protected virtual void Start()
    {
        if (!useInitVel)
            initVel = new Vector3(UnityEngine.Random.Range(-randStart, randStart),
                UnityEngine.Random.Range(-randStart, randStart),
                UnityEngine.Random.Range(-randStart, randStart));
        velocity = initVel;
        try
        {
            group = transform.parent.GetComponent<BoidGrouping>();
        }
        catch
        {
            Debug.LogError("Boid grouping of " + this + " not found on object's parent.");
        }
        if (spawned && group != null) // If spawned, check if the boid has a parent BoidLaws object
            group.AddBoid(this);
        RegisterCombatant();

        rend = GetComponent<Renderer>();
        if (rend == null)
            rend = this.gameObject.AddComponent<MeshRenderer>();
    }

    private void FixedUpdate()
    {
        NeighborContainer neighbors = FindNeighbors(); // Local helper class after FindNeighbors
        EnactBoidLaws(neighbors);
    }

    private NeighborContainer FindNeighbors()
    {
        List<Boid> neighbors = new List<Boid>(); // Max size is the boidSet-1, as boidInput is excluded
        Vector3 posSum = Vector3.zero;
        Vector3 velSum = Vector3.zero;
        float visualDist = group.visualDistance;

        List<Combatant_ABS> combatants = BattleManager._instance.GetCombatants();
        foreach (var comb in combatants)
        {
            // Ignore yourself in the list
            if (comb != this) {
                // Search for just other boids for the time being
                if (IsSameOrSubclass(comb.GetType(), typeof(Boid)) & 
                    CalculateDistance(comb.transform, this.transform, visualDist))
                {
                    Boid otherBoid = (Boid) comb;
                    Faction otherFac = otherBoid.GetFaction();

                    // If you want to detect Factions
                    if (otherFac == this.faction)
                    {
                        neighbors.Add(otherBoid);
                        posSum += otherBoid.transform.position;
                        velSum += otherBoid.velocity;
                    }
                    else if (otherFac == Faction.Enemy & state != BoidState.Targetting)
                    {
                        rend.material.color = new Color(1f, 1.0f, 0.0f, 0.4f);
                        state = BoidState.Avoidance;
                    }

                    if (otherFac == this.faction & otherBoid.state == BoidState.Avoidance)
                    {
                        rend.material.color = new Color(0f, 1.0f, 0.0f, 0.4f);
                        state = BoidState.Targetting;
                    }
                    

                    //General functionality
                    /*
                    neighbors.Add(otherBoid);
                    posSum += otherBoid.transform.position;
                    velSum += otherBoid.velocity;
                    */
                }
            }
        }

        NeighborContainer neighborClass = new NeighborContainer(neighbors, posSum, velSum);
        return neighborClass;

        /*
         * 
        Using Colliders
                Collider[] hitColliders = Physics.OverlapSphere(this.transform.position, visualDist);
                foreach (var hitCollider in hitColliders)
                {
                    // Search for just other boids for the time being
                    if (combatants.Contains(hitCollider.gameObject) & hitCollider.gameObject != this.gameObject) {
                        Do stuff
                    }
                }


        Using BoidGroupings' list
        foreach (Boid boid in groupList)
        {
            if (CalculateDistance(this, boid, visualDist) & boid != this)
            {
                neighbors.Add(boid);
                posSum += boid.transform.position;
                velSum += boid.velocity;
            }
        }
        */

    }

    // Must check for type Boid or if it inherits from Boid, like Boid_RB
    private bool IsSameOrSubclass(Type potentialBase, Type potentialDescendant)
    {
        return potentialDescendant.IsAssignableFrom(potentialBase)
               || potentialDescendant == potentialBase;
    }

    // Various methods to calculate distance with different inputs; only inputs of Boids are currently in use
    #region Calculate Distance methods
    private bool CalculateDistance(GameObject obj1, GameObject obj2, float visualDistance)
    {
        return Vector3.Distance(obj1.transform.position, obj2.transform.position) < visualDistance;
    }

    private bool CalculateDistance(Boid boid1, Boid boid2, float visualDistance)
    {
        return Vector3.Distance(boid1.gameObject.transform.position, boid2.gameObject.transform.position) < visualDistance;
    }

    private bool CalculateDistance(Transform trans1, Transform trans2, float visualDistance)
    {
        return Vector3.Distance(trans1.position, trans2.position) < visualDistance;
    }

    private bool CalculateDistance(Vector3 pos1, Vector3 pos2, float visualDistance)
    {
        return Vector3.Distance(pos1, pos2) < visualDistance;
    }
    #endregion

    public virtual void MoveByVelocity()
    {
        this.transform.position += velocity * Time.deltaTime;
        this.transform.forward = velocity;
        // Debug.DrawRay(transform.position, velocity, Color.cyan);
    }

    public BoidState GetState()
    {
        return state;
    }

    private void EnactBoidLaws(NeighborContainer neighbors) // NeighborContainer is defined in BoidLaws
    {
        if (neighbors.NeighborCount > 0) // The following group only function if the neighbor list is populated
        {
            // Primary group
            Coherence(neighbors);
            Separation(neighbors);
            Alignment(neighbors);
        }

        AvoidObjects(); // Pathfind around other colliders

        TrackTarget(group.target);

        // Final group
        ContainBoids(); // Defined within a set area
        LimitSpeed(); // Limit speed

        // Apply calculated velocity
        MoveByVelocity();
    }

    // A backup error handling if no neighbors exist for a method requiring neighbors
    // Returns true if neighbors exist
    private bool Error_NoNeighbors(NeighborContainer neighbors)
    {
        if (neighbors.NeighborCount < 1) // Error checking; checks length in EnactBoidLaws as well
        {
            Debug.LogWarning(this + " has no neighbors.");
            return false;
        }
        return true;
    }

    // Find the center of mass of the other boids and adjust velocity slightly to
    // point towards the center of mass.
    void Coherence(NeighborContainer neighbors)
    {
        if (!Error_NoNeighbors(neighbors)) return; // If no neighbors exist, exit the method
        float cohWeight = group.GetWeight((int)state, 0);
        if (cohWeight == 0f) return;

        velocity += (neighbors.PositionSum / neighbors.NeighborCount - transform.position) * cohWeight;
    }

    // Move away from other boids that are too close to avoid colliding
    void Separation(NeighborContainer neighbors)
    {
        if (!Error_NoNeighbors(neighbors)) return; // If no neighbors exist, exit the method
        float sepWeight = group.GetWeight((int)state, 1);
        if (sepWeight == 0f) return;

        velocity += (transform.position * neighbors.NeighborCount - neighbors.PositionSum) * sepWeight;

    }

    // Find the average velocity (speed and direction) of the other boids and
    // adjust velocity slightly to match.
    void Alignment(NeighborContainer neighbors)
    {
        if (!Error_NoNeighbors(neighbors)) return; // If no neighbors exist, exit the method
        float alignWeight = group.GetWeight((int)state, 2);
        if (alignWeight == 0f) return;

        // Calculate the center of all other boids; assuming the center of space is 0
        velocity += (neighbors.VelocitySum / neighbors.NeighborCount) * alignWeight;
    }

    void AvoidObjects()
    {
        float avoidWeight = group.GetWeight((int)state, 3);
        if (avoidWeight == 0f) return; // If weight is 0, don't bother making the raycast call

        //velocity += AvoidRayCast(group.avoidDist) * avoidWeight;
        velocity += AvoidSphereCast(group.avoidDist, group.sphereRad) * avoidWeight;
    }
    protected virtual Vector3 AvoidRayCast(float dist)
    {
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        // Bit shift the index of the layer (13), which Boids will not crash into
        int layerMask = 1 << 13;
        // Avoid every layer except layer 13
        // layerMask = ~layerMask;

        Debug.DrawRay(transform.position, forward * dist, Color.blue);
        RaycastHit hit;
        if (Physics.Raycast(transform.position, forward, out hit, dist, layerMask))
        {
            //Debug.Log("Avoiding target");
            Debug.DrawRay(transform.position, -forward, Color.red);
            return -forward; // If a collision occurs, travel in the opposite direction
        }
        return Vector3.zero; // No collision: continue course
    }

    // Spherecast no longer in use
    public virtual Vector3 AvoidSphereCast(float dist, float radius)
    {
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        // Bit shift the index of the layer (13), which Boids will not crash into
        int layerMask = 1 << 13;
        // Avoid every layer except layer 13
        // layerMask = ~layerMask;

        // This would cast rays only against colliders in layer 8.
        // But instead we want to collide against everything except layer 8. The ~ operator does this, it inverts a bitmask.
        //layerMask = ~layerMask;

        Vector3 dir = Vector3.zero;
        Vector3 pos = transform.position;

        RaycastHit[] hits = Physics.SphereCastAll(pos, radius, forward, dist, layerMask);
        if (hits.Length < 1) return dir; // No collisions: dir is Vector3.zero, so exit

        foreach (RaycastHit hit in hits)
            dir += hit.point;

        dir /= hits.Length;

        Debug.DrawRay(hits[0].point, pos * hits[0].distance, Color.green);
        Debug.DrawRay(pos, dir * dist, Color.red);
        Vector3 refDir = Vector3.Reflect(dir, forward);
        Debug.DrawRay(pos, refDir, Color.blue);

        return refDir;
    }

    void TrackTarget(GameObject target)
    {
        if (target == null) return; // If there is no set target, exit

        float trackWeight = group.GetWeight((int)state, 4);
        if (trackWeight == 0f) return;

        Vector3 targetDir = target.transform.position - transform.position;
        velocity += targetDir * trackWeight;

        /*
        float targDist = Vector3.Distance(target.transform.position, boidInput.transform.position); // Trying to keep boids at a set distance from the target
        if (targDist < distFromTarget + 1f)
            targetDir *= -1f;
        */
        //boidInput.velocity += targetDir * targetFactor;
        //Debug.Log(weights[4].Weights[4]);
    }

    void ContainBoids()
    {
        float containWeight = group.GetWeight((int)state, 5);
        if (containWeight == 0f) return;

        Vector3 inputPos = transform.position;
        Vector3 centerDir = group.transform.position - inputPos; // Contains by the center of the BoidLaws

        // boidInput.velocity += centerDir / containDist * Mathf.Pow(containWeight, 1.5f);
        velocity += centerDir / group.containDist * containWeight;

        //if (!m_Collider.bounds.Contains(inputPos)) // If the boid is not in the parent collider zone, reverse its direction and increase magnitude
        //    boidInput.velocity *= -1.5f;
    }

    void LimitSpeed()
    {
        float speed = Mathf.Sqrt(velocity.x * velocity.x + velocity.y * velocity.y + velocity.z * velocity.z);
        float speedLimit = group.speedLimit;
        if (speed > speedLimit)
            velocity = (velocity / speed) * speedLimit;
    }

    private void OnDestroy()
    {
        if (group != null)
            group.RemoveBoid(this);
        UnregisterCombatant();
    }

    // A local class that provides commonly used neighbor boid information, such as the sum of their positions and velocities.
    // All neighborring boids are also provided if advanced funcitionality is needed
    private class NeighborContainer
    {

        private List<Boid> _neighborList;
        public List<Boid> NeighborList
        { get { return _neighborList; } }

        private int _neighborCount;
        public int NeighborCount
        { get { return _neighborCount; } }

        private Vector3 _positionSum;
        public Vector3 PositionSum
        { get { return _positionSum; } }

        private Vector3 _velocitySum;
        public Vector3 VelocitySum
        { get { return _velocitySum; } }

        public NeighborContainer(List<Boid> neighborList, Vector3 positionSum, Vector3 velocitySum)
        {
            this._neighborList = neighborList;
            this._neighborCount = neighborList.Count;
            this._positionSum = positionSum;
            this._velocitySum = velocitySum;
        }
    }
}
