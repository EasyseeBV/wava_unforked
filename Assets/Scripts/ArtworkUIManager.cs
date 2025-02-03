using System;
using System.Collections;
using System.Collections.Generic;
using Messy.Definitions;
using UnityEngine;
using Michsky.MUIP;
using TMPro;
using System.Linq;
using System.Threading.Tasks;
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
    
    private MenuNavigation currentMenuNavigation = MenuNavigation.Artworks;
    public static event Action OnNewDocumentAdded;

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

    public void InitArtworks() 
    {
        if (FirebaseLoader.Artworks == null) return;
        
        ClearStage();
        ShowDefaultLayoutArea(true);

        FetchNewArtworks();
        
        ChangeMenuNavigation(MenuNavigation.Artworks);
    }

    private async Task FetchNewArtworks()
    {
        if (!FirebaseLoader.ArtworkCollectionFull)
        {
            await FirebaseLoader.FetchDocuments<ArtworkData>(1);
        }
        
        // Flatten all ArtWorks, filter those with images, and sort by creationDateTime descending
        var sortedArtworks = FirebaseLoader.Artworks
            .Where(artwork => artwork.artwork_images.Count != 0)
            .OrderByDescending(artwork => artwork.creation_time);
        
        foreach (ArtworkData artwork in sortedArtworks) 
        {
            ArtworkShower shower = Instantiate(ArtworkUIPrefab, defaultLayoutArea).GetComponent<ArtworkShower>();
            shower.Init(artwork);
            CachedGalleryDisplays.Add(shower);
        }
    }

    public void InitExhibitions() 
    {
        if (FirebaseLoader.Exhibitions == null) return;
        
        ClearStage();
        ShowDefaultLayoutArea(true);

        FetchNewExhibitions();
        
        ChangeMenuNavigation(MenuNavigation.Exhibitions);
    }
    
    private async Task FetchNewExhibitions()
    {
        if (!FirebaseLoader.ExhibitionCollectionFull)
        {
            await FirebaseLoader.FetchDocuments<ExhibitionData>(1);
        }
        
        // Sort Exhibitions by creation_time descending
        var sortedExhibitions = FirebaseLoader.Exhibitions.OrderByDescending(exhibition => exhibition.creation_time);
        
        foreach (ExhibitionData exhibition in sortedExhibitions) {
            ExhibitionCard card = Instantiate(ExhibitionUIPrefab, defaultLayoutArea).GetComponent<ExhibitionCard>();
            card.Init(exhibition);
        }
    }
    
    public void InitArtists()
    {
        if (FirebaseLoader.Artists == null) return;
        
        ClearStage();
        ShowDefaultLayoutArea(false);

        FetchNewArtists();
        
        ChangeMenuNavigation(MenuNavigation.Artists);
        LayoutRebuilder.ForceRebuildLayoutImmediate(artistsLayoutArea);
    }
    
    private async Task FetchNewArtists()
    {
        if (!FirebaseLoader.ArtistCollectionFull)
        {
            await FirebaseLoader.FetchDocuments<ExhibitionData>(1);
        }
        
        var sortedArtists = FirebaseLoader.Artists.OrderByDescending(artwork => artwork.creation_time);
        foreach (var artist in sortedArtists)
        {
            ArtistContainer container = Instantiate(ArtistUIPrefab, artistsLayoutArea).GetComponent<ArtistContainer>();
            container.Assign(artist);
        }
    }

    public async Task AddNewDocument()
    {
        switch (currentMenuNavigation)
        {
            case MenuNavigation.Artworks:
                if (FirebaseLoader.ArtworkCollectionFull) return;
                
                var artworkDoc = await FirebaseLoader.FetchDocuments<ArtworkData>(1);
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
        
        ApplySorting();
    }

    public void ApplySorting()
    {
        CachedGalleryDisplays = CachedGalleryDisplays.OrderByDescending(display => display.cachedArtwork.creation_time).ToList();

        return;
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
        Debug.LogError("DISABLED OPENING INFO");
        //if (ArTapper.ARPointToPlace != null)
            //arStaticDetails.Open(ArTapper.ARPointToPlace);

        SetBarInActive();
    }

    public void SetBarInActive()
    {
        PlayerPrefs.SetInt("DetailedInfoHelpBar", 1);
        if (InformationHelpBar != null)
            InformationHelpBar.SetActive(false);
    }

    public void OpenDetailedInformation(ArtworkData artwork) 
    {
        artworkDetailsArea?.SetActive(true);
        exhibitionDetailsArea?.SetActive(false);
        artistDetailsArea?.SetActive(false);
        artworkDetailsPanel?.Fill(artwork);
        if (Title) Title.text = artwork.title;
        if (Description) Description.text = artwork.description;
        if (Header) Header.text = artwork.title;
    }

    public void OpenDetailedInformation(ExhibitionData exhibition)
    {
        exhibitionDetailsArea.SetActive(true);
        artworkDetailsArea.SetActive(false);
        artistDetailsArea.SetActive(false);
        exhibitionDetailsPanel.Fill(exhibition);
    }
    
    public void OpenDetailedInformation(ArtistData artist)
    {
        artistDetailsArea.SetActive(true);
        exhibitionDetailsArea.SetActive(false);
        artworkDetailsArea.SetActive(false);
        artistDetailsPanel.Fill(artist);
    }
}
