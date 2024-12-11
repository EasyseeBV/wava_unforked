using System;
using System.Collections;
using System.Collections.Generic;
using Messy.Definitions;
using UnityEngine;
using Michsky.MUIP;
using TMPro;
using System.Linq;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class ArtworkUIManager : MonoBehaviour
{
    public static ArtworkUIManager Instance;
    
    private enum MenuNavigation
    {
        Artworks,
        Exhibitions,
        Artists
    } 
        
    [Header("Prefabs")]
    public GameObject ArtworkUIPrefab;
    public GameObject ExhibitionUIPrefab;
    public GameObject ArtistUIPrefab;
    [Space]
    public bool HasSelectionMenu = true;
    public static bool SelectedArtworks = true;
    public static ExhibitionSO SelectedExhibition = null;
    public static ARPointSO SelectedArtwork = null;
    public static ArtistSO SelectedArtist = null;

    [Header("Layout Groups")] 
    [SerializeField] private ScrollRect visualAreaScrollRect;
    [SerializeField] private RectTransform defaultLayoutArea; 
    [SerializeField] private RectTransform artistsLayoutArea; 
    
    [Space]
    public GameObject BackArrow;
    public TextMeshProUGUI ExhibitionTitle;
    [Space]
    public GameObject InformationHelpBar;

    [Header("Artwork Details")] 
    [SerializeField] private ArtworkDetailsPanel artworkDetailsPanel;
    [SerializeField] private GameObject artworkDetailsArea;

    [Header("Gallery Details")] 
    [SerializeField] private ExhibitionDetailsPanel exhibitionDetailsPanel;
    [SerializeField] private GameObject exhibitionDetailsArea;

    [Header("Artist Details")] 
    [SerializeField] private ArtistDetailsPanel artistDetailsPanel;
    [SerializeField] private GameObject artistDetailsArea;

    [Header("Old Details Method")]
    public ARStaticDetails arStaticDetails;
    public GameObject DetailedPage;
    public HorizontalSelector horizontalSelector;
    public TextMeshProUGUI Title;
    public TextMeshProUGUI Artist;
    public TextMeshProUGUI Year;
    public TextMeshProUGUI Location;
    public TextMeshProUGUI Description;
    public TextMeshProUGUI Header;

    [Header("Gallery Naviagtion")]
    [SerializeField] private Transform currentNavigationArea;
    [SerializeField] private TMP_Text artworkNavigationLabel;
    [SerializeField] private TMP_Text exhibitionsNavigationLabel;
    [SerializeField] private TMP_Text artistsNavigationLabel;
    [Space]
    [SerializeField] private Color navigationActive = Color.black;
    [SerializeField] private Color navigationInactive = Color.grey;
    
    [FormerlySerializedAs("cachedGalleryDisplays")]
    [HideInInspector] public List<ArtworkShower> CachedGalleryDisplays = new ();
    [HideInInspector] public GalleryFilter.Filter CurrentFilter = GalleryFilter.Filter.RecentlyAdded;

    private void Awake()
    {
        if (!Instance) Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        /*if (HasSelectionMenu) {
            if (SelectedArtworks) {
                InitArtworks();
            } else {
                InitExhibitions();
            }
        }*/

        if (SelectedExhibition)
        {
            OpenDetailedInformation(SelectedExhibition);
            SelectedExhibition = null;
        }
        else if (SelectedArtwork)
        {
            OpenDetailedInformation(SelectedArtwork);
            SelectedArtwork = null;
        }
        else if (SelectedArtist)
        {
            OpenDetailedInformation(SelectedArtist);
            SelectedArtist = null;
        }
        

        if (PlayerPrefs.HasKey("DetailedInfoHelpBar"))
        {
            if(InformationHelpBar) InformationHelpBar.SetActive(false);
        }
    }

    public void ClearStage() {
        foreach (Transform child in defaultLayoutArea.transform) {
            Destroy(child.gameObject);
        }
        
        foreach (Transform child in artistsLayoutArea.transform) {
            Destroy(child.gameObject);
        }
        
        BackArrow.SetActive(false);
        ExhibitionTitle.text = "Exhibitions";
        CachedGalleryDisplays.Clear();
    }

    public void InitArtworks() {
        if (ARInfoManager.ExhibitionsSO == null)
            return;
        
        ClearStage();
        ShowDefaultLayoutArea(true);
        
        // Flatten all ArtWorks, filter those with images, and sort by creationDateTime descending
        var sortedArtworks = ARInfoManager.ExhibitionsSO
            .SelectMany(exhibition => exhibition.ArtWorks)
            .Where(artwork => artwork.ArtworkImages.Count != 0)
            .OrderByDescending(artwork => artwork.creationDateTime);
        
        foreach (ARPointSO point in sortedArtworks) {
            ArtworkShower shower = Instantiate(ArtworkUIPrefab, defaultLayoutArea).GetComponent<ArtworkShower>();
            shower.Init(point);
            CachedGalleryDisplays.Add(shower);
        }
        
        ChangeMenuNavigation(MenuNavigation.Artworks);
    }

    public void InitExhibitions() 
    {
        if (ARInfoManager.ExhibitionsSO == null)
            return;
        
        ClearStage();
        ShowDefaultLayoutArea(true);
        
        // Sort Exhibitions by creationDateTime descending
        var sortedExhibitions = ARInfoManager.ExhibitionsSO.OrderByDescending(exhibition => exhibition.creationDateTime);
        
        foreach (ExhibitionSO exhibition in sortedExhibitions) {
            ExhibitionCard card = Instantiate(ExhibitionUIPrefab, defaultLayoutArea).GetComponent<ExhibitionCard>();
            card.Init(exhibition);
        }
        
        ChangeMenuNavigation(MenuNavigation.Exhibitions);
    }
    
    public void InitArtists()
    {
        if (ARInfoManager.ExhibitionsSO == null) return;
        
        ClearStage();
        ShowDefaultLayoutArea(false);
        List<ArtistSO> unorderedList = new();
        foreach (ExhibitionSO exhibition in ARInfoManager.ExhibitionsSO) 
        {
            foreach (var artwork in exhibition.ArtWorks)
            {
                foreach (var artist in artwork.Artists)
                {
                    if(unorderedList.Contains(artist)) continue;
                    if (artist == null) continue;
                    unorderedList.Add(artist);
                }
            }
        }

        var orderedArtistList = unorderedList.OrderByDescending(artist => artist.creationDateTime).ToList();
        foreach (var artist in orderedArtistList)
        {
            ArtistContainer container = Instantiate(ArtistUIPrefab, artistsLayoutArea).GetComponent<ArtistContainer>();
            container.Assign(artist);
        }
        
        ChangeMenuNavigation(MenuNavigation.Artists);
        LayoutRebuilder.ForceRebuildLayoutImmediate(artistsLayoutArea);
    }

    private void ShowDefaultLayoutArea(bool state)
    {
        visualAreaScrollRect.content = state ? defaultLayoutArea : artistsLayoutArea;
        defaultLayoutArea.gameObject.SetActive(state);
        artistsLayoutArea.gameObject.SetActive(!state);
    }

    private void ChangeMenuNavigation(MenuNavigation navigation)
    {
        artworkNavigationLabel.color = navigation == MenuNavigation.Artworks ? navigationActive : navigationInactive;
        exhibitionsNavigationLabel.color = navigation == MenuNavigation.Exhibitions ? navigationActive : navigationInactive;
        artistsNavigationLabel.color = navigation == MenuNavigation.Artists ? navigationActive : navigationInactive;

        currentNavigationArea.localPosition = navigation switch
        {
            MenuNavigation.Artworks => new Vector3(-114.3f, -18f, 0),
            MenuNavigation.Exhibitions => new Vector3(0, -18f, 0),
            MenuNavigation.Artists => new Vector3(114.3f, -18f, 0),
            _ => throw new ArgumentOutOfRangeException(nameof(navigation), navigation, null)
        };
        
        ApplySorting();
    }

    public void ApplySorting()
    {
        CachedGalleryDisplays = CachedGalleryDisplays.OrderByDescending(display => display.cachedARPointSO.creationDateTime).ToList();

        return;
        switch (CurrentFilter)
        {
            case GalleryFilter.Filter.NewestToOldest:
                CachedGalleryDisplays = CachedGalleryDisplays.OrderByDescending(display => display.cachedARPointSO.Year).ToList();
                break;
            case GalleryFilter.Filter.OldestToNewest:
                CachedGalleryDisplays = CachedGalleryDisplays.OrderBy(display => display.cachedARPointSO.Year).ToList();
                break;
            case GalleryFilter.Filter.Exhibitions:
                CachedGalleryDisplays = CachedGalleryDisplays.OrderBy(display => display.cachedARPointSO.Artist).ToList();
                break;
            case GalleryFilter.Filter.Location:
                CachedGalleryDisplays = CachedGalleryDisplays.OrderBy(display => display.cachedARPointSO.Location).ToList();
                break;
            case GalleryFilter.Filter.RecentlyAdded:
                CachedGalleryDisplays = CachedGalleryDisplays.OrderByDescending(display => display.cachedARPointSO.creationDateTime).ToList();
                break;
            case GalleryFilter.Filter.Any:
            default:
                // If the sorting is "Any" or unknown, just return without sorting
                return;
        }

        // Update the sibling index of each display
        for (int i = 0; i < CachedGalleryDisplays.Count; i++)
        {
            CachedGalleryDisplays[i].transform.SetSiblingIndex(i);
        }
    }

    public void OpenDetailedInfoFromStaticAR() {
        if (ArTapper.ARPointToPlace != null)
            arStaticDetails.Open(ArTapper.ARPointToPlace);

        SetBarInActive();
    }

    public void SetBarInActive()
    {
        PlayerPrefs.SetInt("DetailedInfoHelpBar", 1);
        if (InformationHelpBar != null)
            InformationHelpBar.SetActive(false);
    }

    public void OpenDetailedInformation(ARPointSO arPoint) 
    {
        artworkDetailsArea?.SetActive(true);
        exhibitionDetailsArea?.SetActive(false);
        artistDetailsArea?.SetActive(false);
        artworkDetailsPanel?.Fill(arPoint);
        if (Title) Title.text = arPoint.Title;
        if (Description) Description.text = arPoint.Description;
        if (Header) Header.text = arPoint.Title;
    }

    public void OpenDetailedInformation(ExhibitionSO exhibition)
    {
        exhibitionDetailsArea.SetActive(true);
        artworkDetailsArea.SetActive(false);
        artistDetailsArea.SetActive(false);
        exhibitionDetailsPanel.Fill(exhibition);
    }
    
    public void OpenDetailedInformation(ArtistSO artist)
    {
        artistDetailsArea.SetActive(true);
        exhibitionDetailsArea.SetActive(false);
        artworkDetailsArea.SetActive(false);
        artistDetailsPanel.Fill(artist);
    }
}
