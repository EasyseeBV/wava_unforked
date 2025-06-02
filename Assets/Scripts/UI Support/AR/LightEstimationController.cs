using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.XR.ARFoundation;

public class LightEstimationController : MonoBehaviour
{
    [SerializeField] private ARCameraManager cameraManager;
    [SerializeField] private Light directionalLight;

    void OnEnable()
    {
        cameraManager.frameReceived += OnFrameReceived;
    }

    void OnDisable()
    {
        cameraManager.frameReceived -= OnFrameReceived;
    }

    void OnFrameReceived(ARCameraFrameEventArgs args)
    {
        // ARFoundation 4.x and above expose “main light” estimation:
        if (args.lightEstimation.mainLightDirection.HasValue)
        {
            Vector3 dir = args.lightEstimation.mainLightDirection.Value;
            directionalLight.transform.rotation = Quaternion.LookRotation(dir);
        }
        if (args.lightEstimation.mainLightIntensityLumens.HasValue)
        {
            // scale intensity so that shadow strength/contrast looks correct
            directionalLight.intensity = args.lightEstimation.mainLightIntensityLumens.Value / 1000f;
        }
        if (args.lightEstimation.mainLightColor.HasValue)
        {
            directionalLight.color = args.lightEstimation.mainLightColor.Value;
        }
        // Optionally also set ambient color from spherical harmonics, etc.
        if (args.lightEstimation.ambientSphericalHarmonics.HasValue)
        {
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Skybox;
            RenderSettings.ambientProbe = args.lightEstimation.ambientSphericalHarmonics.Value;
        }
    }
}
