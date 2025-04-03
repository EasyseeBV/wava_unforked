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
    
    protected override void Close()
    {
        ArtworkUIManager.Instance.InitExhibitions();
        base.Close();
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

        switch(menu)
        {
            case MenuNavigation.Artworks:
                var artworks = await GetArtworks();
                for (int i = 0; i < artworks.Count; i++)
                {
                    ArtworkShower artwork = Instantiate(artworkShowerPrefab, layoutArea);
                    artwork.Init(artworks[i]);
                }
                break;
            case MenuNavigation.Artists:
                var artists = await GetArtists();
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

    public void Fill(ExhibitionData exhibition)
    {
        this.exhibition = exhibition;
        
        Clear();
        
        contentTitleLabel.text = exhibition.title;
        fullLengthDescription = exhibition.description;
        
        TruncateText();
        
        // heartImage.sprite = exhibition.Liked ? likedSprite : unlikedSprite;
        
        ChangeMenu(MenuNavigation.Artworks);
        
        scrollSnapper.Setup();
        ChangeIndicator(0, 0);

        FillImages();
    }

    private async void FillImages()
    {
        try
        {
            var images = await exhibition.GetAllImages();
            foreach (var spr in images)
            {
                Image artworkImage = scrollSnapper.AddToBack(galleryImagePrefab.gameObject).GetComponent<Image>();
                artworkImage.sprite = spr;

                Image indicator = Instantiate(indicatorImage, indicatorArea).GetComponentInChildren<Image>();
                indicator.color = inactiveColor;
                indicators.Add(indicator);
            }
            
            StartCoroutine(LateRebuild());
        }
        catch (Exception e)
        {
            Debug.Log("Failed to fill exhibition image: " + e);
        }
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
            var artistsInArtworks = new List<ArtistData>();
            
            if (exhibition.artworks.Count < exhibition.artwork_references.Count)
            {
                await FirebaseLoader.FillExhibitionArtworkData(exhibition);
            }

            foreach (var artwork in exhibition.artworks)
            {
                foreach (var artist in artwork.artists)
                {
                   if(!artistsInArtworks.Any(artistStored => artist.id == artistStored.id || artist.title == artistStored.title))
                   {
                       artistsInArtworks.Add(artist);
                   }
                }
            }

            Debug.Log("Artist count: " + artistsInArtworks.Count);
            return artistsInArtworks;
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

            Debug.Log("Artwork count: " + exhibition.artworks.Count);
            return exhibition.artworks;
        }
        catch (Exception e)
        {
            Debug.Log("Failed to load artworks: " + e);
            return exhibition.artworks;
        }
    }
}
