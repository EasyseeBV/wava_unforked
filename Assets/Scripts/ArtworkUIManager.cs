using Michsky.MUIP;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
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

    [SerializeField] private HorizontalSwipeDetector swipeDetector;

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

    [Header("Navigation")]
    [SerializeField] UnderlinedSelectionUI underlinedSelectionUI;
    [SerializeField] Button artworksButton;
    [SerializeField] Button exhibitionsButton;
    [SerializeField] Button artistsButton;

    [HideInInspector] public GalleryFilter.Filter CurrentFilter = GalleryFilter.Filter.RecentlyAdded;

    private DisplayView openView;

    private List<ArtworkShower> loadedArtworks = new();
    private List<ExhibitionCard> loadedExhibitions = new();
    private List<ArtistContainer> loadedArtists = new();
    
    private MenuNavigation currentMenuNavigation = MenuNavigation.Artworks;

    private bool canTryLoadInvisible = false;
    
    private void Awake()
    {
        // Subscribe to navigation buttons.
        artworksButton.onClick.AddListener(InitArtworks);
        exhibitionsButton.onClick.AddListener(InitExhibitions);
        artistsButton.onClick.AddListener(InitArtists);

        swipeDetector.SwipedRight += (_) =>
        {
            // Don't switch if a details page is opened.
            if (artworkDetailsArea.activeInHierarchy
            || exhibitionDetailsArea.activeInHierarchy
            || artistDetailsArea.activeInHierarchy)
                return;

            switch (openView)
            {
                case DisplayView.Artists:
                    InitExhibitions();
                    break;
                case DisplayView.Exhibitions:
                    InitArtworks();
                    break;
            }
        };

        swipeDetector.SwipedLeft += (_) =>
        {
            // Don't switch if a details page is opened.
            if (artworkDetailsArea.activeInHierarchy
            || exhibitionDetailsArea.activeInHierarchy
            || artistDetailsArea.activeInHierarchy)
                return;

            switch (openView)
            {
                case DisplayView.Artworks:
                    InitExhibitions();
                    break;
                case DisplayView.Exhibitions:
                    InitArtists();
                    break;
            }
        };

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
        openView = DisplayView.Artworks;

        if (loadingCircle != null && loadingCircle.isActiveAndEnabled) loadingCircle.StopLoading();
        
        ReplaceStage<ArtworkData>();
        ShowDefaultLayoutArea(true);
        currentMenuNavigation = MenuNavigation.Artworks;
        underlinedSelectionUI.ShowAsSelected(0);
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
        openView = DisplayView.Exhibitions;

        if (loadingCircle != null && loadingCircle.isActiveAndEnabled) loadingCircle.BeginLoading();
        ReplaceStage<ExhibitionData>();
        ShowDefaultLayoutArea(true);
        currentMenuNavigation = MenuNavigation.Exhibitions;
        underlinedSelectionUI.ShowAsSelected(1);
        FetchNewExhibitions();
        ApplySorting();
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
        openView = DisplayView.Artists;

        if (loadingCircle != null && loadingCircle.isActiveAndEnabled) loadingCircle.StopLoading();
        ReplaceStage<ArtistData>();
        ShowDefaultLayoutArea(false);
        currentMenuNavigation = MenuNavigation.Artists;
        underlinedSelectionUI.ShowAsSelected(2);
        ApplySorting();
        FetchNewArtists();
        LayoutRebuilder.ForceRebuildLayoutImmediate(artistsLayoutArea);
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
            if (li.IsLoading || li.ArtworkImage.sprite != null) continue;

            // get the child’s bounds **relative to** the viewport
            var childBounds = RectTransformUtility.CalculateRelativeRectTransformBounds(viewport, li.transform as RectTransform);
            if (childBounds.center.y > -1200) li.SetImage();
        }
    }
}