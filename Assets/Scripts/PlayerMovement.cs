using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {

    public Vector3 vel;
    public float jumpForce = 5f;
    Rigidbody rigid;

	// Use this for initialization
	void Awake () {
        rigid = GetComponent<Rigidbody>();

        rigid.velocity = Vector3.zero;
	}
	
	// Update is called once per frame
	void Update () {
        vel += new Vector3(Input.GetAxis("Horizontal"), 0);

        /*if (Input.GetAxis("Horizontal") == 0)
        {
            rigid.velocity.y = 0;
        }*/

        if (Input.GetKeyDown(KeyCode.W))
        {
            rigid.AddForce(Vector3.up * jumpForce);
            vel.y += jumpForce;
        }

        vel = Vector3.ClampMagnitude(vel, 1);
        rigid.velocity = vel;
	}
}
