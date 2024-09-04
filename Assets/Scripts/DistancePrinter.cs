using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class DistancePrinter : MonoBehaviour
{
    AROcclusionManager manager;
    // Start is called before the first frame update
    void Start()
    {
        manager = FindObjectOfType<AROcclusionManager>();
    }

    // Update is called once per frame
    void Update()
    {
        print(Vector3.Distance(transform.position, manager.transform.position));
    }
}
