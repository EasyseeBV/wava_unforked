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

    public static ARPointSO SelectedARPoint = null;
    private HotspotManager cachedHotspot;
    
    private void Awake()
    {
        if (!Instance) Instance = this;
        
        container[0].SetActive(false);
        container[1].SetActive(false);
        
        exitButtons[0].onClick.AddListener(OpenArtwork);
        exitButtons[1].onClick.AddListener(OpenArtwork);
    }

    private void OnEnable() => OnlineMapsControlBase.instance.OnMapClick += OnMapClicked;

    private void OnDisable()
    {
        if(OnlineMapsControlBase.instance != null) OnlineMapsControlBase.instance.OnMapClick -= OnMapClicked;
    } 
    

    public void LoadARPointSO()
    {
        if (SelectedARPoint)
        {
            Open(SelectedARPoint.Hotspot, false);
            mapMover.Move(SelectedARPoint);
            SelectedARPoint = null;
        }
    }

    public void Open(HotspotManager hotspot, bool inRange)
    {
        cachedHotspot = hotspot;
        
        container[0].SetActive(!inRange);
        container[1].SetActive(inRange);
        
        mapFilterToggle.Close();
        
        var ar = hotspot.GetHotspotARPointSO();

        hotspotNameLabel[inRange ? 1 : 0].text = ar.Title;
        distanceLabel[inRange ? 1 : 0].text = $"{ar.MaxDistance:F1}m";
        artistLabel[inRange ? 1 : 0].text = ar.Artist;
        artworkLabel[inRange ? 1 : 0].text = hotspot.ConnectedExhibition.Title;
        
        selectionButton[0].onClick.RemoveAllListeners();
        selectionButton[1].onClick.RemoveAllListeners();
        selectionButton[inRange ? 1 : 0].onClick.AddListener(inRange ? OpenArtwork : hotspot.OnTouch);
        
        foreach (var g in inRange ? layoutGroupsInRange : layoutGroups)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(g);
        }
    }

    private void OpenArtwork()
    {
        if (!cachedHotspot) return;
        
        ArtworkUIManager.SelectedArtwork = cachedHotspot.GetHotspotARPointSO();
        SceneManager.LoadScene("Exhibition&Art");
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
    }
    
    private void OnMapClicked() => Close();  
}
