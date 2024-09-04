using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DanielLochner.Assets.SimpleScrollSnap;
using Messy.Definitions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ExhibitionDetailsPanel : DetailsPanel
{
    private ExhibitionSO exhibition;
    
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

    private void ChangeMenu(MenuNavigation menu)
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
                var artworks = GetArtworks();
                for (int i = 0; i < artworks.Count; i++)
                {
                    ArtworkShower artwork = Instantiate(artworkShowerPrefab, layoutArea);
                    artwork.Init(artworks[i]);
                }
                break;
            case MenuNavigation.Artists:
                var artists = GetArtists();
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

    public void Fill(ExhibitionSO exhibition)
    {
        this.exhibition = exhibition;
        
        Clear();

        for (int i = 0; i < exhibition.ExhibitionImages.Count; i++)
        {
            Image artworkImage = scrollSnapper.AddToBack(galleryImagePrefab.gameObject).GetComponent<Image>();
            artworkImage.sprite = exhibition.ExhibitionImages[i];

            Image indicator = Instantiate(indicatorImage, indicatorArea).GetComponentInChildren<Image>();
            indicator.color = inactiveColor;
            indicators.Add(indicator);
        }
        
        contentTitleLabel.text = exhibition.Title;
        fullLengthDescription = exhibition.Description;
        
        TruncateText();
        
        heartImage.sprite = exhibition.Liked ? likedSprite : unlikedSprite;
        
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
        if (!exhibition) return;

        exhibition.Liked = !exhibition.Liked;
        heartImage.sprite = exhibition.Liked ? likedSprite : unlikedSprite;
    }

    private void ChangeIndicator(int newIndex,int oldIndex)
    {
        if (indicators.Count <= 0) return;
        
        indicators[oldIndex].color = inactiveColor;
        indicators[newIndex].color = activeColor;
    }
    
    private List<ArtistSO> GetArtists()
    {
        List<ArtistSO> artists = new();
        foreach (var artwork in exhibition.ArtWorks)
        {
            foreach (var artist in artwork.Artists)
            {
                if (artists.Contains(artist)) continue;
                artists.Add(artist);
            }
        }

        return artists;
    }

    private List<ARPointSO> GetArtworks()
    {
        return exhibition.ArtWorks;
    }
}
