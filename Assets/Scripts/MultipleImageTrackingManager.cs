using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.ARFoundation;

/// <summary>
///   <para>This class controls the AR Tracking for multiple reference images. </para>
/// </summary>
//[RequireComponent(typeof(ARTrackedImageManager))]
public class MultipleImageTrackingManager : MonoBehaviour
{
    [SerializeField] AudioController audioController;

    [SerializeField] ARTrackedImageManager m_TrackedImageManager;

    [SerializeField] ARSession m_ArSession;

    //Maintain a list of images currently being tracked
    [SerializeField] public List<ARTrackedImage> trackedImagesList;

    private List<bool> trackedImagesFound;


    public void Awake()
    {
        //m_TrackedImageManager = GetComponent<ARTrackedImageManager>();
        trackedImagesList = new List<ARTrackedImage>();
        m_ArSession.Reset();
    }

    void OnEnable()
    {
        m_TrackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    void OnDisable()
    {
        m_TrackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        foreach (var addedImage in eventArgs.added)
        {
            TrackedImageFound(addedImage);
        }

        foreach (var updatedImage in eventArgs.updated)
        {
            if (updatedImage.trackingState == TrackingState.None)
                TrackedImageLost(updatedImage);
        }

        foreach (var removedImage in eventArgs.removed)
        {
            TrackedImageLost(removedImage);
        }
    }

    //This function is called every time a new image is found
    public void TrackedImageFound(ARTrackedImage trackedImage)
    {
        trackedImagesList.Add(trackedImage);
        Debug.Log($"Detected image {trackedImage.name} added");
        SelectDesiredOutcome(trackedImage);
    }

    //This function is called every time an existing image is lost
    public void TrackedImageLost(ARTrackedImage trackedImage)
    {
        trackedImagesList.Remove(trackedImage);
        Debug.Log($"Detected image {trackedImage.name} removed");
    }

    //This is a generic function used to render AR output 
    //This function is not necessary if the output is directly decided by the function SelectDesiredOutcome().
    //In the current setup, one function decides what to do based on which image has been detected
    //While the RenderObject function is a generic function that applies to all use cases
    void RenderObject()
    {
        //Use the AR Points fields to do the rendering, or add special logic.
    }


    /// <summary>
    ///   <para>This is a generic function to check the reference image name and then select a corresponding action to that. </para>
    /// </summary>
    public void SelectDesiredOutcome(ARTrackedImage trackedImage)
    {
        audioController.PlayByName(trackedImage.referenceImage.name);
    }





    void Update()
    {
        //Useful to have an update call if there is some rendering specific behaviour that we want updated every frame
        if (trackedImagesList.Count > 0)
        {
            RenderObject();
        }
    }

}