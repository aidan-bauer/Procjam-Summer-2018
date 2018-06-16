using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {

    public Vector3 vel;
    public float moveSpeed = 5;
    public float jumpForce = 5f;
    public float fallMultiplier = 3f;
    private float jumpForceStorage = 0f; //storage variable for jumping
    Rigidbody rigid;

    float xMove = 0;
    bool jump;

	// Use this for initialization
	void Awake () {
        rigid = GetComponent<Rigidbody>();

        rigid.velocity = Vector3.zero;

        //jumpForceStorage = jumpForce;
	}
	
	// Update is called once per frame
	void Update () {
        rigid.AddForce(Vector3.right * Input.GetAxis("Horizontal") * moveSpeed);

        if (Input.GetKeyDown(KeyCode.W))
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, Vector3.down, out hit))
            {
                if (hit.collider.CompareTag("Environment"))
                {
                    rigid.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                    //rigid.velocity = Vector3.up * jumpForce;
                }
            }
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
