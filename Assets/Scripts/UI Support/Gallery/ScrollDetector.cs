using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// This class could be improved by deriving from scrollrect instead and handling the scroll threshold on the ondrag method
public class ScrollDetector : MonoBehaviour
{
    [SerializeField] private ScrollRect scrollRect;
    
    [Range(0f, 1f)]
    [SerializeField] private float scrollThreshold = 0.8f;

    private bool paused = false;

    private void OnEnable()
    {
        ArtworkUIManager.OnNewDocumentAdded += Unpause;
    }

    private void OnDisable()
    {
        ArtworkUIManager.OnNewDocumentAdded -= Unpause;
    }

    private void Update()
    {
        if (scrollRect == null || paused)
            return;

        // For vertical scrolling, use verticalNormalizedPosition
        float currentScroll = scrollRect.verticalNormalizedPosition;
        float adjustedThreshold = 1f - scrollThreshold;
        
        if (currentScroll <= adjustedThreshold)
        {
            Debug.Log("Fetch new document from firestore here.");
            paused = true;
            ArtworkUIManager.Instance.AddNewDocument();
        }
    }

    private void Unpause()
    {
        paused = false;
    }
}
