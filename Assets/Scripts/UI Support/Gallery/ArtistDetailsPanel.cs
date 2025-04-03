using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Messy.Definitions;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class ArtistDetailsPanel : DetailsPanel
{
    private ArtistData artist;
    
    private enum MenuNavigation
    {
        Default,
        Artworks,
        Exhibitions
    }

    [Header("Header info")] 
    [SerializeField] private TextMeshProUGUI locationLabel;
    [SerializeField] private Image profileIcon;

    [Header("Menus")]
    [SerializeField] private Button artworksButton;
    [SerializeField] private TMP_Text artworkText;
    [SerializeField] private Button exhibitionButton;
    [SerializeField] private TMP_Text exhibitionText;
    [SerializeField] private Transform menuBar;
    [Space]
    [SerializeField] private Transform layoutArea;
    [SerializeField] private ArtworkShower artworkShowerPrefab;
    [SerializeField] private ExhibitionCard exhibitionCardPrefab;
    [Space]
    [SerializeField] private Color selectedColor;
    [SerializeField] private Color unselectedColor;

    private MenuNavigation currentMenu = MenuNavigation.Default;

    private const float MENU_BAR_ARTWORKS = -171.5f;
    private const float MENU_BAR_EXHBITIONS = 0;

    protected override void Setup()
    {
        base.Setup();
        heartButton.onClick.AddListener(LikeArtwork);
        artworksButton.onClick.AddListener(() => ChangeMenu(MenuNavigation.Artworks));
        exhibitionButton.onClick.AddListener(() => ChangeMenu(MenuNavigation.Exhibitions));
    }
    
    protected override void Close()
    {
        ArtworkUIManager.Instance.InitArtists();
        base.Close();
    }

    private void ChangeMenu(MenuNavigation menu)
    {
        if (currentMenu == menu) return;

        menuBar.transform.localPosition = new Vector3(
            menu == MenuNavigation.Artworks ? MENU_BAR_ARTWORKS : MENU_BAR_EXHBITIONS,
            menuBar.transform.localPosition.y,
            menuBar.transform.localPosition.z);

        artworkText.color = menu == MenuNavigation.Artworks ? selectedColor : unselectedColor;
        exhibitionText.color = menu == MenuNavigation.Exhibitions ? selectedColor : unselectedColor;

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
            case MenuNavigation.Exhibitions:
                var exhibitions = GetExhibitions();
                for (int i = 0; i < exhibitions.Count; i++)
                {
                    ExhibitionCard exhibition = Instantiate(exhibitionCardPrefab, layoutArea);
                    exhibition.Init(exhibitions[i]);
                }
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(menu), menu, null);
        }
        
        currentMenu = menu;
        
        StartCoroutine(LateRebuild());
    }
    
    public async void Fill(ArtistData artist)
    {
        this.artist = artist;
        
        Clear();

        contentTitleLabel.text = artist.title;
        profileIcon.sprite = await artist.GetIcon();
        fullLengthDescription = artist.description;
        TruncateText();
        
        // heartImage.sprite = artist.Liked ? likedSprite : unlikedSprite;
        
        ChangeMenu(MenuNavigation.Artworks);
        
        StartCoroutine(LateRebuild());
    }

    private void Clear()
    {
        foreach (Transform child in layoutArea)
        {
            Destroy(child.gameObject);
        }

        currentMenu = MenuNavigation.Default;
        readingMore = false;
        fullLengthDescription = string.Empty;
    }

    private void LikeArtwork()
    {
        if (artist == null) return;

        // artist.Liked = !artist.Liked;
        // heartImage.sprite = artist.Liked ? likedSprite : unlikedSprite;
    }

    private List<ExhibitionData> GetExhibitions()
    {
        List<ExhibitionData> exhibitions = new();
        var artworks = GetArtworks();

        foreach (var exhibition in FirebaseLoader.Exhibitions)
        {
            foreach (var artwork in artworks)
            {
                if (exhibition.artworks.Contains(artwork))
                {
                    if (!exhibitions.Contains(exhibition)) exhibitions.Add(exhibition);
                    continue;
                }

                if (exhibition.artwork_references.Any(artworkRef => artworkRef.Id == artwork.id))
                {
                    if (!exhibitions.Contains(exhibition)) exhibitions.Add(exhibition);
                    continue;
                }
                
                if (exhibition.artworks.Any(artworkRef => artworkRef.title == artwork.title))
                {
                    if (!exhibitions.Contains(exhibition)) exhibitions.Add(exhibition);
                }
            }
        }

        return exhibitions;
    }

    private List<ArtworkData> GetArtworks()
    {
        List<ArtworkData> works = new();
        
        foreach (var artwork in FirebaseLoader.Artworks)
        {
            if (artwork.artists.Contains(artist))
            {
                works.Add(artwork);
                continue;
            }

            if (artwork.artists.Any(artistData => artistData.id == artist.id || artistData.title == artist.title))
            {
                works.Add(artwork);
            }
        }

        return works;
    }
}
