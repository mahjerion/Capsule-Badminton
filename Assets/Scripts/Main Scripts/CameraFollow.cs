using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;

    public float smoothSpeed = 10f;
    public Vector3 offset;
    public Vector3 regular;

    void LateUpdate()
    {
        if (SinglePlayerCamera.altCamera == false && !Points.doublesOn)
        {
            Vector3 desiredPosition = target.position + offset;
            //Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
            transform.position = smoothedPosition;
            //transform.LookAt(target);
        }
    }
}
