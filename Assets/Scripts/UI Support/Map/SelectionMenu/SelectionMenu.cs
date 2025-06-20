using System;
using System.Collections;
using System.Collections.Generic;
using Messy.Definitions;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SelectionMenu : MonoBehaviour
{
    public static SelectionMenu Instance;
    
    [Header("Dependencies")]
    [SerializeField] private MoveMapToArtwork mapMover;

    [Header("References")]
    [SerializeField] private RectTransform viewport;
    [SerializeField] private RectTransform content;
    [Space]
    [SerializeField] private GameObject headerLabel;
    [SerializeField] private GameObject dividerBar;
    [Space]
    [SerializeField] private ArtworkNavigationCard artworkNavigationCardPrefab;

    [Header("Settings")]
    [SerializeField] private float expandedViewportSize = 243f;
    [SerializeField] private float collapsedViewportSize = 96f;
    [SerializeField] private float expandedContentSize = 140f;
    [SerializeField] private float collapsedContentSize = 44f;

    public HotspotManager SelectedHotspot { get; private set; }
    
    private bool showingMinimal = true;
    
    private List<ArtworkNavigationCard> cachedArtworkNavigationCards = new List<ArtworkNavigationCard>();
    
    private void Awake()
    {
        if (!Instance) Instance = this;
        
        headerLabel.SetActive(false);
        dividerBar.SetActive(false);
    }

    public void Add(ArtworkData artwork)
    {
        Debug.Log("Adding: " + artwork.title);
        var card =  Instantiate(artworkNavigationCardPrefab, content);
        card.Populate(artwork);
        cachedArtworkNavigationCards.Add(card);
    }
    
    public void SelectHotspot(HotspotManager hotspot, bool inRange)
    {
        SelectedHotspot = hotspot;
        SelectedHotspot.BorderRingMesh.enabled = true;
        SelectedHotspot.selected = true;
    }

    public void DeselectHotspot()
    {
        SelectedHotspot.BorderRingMesh.enabled = false;
        SelectedHotspot.selected = false;
        SelectedHotspot = null;
    }

    public void StartExpanding()
    {
        if (!showingMinimal) return;
        showingMinimal = false;
        
        Expand(true);
    }

    public void StartCollapsing()
    {
        if (showingMinimal) return;
        showingMinimal = true;
        
        Expand(false);
    }
    
    public void ShowExpandedDetails()
    {
        viewport.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, expandedViewportSize);
        //content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, expandedContentSize);
    }

    public void ShowMinimalDetails()
    {
        viewport.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, collapsedViewportSize);
        //content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, collapsedContentSize);
    }

    private void Expand(bool state)
    {
        headerLabel.SetActive(state);
        dividerBar.SetActive(state);
            
        content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, state ? expandedContentSize : collapsedContentSize);
        
        foreach (var card in cachedArtworkNavigationCards)
        {
            card.Expand(state);
        }
    }
}
