using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour {

    public Transform target;
    public Vector3 offset = new Vector3(0, 0.5f, -10);

    [Range(0.05f, 0.5f)]
    public float smoothSpeed = 0.125f;

    public bool lookAtTarget = true;

    private void FixedUpdate()
    {
        Vector3 desiredPos = target.position + offset;
        Vector3 smoothedPos = Vector3.Lerp(transform.position, desiredPos, smoothSpeed);
        transform.position = smoothedPos;

        if (lookAtTarget)
            transform.LookAt(target);
    }
}
