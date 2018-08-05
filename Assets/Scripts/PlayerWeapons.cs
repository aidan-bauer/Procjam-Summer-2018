using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeapons : MonoBehaviour {

    public bool is2D = false;

    public Grapple grapple;
    public KeyCode breakKey;

    private Grapple grappleInst;
    private PlayerMovement playerMovement;

    private Vector3 mousePos, dir;

    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
    }

    void Update()
    {
        //grapple code
        if (Input.GetMouseButtonDown(1))
        {
            //only allow one grappling hook to exist at once
            if (grappleInst)
            {
                grappleInst.breakGrapple();
                Destroy(grappleInst.gameObject);
            }

            if (!is2D) {
                //Debug.Log(Input.mousePosition + ", " + GetWorldPositionOnPlane(Input.mousePosition, 0));
                mousePos = GetWorldPositionOnPlane(Input.mousePosition, 0);
                dir = mousePos - transform.position;
                dir.z = 0; //zero out depth just to make sure
            } else
            {
                mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                dir = mousePos - transform.position;
                dir.z = 0; //zero out depth just to make sure
            }
            

            grappleInst = Instantiate(grapple, transform.position, Quaternion.identity) as Grapple;
            grappleInst.holder = transform;
            grappleInst.playerMovement = playerMovement;
            grappleInst.launchGrapple(Vector3.ClampMagnitude(dir, 1));
        }

        //allow player to break grapple at any time
        if (grappleInst && (Input.GetKeyDown(breakKey) || Input.GetKeyDown(playerMovement.jump) 
            || Input.GetKeyDown(playerMovement.moveLeft) || Input.GetKeyDown(playerMovement.moveRight)))
        {
            grappleInst.breakGrapple();
            Destroy(grappleInst.gameObject);
        }
    }

    //get mouse position in a perspective camera
    public Vector3 GetWorldPositionOnPlane(Vector3 screenPosition, float z)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPosition);
        Plane xy = new Plane(Vector3.forward, new Vector3(0, 0, z));
        float distance;
        xy.Raycast(ray, out distance);
        return ray.GetPoint(distance);
    }
}
