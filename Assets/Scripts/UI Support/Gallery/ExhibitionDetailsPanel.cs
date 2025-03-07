using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DanielLochner.Assets.SimpleScrollSnap;
using Messy.Definitions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ExhibitionDetailsPanel : DetailsPanel
{
    private ExhibitionData exhibition;
    
    private enum MenuNavigation
    {
        Default,
        Artworks,
        Artists
    }
    
    [Header("Gallery Area")] 
    [SerializeField] private SimpleScrollSnap scrollSnapper;
    [SerializeField] private Transform galleryArea;
    [SerializeField] private Image galleryImagePrefab;
    [Space]
    [SerializeField] private Transform indicatorArea;
    [SerializeField] private GameObject indicatorImage;
    [SerializeField] private Color activeColor;
    [SerializeField] private Color inactiveColor;
    
    [Header("Menus")]
    [SerializeField] private Button artworksButton;
    [SerializeField] private TMP_Text artworkText;
    [SerializeField] private Button artistsButton;
    [SerializeField] private TMP_Text artistsText;
    [SerializeField] private Transform menuBar;
    [Space]
    [SerializeField] private Transform layoutArea;
    [SerializeField] private ArtworkShower artworkShowerPrefab;
    [SerializeField] private ArtistContainer artistContainer;
    [Space]
    [SerializeField] private Color selectedColor;
    [SerializeField] private Color unselectedColor;
    
    private MenuNavigation currentMenu = MenuNavigation.Default;

    private const float MENU_BAR_ARTWORKS = -171.5f;
    private const float MENU_BAR_ARTISTS = 0;

    private List<Image> indicators = new();

    protected override void Setup()
    {
        base.Setup();
        heartButton.onClick.AddListener(LikeArtwork);
        artworksButton.onClick.AddListener(() => ChangeMenu(MenuNavigation.Artworks));
        artistsButton.onClick.AddListener(() => ChangeMenu(MenuNavigation.Artists));
        scrollSnapper.OnPanelCentered.AddListener(ChangeIndicator);
    }

    private async void ChangeMenu(MenuNavigation menu)
    {
        if (currentMenu == menu) return;

        menuBar.transform.localPosition = new Vector3(
            menu == MenuNavigation.Artworks ? MENU_BAR_ARTWORKS : MENU_BAR_ARTISTS,
            menuBar.transform.localPosition.y,
            menuBar.transform.localPosition.z);

        artworkText.color = menu == MenuNavigation.Artworks ? selectedColor : unselectedColor;
        artistsText.color = menu == MenuNavigation.Artists ? selectedColor : unselectedColor;

        foreach (Transform child in layoutArea)
        {
            Destroy(child.gameObject);
        }

        Debug.Log("switching menu too " + menu);
        switch(menu)
        {
            case MenuNavigation.Artworks:
                var artworks = await GetArtworks();
                Debug.Log("got artworks: " + artworks.Count);
                for (int i = 0; i < artworks.Count; i++)
                {
                    ArtworkShower artwork = Instantiate(artworkShowerPrefab, layoutArea);
                    artwork.Init(artworks[i]);
                }
                break;
            case MenuNavigation.Artists:
                var artists = await GetArtists();
                Debug.Log("got artist: " + artists.Count);
                for (int i = 0; i < artists.Count; i++)
                {
                    ArtistContainer container = Instantiate(artistContainer, layoutArea);
                    container.Assign(artists[i]);
                }
                break;
        }
        
        currentMenu = menu;
        
        StartCoroutine(LateRebuild());
    }

    public async void Fill(ExhibitionData exhibition)
    {
        this.exhibition = exhibition;
        
        Clear();

        if (exhibition.images == null || exhibition.images.Count == 0)
        {
            await FirebaseLoader.LoadArtworkImages(exhibition);
            
        }

        for (int i = 0; i < exhibition.images?.Count; i++)
        {
            Image artworkImage = scrollSnapper.AddToBack(galleryImagePrefab.gameObject).GetComponent<Image>();
            artworkImage.sprite = exhibition.images[i];

            Image indicator = Instantiate(indicatorImage, indicatorArea).GetComponentInChildren<Image>();
            indicator.color = inactiveColor;
            indicators.Add(indicator);
        }
        
        contentTitleLabel.text = exhibition.title;
        fullLengthDescription = exhibition.description;
        
        TruncateText();
        
        // heartImage.sprite = exhibition.Liked ? likedSprite : unlikedSprite;
        
        ChangeMenu(MenuNavigation.Artworks);
        
        StartCoroutine(LateRebuild());
        scrollSnapper.Setup();
        ChangeIndicator(0, 0);
    }

    private void Clear()
    {
        scrollSnapper.RemoveAll();
        
        foreach (Transform child in indicatorArea)
        {
            Destroy(child.gameObject);
        }
        
        foreach (Transform child in layoutArea)
        {
            Destroy(child.gameObject);
        }
        
        currentMenu = MenuNavigation.Default;
        indicators.Clear();
        readingMore = false;
        fullLengthDescription = string.Empty;
    }

    private void LikeArtwork()
    {
        if (exhibition == null) return;

        // exhibition.Liked = !exhibition.Liked;
        // heartImage.sprite = exhibition.Liked ? likedSprite : unlikedSprite;
    }

    private void ChangeIndicator(int newIndex,int oldIndex)
    {
        if (indicators.Count <= 0) return;
        
        indicators[oldIndex].color = inactiveColor;
        indicators[newIndex].color = activeColor;
    }
    
    private async Task<List<ArtistData>> GetArtists()
    {
        try
        {
            var _artists = new List<ArtistData>();
            foreach (var artist in FirebaseLoader.Artists)
            {
                if (exhibition.artists.Contains(artist) && !_artists.Contains(artist))
                {
                    _artists.Add(artist);
                }

                if (exhibition.artist_references.Any(docRef => docRef.Id == artist.artist_id) &&
                    !_artists.Contains(artist))
                {
                    _artists.Add(artist);
                    exhibition.artists.Add(artist);
                }
            }

            return _artists;
        }
        catch (Exception e)
        {
            Debug.Log("Could not load artists: " + e);
            
            return exhibition.artists;
        }
    }

    private async Task<List<ArtworkData>> GetArtworks()
    {
        try
        {
            if (exhibition.artworks.Count < exhibition.artwork_references.Count)
            {
                await FirebaseLoader.FillExhibitionArtworkData(exhibition);
            }

            return exhibition.artworks;
        }
        catch (Exception e)
        {
            Debug.Log("Failed to load artworks: " + e);
            return exhibition.artworks;
        }
    }
}
