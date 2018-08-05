using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public bool is2D = false;

    public bool canMove = true;
    public bool canJump;

    public KeyCode moveLeft = KeyCode.A;
    public KeyCode moveRight = KeyCode.D;
    public KeyCode jump = KeyCode.W;
    public LayerMask playerMask;

    public Vector3 vel;
    public float moveSpeed = 5;
    public float jumpForce = 5f;
    public float fallMultiplier = 3f;
    Rigidbody rigid;
    Rigidbody2D rigid2D;

    float xMove = 0;
    RaycastHit hit;

    // Use this for initialization
    void Awake () {
        if (!is2D)
        {
            rigid = GetComponent<Rigidbody>();
            rigid.velocity = Vector3.zero;
        }
        else
        {
            rigid2D = GetComponent<Rigidbody2D>();
            rigid2D.velocity = Vector3.zero;
        }
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (canMove)
        {
            if (!is2D)
                vel = rigid.velocity;
            else
                vel = rigid2D.velocity;

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

            if (!is2D)
            {
                if (Physics.Raycast(transform.position, Vector3.down, out hit, 1f))
                {
                    if (hit.collider.CompareTag("Environment"))
                    {
                        canJump = true;
                        if (Input.GetKeyDown(KeyCode.W))
                        {
                            vel.y = jumpForce;
                        }
                    }
                    else
                    {
                        canJump = false;
                    }
                }
                else
                {
                    canJump = false;
                }
            }
            else
            {
                RaycastHit2D hit2D = Physics2D.Raycast(transform.position, Vector3.down, 1.5f, 1 << playerMask);

                if (hit2D.collider)
                {
                    if (hit2D.collider.CompareTag("Environment"))
                    {
                        canJump = true;

                        if (Input.GetKeyDown(KeyCode.W))
                        {
                            vel.y = jumpForce;
                        }
                    }
                    else
                    {
                        canJump = false;
                    }
                }
                else
                {
                    canJump = false;
                }
            }

            if (!is2D)
                rigid.velocity = vel;
            else
                rigid2D.velocity = vel;
        }
	}

    //code to make the player fall faster
    private void FixedUpdate()
    {
        if (!is2D)
        {
            if (rigid.velocity.y < 0)
            {
                rigid.velocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * Time.deltaTime; //-1 to account for physics system's normal gravity
                
            }
        }
        else
        {
            if (rigid2D.velocity.y < 0)
            {
                //rigidTwo.velocity += Vector2.up * Physics.gravity.y * (fallMultiplier - 1) * Time.deltaTime; //-1 to account for physics system's normal gravity
                rigid2D.gravityScale = fallMultiplier;
            } else
            {
                rigid2D.gravityScale = 1;
            }
        }
    }
}
