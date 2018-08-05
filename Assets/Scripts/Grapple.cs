using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grapple : MonoBehaviour {

    public bool is2D = false;

    public Transform holder;    //the player or NPC who's holding the grappling hook
    public float speed;
    public float airTime = 2f;  //how long the grapple can be in the air before it's destroyed
    private bool inAir = false;

    private Rigidbody rigid;
    private Rigidbody2D rigid2D;
    private Rigidbody holderRigid;
    private Rigidbody2D holderRigid2D;
    private HingeJoint grabHinge;
    private HingeJoint2D grabHinge2D;
    private LineRenderer lineRend;
    [HideInInspector]public PlayerMovement playerMovement;

    IEnumerator selfDestruct;

    // Use this for initialization
    void Awake () {
        rigid = GetComponent<Rigidbody>();
        rigid2D = GetComponent<Rigidbody2D>();
        lineRend = GetComponent<LineRenderer>();
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        lineRend.SetPosition(0, transform.position);
        lineRend.SetPosition(lineRend.positionCount - 1, holder.position);
    }

    public void launchGrapple(Vector3 launchAngle)
    {
        //launchAngle.Normalize();
        if (!is2D)
            rigid.velocity = launchAngle * speed;
        else
            rigid2D.velocity = launchAngle * speed;

        inAir = true;
        selfDestruct = DestoryInTime(airTime);
        StartCoroutine(selfDestruct);
    }

    public void breakGrapple()
    {
        if (!is2D)
        {
            grabHinge.connectedBody = null;
            Destroy(grabHinge);
            holderRigid.isKinematic = false;  //allow player to move again
        } else
        {
            grabHinge2D.connectedBody = null;
            Destroy(grabHinge2D);
            //holderRigid2D.isKinematic = false;  //allow player to move again
            holderRigid2D.bodyType = RigidbodyType2D.Dynamic;
        }
        
        playerMovement.canMove = true;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Environment"))
        {
            if (inAir)
            {
                StopCoroutine(selfDestruct);
                StartCoroutine("MoveTowardsHook");
                rigid.velocity = Vector3.zero;
                grabHinge = gameObject.AddComponent<HingeJoint>();
                grabHinge.connectedBody = collision.rigidbody;

                inAir = false;
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Environment"))
        {
            if (inAir)
            {
                StopCoroutine(selfDestruct);
                StartCoroutine("MoveTowardsHook");
                rigid2D.velocity = Vector3.zero;
                grabHinge2D = gameObject.AddComponent<HingeJoint2D>();
                grabHinge2D.connectedBody = collision.rigidbody;

                inAir = false;
            }
        }
    }

    public IEnumerator DestoryInTime(float delay)
    {
        playerMovement.canMove = true;
        yield return new WaitForSeconds(delay);
        Destroy(this.gameObject);
    }

    public IEnumerator MoveTowardsHook()
    {
        //Debug.Log("moving");
        if (!is2D)
        {
            holderRigid = holder.GetComponent<Rigidbody>();
            holderRigid.isKinematic = true;
        } else
        {
            holderRigid2D = holder.GetComponent<Rigidbody2D>();
            //holderRigid2D.isKinematic = true;
            holderRigid2D.bodyType = RigidbodyType2D.Static;
        }

        playerMovement.canMove = false;
        Vector3 dist = holder.position - transform.position;

        while (dist.magnitude > 0.25f)
        {
            //Debug.Log(dist.magnitude);
            if (dist.magnitude < 1.0f)
            {
                //Debug.Log("reached hook");
                yield return null;
            }

            dist = holder.position - transform.position;
            holder.position = Vector3.MoveTowards(holder.position, transform.position, 0.8f);
            
            yield return new WaitForSeconds(Time.deltaTime / 2.0f);
        }
    }
}
