using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public bool isTwoDimensional = false;

    public bool canMove = true;
    public bool canJump;

    public KeyCode moveLeft = KeyCode.A;
    public KeyCode moveRight = KeyCode.D;
    public KeyCode jump = KeyCode.W;

    public Vector3 vel;
    public float moveSpeed = 5;
    public float jumpForce = 5f;
    public float fallMultiplier = 3f;
    Rigidbody rigid;
    Rigidbody2D rigidTwo;

    float xMove = 0;
    RaycastHit hit;

    // Use this for initialization
    void Awake () {
        if (!isTwoDimensional)
        {
            rigid = GetComponent<Rigidbody>();
            rigid.velocity = Vector3.zero;
        }
        else
        {
            rigidTwo = GetComponent<Rigidbody2D>();
            rigidTwo.velocity = Vector3.zero;
        }
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (canMove)
        {
            if (!isTwoDimensional)
                vel = rigid.velocity;
            else
                vel = rigidTwo.velocity;

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

            if (Physics.Raycast(transform.position, Vector3.down, out hit, 1f))
            {
                if (hit.collider.CompareTag("Environment"))
                {
                    canJump = true;
                    if (Input.GetKeyDown(KeyCode.W))
                    {
                        vel.y = jumpForce;
                    }
                } else
                {
                    canJump = false;
                }
            }
            else
            {
                canJump = false;
            }

            if (!isTwoDimensional)
                rigid.velocity = vel;
            else
                rigidTwo.velocity = vel;
        }
	}

    //code to make the player fall faster
    private void FixedUpdate()
    {
        if (!isTwoDimensional)
        {
            if (rigid.velocity.y < 0)
            {
                rigid.velocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * Time.deltaTime; //-1 to account for physics system's normal gravity
            }
        }
        else
        {
            if (rigidTwo.velocity.y < 0)
            {
                rigidTwo.velocity += Vector2.up * Physics.gravity.y * (fallMultiplier - 1) * Time.deltaTime; //-1 to account for physics system's normal gravity
            }
        }
    }
}
