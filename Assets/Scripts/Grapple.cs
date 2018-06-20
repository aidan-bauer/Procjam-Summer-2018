using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grapple : MonoBehaviour {

    public Transform holder;    //the player or NPC who's holding the grappling hook
    public float speed;
    public float airTime = 2f;  //how long the grapple can be in the air before it's destroyed
    private bool inAir = false;

    private Rigidbody rigid;
    private Rigidbody holderRigid;
    private HingeJoint grabHinge;
    private LineRenderer lineRend;
    [HideInInspector]public PlayerMovement playerMovement;

    IEnumerator selfDestruct;

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
        //launchAngle.Normalize();
        rigid.velocity = launchAngle * speed;
        inAir = true;
        selfDestruct = DestoryInTime(airTime);
        StartCoroutine(selfDestruct);
    }

    public void breakGrapple()
    {
        grabHinge.connectedBody = null;
        Destroy(grabHinge);
        holderRigid.isKinematic = false;  //allow player to move again
        playerMovement.canMove = true;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.CompareTag("Environment"))
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

    public IEnumerator DestoryInTime(float delay)
    {
        playerMovement.canMove = true;
        yield return new WaitForSeconds(delay);
        Destroy(this.gameObject);
    }

    public IEnumerator MoveTowardsHook()
    {
        //Debug.Log("moving");
        holderRigid = holder.GetComponent<Rigidbody>();
        holderRigid.isKinematic = true;
        playerMovement.canMove = false;
        Vector3 dist = holder.position - transform.position;
        //holderRigid.velocity = Vector3.ClampMagnitude(dist, 1) * 5f;

        while (dist.magnitude > 0.25f)
        {
            //Debug.Log(dist.magnitude);
            dist = holder.position - transform.position;
            holder.position = Vector3.MoveTowards(holder.position, transform.position, 1f);

            if (dist.magnitude < 0.6f)
            {
                //Debug.Log("reached hook");
                yield return null;
            }
            yield return new WaitForSeconds(0.05f);
        }

        //yield return null;
    }
}
