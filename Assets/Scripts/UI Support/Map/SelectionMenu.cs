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

    [Header("References")]
    [SerializeField] private TMP_Text[] hotspotNameLabel;
    [SerializeField] private TMP_Text[] distanceLabel;
    [SerializeField] private TMP_Text[] artistLabel;
    [SerializeField] private TMP_Text[] artworkLabel;
    [SerializeField] private Button[] selectionButton;
    [SerializeField] private Button[] exitButtons; // these are changed from exit buttons to view buttons, that will open the artwork in the exhibition section
    [Space]
    [SerializeField] private GameObject[] container;
    [SerializeField] private List<RectTransform> layoutGroups;
    [SerializeField] private List<RectTransform> layoutGroupsInRange;

    [Header("Dependencies")]
    [SerializeField] private MapFilterToggle mapFilterToggle;
    [SerializeField] private MoveMapToArtwork mapMover;

    [Header("Navigation Tools")]
    [SerializeField] private GameObject navigationObject;

    [Header("Debug")]
    [SerializeField] private Button openButton;
    
    public static ArtworkData SelectedARPoint = null;
    private HotspotManager cachedHotspot;
    
    private void Awake()
    {
        if (!Instance) Instance = this;
        
        container[0].SetActive(false);
        container[1].SetActive(false);
        
        exitButtons[0].onClick.AddListener(OpenArtwork);
        exitButtons[1].onClick.AddListener(StartAR);
        
        openButton.onClick.AddListener(StartAR);
        
        openButton.gameObject.SetActive(true);        
#if UNITY_EDITOR
        openButton.gameObject.SetActive(true);        
#endif
    }

    private void OnEnable()
    {
        if(OnlineMapsControlBase.instance) OnlineMapsControlBase.instance.OnMapClick += OnMapClicked;
        ARMapPointMaker.OnHotspotsSpawned += GoToSelection;
        
#if UNITY_EDITOR
        openButton.gameObject.SetActive(true);        
#endif
    }

    private void OnDisable()
    {
        if(OnlineMapsControlBase.instance) OnlineMapsControlBase.instance.OnMapClick -= OnMapClicked;
        ARMapPointMaker.OnHotspotsSpawned += GoToSelection;
    }

    private void GoToSelection()
    {
        if (SelectedARPoint != null)
        {
            mapMover.Move(SelectedARPoint);
            SelectedARPoint = null;
        }
    }
    
    public void LoadARPointSO()
    {
        /*if (SelectedARPoint)
        {
            Open(SelectedARPoint.Hotspot, false);
            mapMover.Move(SelectedARPoint);
            SelectedARPoint = null;
        }*/
    }

    public void Open(HotspotManager hotspot, bool inRange)
    {
        if(cachedHotspot) cachedHotspot.ShowSelectionBorder(false);
        cachedHotspot = hotspot;
        
        container[0].SetActive(!inRange);
        container[1].SetActive(inRange);
        
        mapFilterToggle.Close();
        
        var artwork = hotspot.GetHotspotArtwork();

        hotspotNameLabel[inRange ? 1 : 0].text = artwork.title;
        distanceLabel[inRange ? 1 : 0].text = $"{artwork.max_distance:F1}m";
        artistLabel[inRange ? 1 : 0].text = artwork.artists.Count > 0 ? $"{artwork.artists[0].title}" : string.Empty;
        artworkLabel[inRange ? 1 : 0].text = hotspot.ConnectedExhibition.title;
        
        selectionButton[0].onClick.RemoveAllListeners();
        selectionButton[1].onClick.RemoveAllListeners();
        selectionButton[inRange ? 1 : 0].onClick.AddListener(inRange && hotspot.CanShow ? OpenArtwork : hotspot.GetDirections);
        
        foreach (var g in inRange ? layoutGroupsInRange : layoutGroups)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(g);
        }
        
        navigationObject.SetActive(false);
    }

    private void OpenArtwork()
    {
        if (!cachedHotspot) return;
        
        ArtworkUIManager.SelectedArtwork = cachedHotspot.GetHotspotArtwork();
        SceneManager.LoadScene("Exhibition&Art");
    }

    private void StartAR()
    {
        if (!cachedHotspot) return;
        
        cachedHotspot.StartAR(cachedHotspot.GetHotspotArtwork());
    }

    public void UpdateDistance(float d)
    {
        distanceLabel[0].text = $"{d:F1}m";
        distanceLabel[1].text = $"{d:F1}m";
    }

    public void Close()
    {
        container[0].SetActive(false);
        container[1].SetActive(false);
        navigationObject.SetActive(true);
    }
    
    private void HardClose()
    {
        if (cachedHotspot == null) return;
        if (cachedHotspot.inPlayerRange) return;
        cachedHotspot.BorderRingMesh.enabled = false;
        cachedHotspot.selected = false;
        Close();
        cachedHotspot = null;
    }
    
    private void OnMapClicked() => HardClose();  
}
