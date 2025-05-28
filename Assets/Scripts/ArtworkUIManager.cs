using System;
using System.Collections;
using System.Collections.Generic;
using Messy.Definitions;
using UnityEngine;
using Michsky.MUIP;
using TMPro;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
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
    public static ExhibitionData SelectedExhibition = null;
    public static ArtworkData SelectedArtwork = null;
    public static ArtistData SelectedArtist = null;

    [Header("Layout Groups")] 
    [SerializeField] private ScrollRect visualAreaScrollRect;
    [SerializeField] private RectTransform defaultLayoutArea; 
    [SerializeField] private RectTransform artistsLayoutArea;

    [Header("Scroll Rect Info")]
    [SerializeField] private Canvas parentCanvas;
    [SerializeField] private RectTransform viewport;
    [SerializeField] private RectTransform content;
    
    private float preloadMargin = 100f; 
    
    [Space]
    public GameObject BackArrow;
    public TextMeshProUGUI ExhibitionTitle;
    [Space]
    public GameObject InformationHelpBar;
    [SerializeField] private LoadingCircle loadingCircle;

    [Header("Artwork Details")] 
    [SerializeField] private ArtworkDetailsPanel artworkDetailsPanel;
    [SerializeField] private GameObject artworkDetailsArea;
    [SerializeField] private int minArtworkCount = 3;

    [Header("Exhibition Details")] 
    [SerializeField] private ExhibitionDetailsPanel exhibitionDetailsPanel;
    [SerializeField] private GameObject exhibitionDetailsArea;
    [SerializeField] private int minExhibitionCount = 3;
    
    [Header("Artist Details")] 
    [SerializeField] private ArtistDetailsPanel artistDetailsPanel;
    [SerializeField] private GameObject artistDetailsArea;
    [SerializeField] private int minArtistCount = 8;
    
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

    [Header("Gallery Navigation")]
    [SerializeField] private Transform currentNavigationArea;
    [SerializeField] private TMP_Text artworkNavigationLabel;
    [SerializeField] private TMP_Text exhibitionsNavigationLabel;
    [SerializeField] private TMP_Text artistsNavigationLabel;
    [Space]
    [SerializeField] private Color navigationActive = Color.black;
    [SerializeField] private Color navigationInactive = Color.grey;

    [HideInInspector] public GalleryFilter.Filter CurrentFilter = GalleryFilter.Filter.RecentlyAdded;

    private List<ArtworkShower> loadedArtworks = new();
    private List<ExhibitionCard> loadedExhibitions = new();
    private List<ArtistContainer> loadedArtists = new();
    
    private MenuNavigation currentMenuNavigation = MenuNavigation.Artworks;

    private bool canTryLoadInvisible = false;
    

    private void Awake()
    {
        if (!Instance) Instance = this;

        if (parentCanvas == null) parentCanvas = GetComponentInParent<Canvas>();
        
        loadingCircle?.BeginLoading();
        visualAreaScrollRect?.onValueChanged.AddListener(_ => TryLoadVisible());
    }
    
    private void Start()
    {
        if (SelectedExhibition != null)
        {
            OpenDetailedInformation(SelectedExhibition);
            SelectedExhibition = null;
        }
        else if (SelectedArtwork != null)
        {
            OpenDetailedInformation(SelectedArtwork);
            SelectedArtwork = null;
        }
        else if (SelectedArtist != null)
        {
            OpenDetailedInformation(SelectedArtist);
            SelectedArtist = null;
        }
        else
        {
            if (AutoLoadDisplay.View == DisplayView.Exhibitions)
            {
                InitExhibitions();
            }
            else if (AutoLoadDisplay.View == DisplayView.Artists)
            {
                InitArtists();
            }
            else if (AutoLoadDisplay.View == DisplayView.Artworks)
            {
                InitArtworks();
                
                for (int i = 0; i < Mathf.Min(minArtworkCount, loadedArtworks.Count); i++)
                {
                    loadedArtworks[i].SetImage();
                }
            }
        }

        if (PlayerPrefs.HasKey("DetailedInfoHelpBar"))
        {
            if(InformationHelpBar) InformationHelpBar.SetActive(false);
        }
    }

    private void ReplaceStage<T>() where T : FirebaseData
    {
        visualAreaScrollRect.verticalNormalizedPosition = 1f;
        canTryLoadInvisible = false;
            
        foreach (var artwork in loadedArtworks)
        {
            artwork.gameObject.SetActive(typeof(T) == typeof(ArtworkData));
        }
        
        foreach (var exhibition in loadedExhibitions)
        {
            exhibition.gameObject.SetActive(typeof(T) == typeof(ExhibitionData));
        }
        
        foreach (var artist in loadedArtists)
        {
            artist.gameObject.SetActive(typeof(T) == typeof(ArtistData));
        }
        
        BackArrow.SetActive(false);
        ExhibitionTitle.text = "Exhibitions";
    }

    private IEnumerator WaitForCanvases()
    {
        yield return new WaitForEndOfFrame();
        canTryLoadInvisible = true;
    }

    public void InitArtworks() 
    {
        if (loadingCircle != null && loadingCircle.isActiveAndEnabled) loadingCircle.StopLoading();
        
        ReplaceStage<ArtworkData>();
        ShowDefaultLayoutArea(true);
        ChangeMenuNavigationUI(MenuNavigation.Artworks);
        FetchNewArtworks();
        ApplySorting();
        StartCoroutine(WaitForCanvases());
    }

    private void FetchNewArtworks()
    {
        // Flatten all ArtWorks, filter those with images, and sort by creationDateTime descending
        var sortedArtworks = FirebaseLoader.Artworks
            .OrderByDescending(artwork => artwork.creation_date_time);
        
        foreach (ArtworkData artwork in sortedArtworks)
        {
            if (loadedArtworks.Any(artworkShower => artworkShower.cachedArtwork == artwork)) continue;
            
            ArtworkShower shower = Instantiate(ArtworkUIPrefab, defaultLayoutArea).GetComponent<ArtworkShower>();
            shower.Init(artwork, false);
            loadedArtworks.Add(shower);
            if (loadingCircle != null && loadingCircle.isActiveAndEnabled) loadingCircle.StopLoading();
        }
    }

    public void InitExhibitions() 
    {
        if (loadingCircle != null && loadingCircle.isActiveAndEnabled) loadingCircle.BeginLoading();
        ReplaceStage<ExhibitionData>();
        ShowDefaultLayoutArea(true);
        ChangeMenuNavigationUI(MenuNavigation.Exhibitions);
        FetchNewExhibitions();
        ApplySorting();
        StartCoroutine(WaitForCanvases());
    }
    
    private void FetchNewExhibitions()
    {
        // Sort Exhibitions by creation_time descending
        var sortedExhibitions = FirebaseLoader.Exhibitions.
            OrderByDescending(exhibition => exhibition.creation_date_time);

        foreach (ExhibitionData exhibition in sortedExhibitions)
        {
            if (loadedExhibitions.Any(exhibitionCard => exhibitionCard.exhibition == exhibition)) continue;
            
            ExhibitionCard card = Instantiate(ExhibitionUIPrefab, defaultLayoutArea).GetComponent<ExhibitionCard>();
            card.Init(exhibition);
            loadedExhibitions.Add(card);
            if (loadingCircle != null && loadingCircle.isActiveAndEnabled) loadingCircle.StopLoading();
        }
    }
    
    public void InitArtists()
    {
        if (loadingCircle != null && loadingCircle.isActiveAndEnabled) loadingCircle.StopLoading();
        ReplaceStage<ArtistData>();
        ShowDefaultLayoutArea(false);
        ChangeMenuNavigationUI(MenuNavigation.Artists);
        ApplySorting();
        FetchNewArtists();
        LayoutRebuilder.ForceRebuildLayoutImmediate(artistsLayoutArea);
        StartCoroutine(WaitForCanvases());
    }
    
    private void FetchNewArtists()
    {
        var sortedArtists = FirebaseLoader.Artists.
            OrderByDescending(artwork => artwork.creation_time);
        
        foreach (var artist in sortedArtists)
        {
            if (loadedArtists.Any(artistContainer => artistContainer.artist == artist)) continue;
            
            ArtistContainer container = Instantiate(ArtistUIPrefab, artistsLayoutArea).GetComponent<ArtistContainer>();
            container.Assign(artist);
            loadedArtists.Add(container);
            if (loadingCircle != null && loadingCircle.isActiveAndEnabled) loadingCircle.StopLoading();
        }
    }
    
    private void ShowDefaultLayoutArea(bool state)
    {
        visualAreaScrollRect.content = state ? defaultLayoutArea : artistsLayoutArea;
        defaultLayoutArea.gameObject.SetActive(state);
        artistsLayoutArea.gameObject.SetActive(!state);
    }

    private void ChangeMenuNavigationUI(MenuNavigation navigation)
    {
        currentMenuNavigation = navigation;
        
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
    }

    public void ApplySorting()
    {
        /*CachedGalleryDisplays = CachedGalleryDisplays.OrderByDescending(display => display.cachedArtwork.creation_date_time).ToList();

        switch (CurrentFilter)
        {
            case GalleryFilter.Filter.NewestToOldest:
                CachedGalleryDisplays = CachedGalleryDisplays.OrderByDescending(display => display.cachedArtwork.year).ToList();
                break;
            case GalleryFilter.Filter.OldestToNewest:
                CachedGalleryDisplays = CachedGalleryDisplays.OrderBy(display => display.cachedArtwork.year).ToList();
                break;
            case GalleryFilter.Filter.Exhibitions:
                CachedGalleryDisplays = CachedGalleryDisplays.OrderBy(display => display.cachedArtwork.artists).ToList();
                break;
            case GalleryFilter.Filter.Location:
                CachedGalleryDisplays = CachedGalleryDisplays.OrderBy(display => display.cachedArtwork.location).ToList();
                break;
            case GalleryFilter.Filter.RecentlyAdded:
                CachedGalleryDisplays = CachedGalleryDisplays.OrderByDescending(display => display.cachedArtwork.creation_time).ToList();
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
        }*/
    }

    public void OpenDetailedInfoFromStaticAR() 
    {
        if (ArTapper.ArtworkToPlace != null) arStaticDetails.Open(ArTapper.ArtworkToPlace);
        SetBarInActive();
    }

    public void SetBarInActive()
    {
        PlayerPrefs.SetInt("DetailedInfoHelpBar", 1);
        if (InformationHelpBar != null)
            InformationHelpBar.SetActive(false);
    }

    public void OpenDetailedInformation<T>(T data) where T : FirebaseData
    {
        // Disable all detail areas initially
        artworkDetailsArea?.SetActive(typeof(T) == typeof(ArtworkData));
        exhibitionDetailsArea?.SetActive(typeof(T) == typeof(ExhibitionData));
        artistDetailsArea?.SetActive(typeof(T) == typeof(ArtistData));
        
        //loadingCircle?.gameObject.SetActive(true);
        //loadingCircle?.BeginLoading();
        
        switch (data)
        {
            case ArtworkData artwork:
            {
                artworkDetailsPanel?.Fill(artwork);
                if (Title != null) Title.text = artwork.title;
                if (Description != null) Description.text = artwork.description;
                if (Header != null) Header.text = artwork.title;
                break;
            }
            case ExhibitionData exhibition:
                exhibitionDetailsPanel.Fill(exhibition);
                break;
            case ArtistData artist:
                artistDetailsPanel.Fill(artist);
                break;
            default:
                throw new ArgumentException($"Unsupported data type: {typeof(T).Name}", nameof(data));
        }
    }

    private void TryLoadVisible()
    {
        if (currentMenuNavigation != MenuNavigation.Artworks || !canTryLoadInvisible) return;
        foreach (var li in loadedArtworks)
        {
            if (li.IsLoading || li.ARPhoto.sprite != null) continue;

            // get the child’s bounds **relative to** the viewport
            var childBounds = RectTransformUtility.CalculateRelativeRectTransformBounds(viewport, li.transform as RectTransform);
            if (childBounds.center.y > -1200) li.SetImage();
        }
    }
}