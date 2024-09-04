using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class NewBehaviourScript : MonoBehaviour
{

    private ARTrackedImageManager _aRTrackedImageManager;

    private void Awake()
    {
        _aRTrackedImageManager = FindObjectOfType<ARTrackedImageManager>();
    }

    public void onEnable()
    {
        _aRTrackedImageManager.trackedImagesChanged += OnImageChanged;
    }

    public void onDisable()
    {
        _aRTrackedImageManager.trackedImagesChanged -= OnImageChanged;
    }

    public void OnImageChanged(ARTrackedImagesChangedEventArgs args)
    {
        foreach (var trackedImage in args.added)
            Debug.Log(trackedImage.name);

    }

}