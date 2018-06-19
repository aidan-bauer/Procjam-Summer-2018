using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grapple : MonoBehaviour {

    public Transform holder;    //the player or NPC who's holding the grappling hook
    public float speed;
    private bool inAir = false;
    private Rigidbody rigid;
    private HingeJoint grabHinge;
    private LineRenderer lineRend;

	// Use this for initialization
	void Awake () {
        rigid = GetComponent<Rigidbody>();
        lineRend = GetComponent<LineRenderer>();
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        lineRend.SetPosition(0, transform.position);
        lineRend.SetPosition(lineRend.positionCount - 1, holder.position);
    }

    public void launchGrapple(Vector3 launchAngle)
    {
        rigid.velocity = launchAngle * speed;
        inAir = true;
    }

    public void breakGrapple()
    {
        grabHinge.connectedBody = null;
        Destroy(grabHinge);
    }

    void OnCollisionEnter(Collision collision)
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
