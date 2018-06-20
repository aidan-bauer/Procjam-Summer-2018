using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {

    public bool canMove = true;

    public KeyCode moveLeft = KeyCode.A;
    public KeyCode moveRight = KeyCode.D;
    public KeyCode jump = KeyCode.W;

    public Vector3 vel;
    public float moveSpeed = 5;
    public float jumpForce = 5f;
    public float fallMultiplier = 3f;
    Rigidbody rigid;

    float xMove = 0;
    RaycastHit hit;

    // Use this for initialization
    void Awake () {
        rigid = GetComponent<Rigidbody>();

        rigid.velocity = Vector3.zero;
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (canMove)
        {
            vel = rigid.velocity;

            if (Input.GetKey(moveRight))
            {
                xMove = 1;
            }
            else if (Input.GetKey(moveLeft))
            {
                xMove = -1;
            }
            else
            {
                xMove = 0;
            }

            vel.x = xMove * moveSpeed;

            if (Physics.Raycast(transform.position, Vector3.down, out hit))
            {
                if (hit.collider.CompareTag("Environment"))
                {

                    if (Input.GetKeyDown(KeyCode.W))
                    {
                        vel.y = jumpForce;
                    }
                }
            }

            rigid.velocity = vel;
        }
	}

    //code to make the player fall faster
    private void FixedUpdate()
    {
        if (rigid.velocity.y < 0)
        {
            rigid.velocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * Time.deltaTime; //-1 to account for physics system's normal gravity
        }
    }
}
