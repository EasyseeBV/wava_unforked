using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    [SerializeField] private DraggableSelectionBar selectionBar;

    [Header("References")]
    [SerializeField] private GameObject container;
    [SerializeField] private GameObject notificationObject;
    [SerializeField] private GameObject arrivedNotificationObject;
    [Space]
    [SerializeField] private RectTransform viewport;
    [SerializeField] private RectTransform content;
    [SerializeField] private ScrollRect scrollRect;
    [Space]
    [SerializeField] private GameObject headerLabel;
    [SerializeField] private GameObject dividerBar;
    [Space]
    [SerializeField] private ArtworkNavigationCard artworkNavigationCardPrefab;

    [Header("Settings")]
    [SerializeField] private float rangeTolerance;
    [Space]
    [SerializeField] private float expandedViewportSize = 243f;
    [SerializeField] private float collapsedViewportSize = 96f;
    [SerializeField] private float expandedContentSize = 140f;
    [SerializeField] private float collapsedContentSize = 44f;
    [Space]
    [SerializeField] private float scrollDuration = 0.5f;

    public HotspotManager SelectedHotspot { get; private set; }
    
    private bool showingMinimal = true;
    
    private List<ArtworkNavigationCard> cachedArtworkNavigationCards = new List<ArtworkNavigationCard>();
    private List<ArtworkData> cachedArtworkData = new List<ArtworkData>();
    
    private void Awake()
    {
        if (!Instance) Instance = this;
        
        headerLabel.SetActive(false);
        dividerBar.SetActive(false);
        
        notificationObject.SetActive(false);
        arrivedNotificationObject.SetActive(false);
    }

    public void Add(ArtworkData artwork)
    {
        if (cachedArtworkData.Contains(artwork)) return;
        cachedArtworkData.Add(artwork);
    }

    public void Build()
    {
        foreach (var card in cachedArtworkNavigationCards) Destroy(card.gameObject);
        cachedArtworkNavigationCards.Clear();

        var lat = PlayerMarker.Instance.Latitude;
        var lon = PlayerMarker.Instance.Longitude;
        
        var withinRange = cachedArtworkData
            .Select(a => new {
                Data     = a,
                Distance = GeoUtils.DistanceInMeters(lat, lon, a.latitude, a.longitude)
            })
            .Where(x => x.Distance <= rangeTolerance)
            .OrderBy(x => x.Distance)
            .ToList();
        
        if (withinRange.Count > 0) container.gameObject.SetActive(true);
        
        foreach (var entry in withinRange)
        {
            var card = Instantiate(artworkNavigationCardPrefab, content);
            card.Populate(entry.Data);
            cachedArtworkNavigationCards.Add(card);
        }

        if (cachedArtworkNavigationCards.Count <= 0)
        {
            container.gameObject.SetActive(false);
        }
        
        LayoutRebuilder.ForceRebuildLayoutImmediate(content as RectTransform);
    }
    
    public void SelectHotspot(HotspotManager hotspot, bool inRange)
    {
        if (SelectedHotspot != null) DeselectHotspot();

        if (inRange && showingMinimal)
        {
            selectionBar.Expand(true);
        }
        
        SelectedHotspot = hotspot;
        SelectedHotspot.BorderRingMesh.enabled = true;
        SelectedHotspot.selected = true;
        
        notificationObject.SetActive(!inRange);
        arrivedNotificationObject.SetActive(inRange);

        if (inRange)
        {
            var card = cachedArtworkNavigationCards.FirstOrDefault(c => c.CachedArtworkData == hotspot.artwork);
            card?.AllowAR();
        }

        ScrollToArtwork(hotspot.artwork);
    }

    public void DeselectHotspot()
    {
        SelectedHotspot.BorderRingMesh.enabled = false;
        SelectedHotspot.selected = false;
        var card = cachedArtworkNavigationCards.FirstOrDefault(c => c.CachedArtworkData == SelectedHotspot.artwork);
        card?.DisallowAR();
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
    
    public void ScrollToArtwork(ArtworkData artwork)
    {
        int idx = cachedArtworkNavigationCards.FindIndex(c => c.CachedArtworkData == artwork);
        if (idx < 0)
        {
            Debug.LogWarning($"Artwork not found in active cards: {artwork.title} | Creating an artwork card");
            
            var card = Instantiate(artworkNavigationCardPrefab, content);
            card.Populate(artwork);
            if (!showingMinimal) card.Expand(true);
            cachedArtworkNavigationCards.Add(card);
            idx = cachedArtworkNavigationCards.Count - 1;
        }
        
        int total = cachedArtworkNavigationCards.Count;
        if (total == 0) return;

        // normalized target: idx=0 → 1.0 (top), idx=total-1 → 0.0 (bottom)
        float targetNorm = (total == 1) ? 0f : ((float)idx / (total - 1));

        StopAllCoroutines();
        StartCoroutine(ScrollCoroutine(targetNorm, scrollDuration));
    }

    private IEnumerator ScrollCoroutine(float target, float duration)
    {
        float start = scrollRect.horizontalNormalizedPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            scrollRect.horizontalNormalizedPosition = Mathf.Lerp(start, target, t);
            yield return null;
        }

        scrollRect.horizontalNormalizedPosition = target;
    }
}
