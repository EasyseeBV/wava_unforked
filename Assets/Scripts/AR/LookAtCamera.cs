using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    public Vector3 Offset;
    public bool TurnOffX;
    public Transform ObjectToLookAt;

    // Update is called once per frame
    void Update()
    {
        if (ObjectToLookAt == null)
            transform.LookAt(Camera.main.transform);
        else
            transform.LookAt(ObjectToLookAt);
        if (TurnOffX)
        {
            Vector3 axis = transform.eulerAngles;
            axis.x = 0;
            transform.eulerAngles = axis;
        }
        transform.eulerAngles += Offset;
    }
}
