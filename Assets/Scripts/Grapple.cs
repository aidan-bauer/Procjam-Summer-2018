﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grapple : MonoBehaviour {

    public Transform holder;    //the player or NPC who's holding the grappling hook
    public float speed;
    private bool inAir = false;
    private HingeJoint hingeJoint;
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
        hingeJoint.connectedBody = null;
        Destroy(hingeJoint);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.CompareTag("Environment"))
        {
            if (inAir)
            {
                rigid.velocity = Vector3.zero;
                gameObject.AddComponent<HingeJoint>();
                hingeJoint.connectedBody = collision.rigidbody;
                inAir = false;
            }
        }
    }
}