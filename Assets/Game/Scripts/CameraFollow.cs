using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class CameraFollow : MonoBehaviour
{
    public static CameraFollow instance;

    public float smooth = 0.5f;

    public Vector3 baseCameraOffset;    //def distance between cam and target
    public Vector3 cameraOffset;        //def distance between cam and target

    [HideInInspector] public Transform objToFollow = null;

    private void Awake()
    {
        instance = this;
    }

    public void CalculateCamOffset()
    {
        if(objToFollow != null)
            baseCameraOffset = transform.position - objToFollow.position; cameraOffset = baseCameraOffset;
    }

    private void LateUpdate()
    {
        if (objToFollow != null)
        {
            Vector3 newPosition = objToFollow.position + cameraOffset;
            transform.position = Vector3.Slerp(transform.position, newPosition, smooth);
            transform.LookAt(objToFollow);
        }
    }
}
