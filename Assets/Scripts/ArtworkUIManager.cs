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
    
    [FormerlySerializedAs("cachedGalleryDisplays")]
    [HideInInspector] public List<ArtworkShower> CachedGalleryDisplays = new ();
    [HideInInspector] public GalleryFilter.Filter CurrentFilter = GalleryFilter.Filter.RecentlyAdded;
    
    private MenuNavigation currentMenuNavigation = MenuNavigation.Artworks;
    public static event Action OnNewDocumentAdded;

    private string currentSceneName = string.Empty;

    private void Awake()
    {
        if (!Instance) Instance = this;
        
        loadingCircle?.BeginLoading();
    }

    private void OnEnable()
    {
        currentSceneName = SceneManager.GetActiveScene().name;
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
            }
        }

        if (PlayerPrefs.HasKey("DetailedInfoHelpBar"))
        {
            if(InformationHelpBar) InformationHelpBar.SetActive(false);
        }
    }

    public void ClearStage() 
    {
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

    public void InitArtworks() 
    {
        if (loadingCircle != null && loadingCircle.isActiveAndEnabled) loadingCircle.BeginLoading();
        ClearStage();
        ShowDefaultLayoutArea(true);
        ChangeMenuNavigation(MenuNavigation.Artworks);
        FetchNewArtworks();
        ApplySorting();
    }

    private async Task FetchNewArtworks()
    {
        if (!FirebaseLoader.ArtworkCollectionFull && FirebaseLoader.Artworks.Count < minArtworkCount)
        {
            await FirebaseLoader.FetchDocuments<ArtworkData>(Mathf.Abs(minArtworkCount - FirebaseLoader.Artworks.Count));
        }
        
        // Flatten all ArtWorks, filter those with images, and sort by creationDateTime descending
        var sortedArtworks = FirebaseLoader.Artworks
            .OrderByDescending(artwork => artwork.creation_date_time);
        
        foreach (ArtworkData artwork in sortedArtworks)
        {
            // guard clause to avoid populating in the incorrect menu or scene
            if (currentMenuNavigation != MenuNavigation.Artworks || currentSceneName != SceneManager.GetActiveScene().name) return;
            // double guard
            if (currentMenuNavigation != MenuNavigation.Artworks || currentSceneName != SceneManager.GetActiveScene().name) return;
            
            ArtworkShower shower = Instantiate(ArtworkUIPrefab, defaultLayoutArea).GetComponent<ArtworkShower>();
            shower.Init(artwork);
            if (loadingCircle != null && loadingCircle.isActiveAndEnabled) loadingCircle.StopLoading();
            CachedGalleryDisplays.Add(shower);
        }
    }

    public void InitExhibitions() 
    {
        if (loadingCircle != null && loadingCircle.isActiveAndEnabled) loadingCircle.BeginLoading();
        ClearStage();
        ShowDefaultLayoutArea(true);
        ChangeMenuNavigation(MenuNavigation.Exhibitions);
        FetchNewExhibitions();
        ApplySorting();
    }
    
    private async Task FetchNewExhibitions()
    {
        if (!FirebaseLoader.ExhibitionCollectionFull && FirebaseLoader.ExhibitionCollectionSize < minExhibitionCount)
        {
            await FirebaseLoader.LoadRemainingExhibitions();
        }
        else if (!FirebaseLoader.ExhibitionCollectionFull && FirebaseLoader.Exhibitions.Count < minExhibitionCount)
        {
            await FirebaseLoader.FetchDocuments<ExhibitionData>(Mathf.Abs(minExhibitionCount - FirebaseLoader.Exhibitions.Count));
        }
        
        // Sort Exhibitions by creation_time descending
        var sortedExhibitions = FirebaseLoader.Exhibitions.OrderByDescending(exhibition => exhibition.creation_date_time);

        foreach (ExhibitionData exhibition in sortedExhibitions)
        {
            // guard clause to avoid populating in the incorrect menu or scene
            if (currentMenuNavigation != MenuNavigation.Exhibitions || currentSceneName != SceneManager.GetActiveScene().name) return;
            ExhibitionCard card = Instantiate(ExhibitionUIPrefab, defaultLayoutArea).GetComponent<ExhibitionCard>();
            card.Init(exhibition);
            if (loadingCircle != null && loadingCircle.isActiveAndEnabled) loadingCircle.StopLoading();
        }
    }
    
    public void InitArtists()
    {
        if (loadingCircle != null && loadingCircle.isActiveAndEnabled) loadingCircle.StopLoading();
        ClearStage();
        ShowDefaultLayoutArea(false);
        ChangeMenuNavigation(MenuNavigation.Artists);
        ApplySorting();
        FetchNewArtists();
        LayoutRebuilder.ForceRebuildLayoutImmediate(artistsLayoutArea);
    }
    
    private async Task FetchNewArtists()
    {
        if (!FirebaseLoader.ArtistCollectionFull && FirebaseLoader.Artists.Count < minArtistCount)
        {
            await FirebaseLoader.FetchDocuments<ArtistData>(Mathf.Abs(minArtistCount - FirebaseLoader.Artists.Count));
        }
        
        var sortedArtists = FirebaseLoader.Artists.OrderByDescending(artwork => artwork.creation_time);
        foreach (var artist in sortedArtists)
        {
            // guard clause to avoid populating in the incorrect menu or scene
            if (currentMenuNavigation != MenuNavigation.Artists || currentSceneName != SceneManager.GetActiveScene().name) return;
            ArtistContainer container = Instantiate(ArtistUIPrefab, artistsLayoutArea).GetComponent<ArtistContainer>();
            container.Assign(artist);
            if (loadingCircle != null && loadingCircle.isActiveAndEnabled) loadingCircle.StopLoading();
        }
    }

    public async void AddNewDocument()
    {
        try
        {
            switch (currentMenuNavigation)
            {
                case MenuNavigation.Artworks:
                    if (FirebaseLoader.ArtworkCollectionFull)
                    {
                        return;
                    }
                    
                    var artworkDoc = await FirebaseLoader.FetchDocuments<ArtworkData>(1);

                    if (artworkDoc.Count < 0) return;
                    if (artworkDoc[0] == null) return;
                
                    ArtworkShower shower = Instantiate(ArtworkUIPrefab, defaultLayoutArea).GetComponent<ArtworkShower>();
                    shower.Init(artworkDoc[0]);
                    CachedGalleryDisplays.Add(shower);
                    break;
            
                case MenuNavigation.Exhibitions:
                    if (FirebaseLoader.ExhibitionCollectionFull) return;
                
                    var exhibitionDoc = await FirebaseLoader.FetchDocuments<ExhibitionData>(1);
                    ExhibitionCard card = Instantiate(ExhibitionUIPrefab, defaultLayoutArea).GetComponent<ExhibitionCard>();
                    card.Init(exhibitionDoc[0]);
                    break;
            
                case MenuNavigation.Artists:
                    if (FirebaseLoader.ArtistCollectionFull) return;
                
                    var artistDoc = await FirebaseLoader.FetchDocuments<ArtistData>(1);
                    ArtistContainer container = Instantiate(ArtistUIPrefab, artistsLayoutArea).GetComponent<ArtistContainer>();
                    container.Assign(artistDoc[0]);
                    break;
            
                default:
                    throw new ArgumentOutOfRangeException();
            }
        
            OnNewDocumentAdded?.Invoke();
        }
        catch (Exception e)
        {
            Debug.LogWarning("Failed to add new documents: " + e);
        }
    }
    
    private void ShowDefaultLayoutArea(bool state)
    {
        visualAreaScrollRect.content = state ? defaultLayoutArea : artistsLayoutArea;
        defaultLayoutArea.gameObject.SetActive(state);
        artistsLayoutArea.gameObject.SetActive(!state);
    }

    private void ChangeMenuNavigation(MenuNavigation navigation)
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
        return;
        CachedGalleryDisplays = CachedGalleryDisplays.OrderByDescending(display => display.cachedArtwork.creation_date_time).ToList();

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
        }
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

}