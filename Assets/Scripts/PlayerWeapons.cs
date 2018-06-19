using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeapons : MonoBehaviour {

    public Grapple grapple;
    public KeyCode breakKey;

    private Grapple grappleInst;

    private void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            //only allow one grappling hook to exist at once
            if (grappleInst)
            {
                grappleInst.breakGrapple();
                Destroy(grappleInst.gameObject);
            }

            Debug.Log(Input.mousePosition + ", " + Camera.main.ScreenToWorldPoint(Input.mousePosition));
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;
            Vector3 dir = mousePos - transform.position;
            dir.z = 0;
            grappleInst = Instantiate(grapple, transform.position, Quaternion.identity) as Grapple;
            grappleInst.holder = transform;
            grappleInst.launchGrapple(Vector3.ClampMagnitude(dir, 1));

            /*TODO: how to do accurate mousePos difference in perspective camera
             
             Ray ray = Camera.main.ScreenPointToRay(screenPosition);
             Plane xy = new Plane(Vector3.forward, new Vector3(0, 0, z));
             float distance;
             xy.Raycast(ray, out distance);
             return ray.GetPoint(distance);*/
        }

        if (grappleInst && Input.GetKeyDown(breakKey))
        {
            grappleInst.breakGrapple();
            Destroy(grappleInst.gameObject);
        }
    }
}
