using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Boid_RB : Boid
{

    private Rigidbody rb;

    public bool seeDir = false;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        rb = GetComponent<Rigidbody>();
        if (rb == null)
            rb = this.gameObject.AddComponent<Rigidbody>();
    }

    public override void MoveByVelocity()
    {
        rb.AddForce(velocity, ForceMode.Force);
        this.transform.forward = velocity;
        if (seeDir)
            Debug.DrawRay(transform.position, velocity, Color.cyan);
    }

}
