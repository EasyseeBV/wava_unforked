using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class AROcclusionSetter : MonoBehaviour
{
    private AROcclusionManager occlusionManager;
    
    private void Awake()
    {
        occlusionManager = FindObjectOfType<AROcclusionManager>();
    }
    
    private IEnumerator Start()
    {
        // wait until subsystem descriptor is available
        yield return new WaitUntil(() => occlusionManager.descriptor != null);

        ConfigureOcclusion();
    }

    private void ConfigureOcclusion()
    {
        // Depth
        if (occlusionManager.descriptor.humanSegmentationDepthImageSupported == Supported.Supported)
        {
            occlusionManager.requestedHumanDepthMode = HumanSegmentationDepthMode.Fastest;
            Debug.Log("Enabled human depth segmentation.");
        }
        else
        {
            occlusionManager.requestedHumanDepthMode = HumanSegmentationDepthMode.Disabled;
            Debug.Log("Human depth segmentation not supported.");
        }

        // Stencil
        if (occlusionManager.descriptor.humanSegmentationStencilImageSupported == Supported.Supported)
        {
            occlusionManager.requestedHumanStencilMode = HumanSegmentationStencilMode.Fastest;
            Debug.Log("Enabled human stencil segmentation.");
        }
        else
        {
            occlusionManager.requestedHumanStencilMode = HumanSegmentationStencilMode.Disabled;
            Debug.Log("Human stencil segmentation not supported.");
        }
    }
}
