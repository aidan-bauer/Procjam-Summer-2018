using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grapple : MonoBehaviour {

    public Transform holder;    //the player or NPC who's holding the grappling hook
    public float speed;
    private bool inAir = false;
    private HingeJoint grabHinge;
    private Rigidbody rigid;

	// Use this for initialization
	void Awake () {
        //hingeJoint = GetComponent<HingeJoint>();
        rigid = GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		
	}

    void launchGrapple(Vector3 launchAngle)
    {
        rigid.velocity = launchAngle * speed;
        inAir = true;
    }

    void unhook()
    {
        grabHinge.connectedBody = null;
        Destroy(grabHinge);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.CompareTag("Environment"))
        {
            if (inAir)
            {
                inAir = false;
                rigid.velocity = Vector3.zero;
                grabHinge = gameObject.AddComponent<HingeJoint>();
                grabHinge.connectedBody = collision.rigidbody;

                //code for moving player towards grapple
            }
        }
    }
}
