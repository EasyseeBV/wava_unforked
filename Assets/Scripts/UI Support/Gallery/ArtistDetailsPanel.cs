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
        if (this.artist.iconImage == null)
        {
            await FirebaseLoader.LoadArtworkImages(artist);
        }
        
        profileIcon.sprite = artist.iconImage;
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
        if (FirebaseLoader.Exhibitions == null) return null;

        List<ExhibitionData> exhibitions = new();
        
        foreach (var exhibition in FirebaseLoader.Exhibitions)
        {
            if (exhibition.artists.Contains(artist) && !exhibitions.Contains(exhibition))
            {
                exhibitions.Add(exhibition);
            }

            foreach (var artwork in exhibition.artworks.Where(artwork => artwork.artists.Contains(artist) && !exhibitions.Contains(exhibition)))
            {
                exhibitions.Add(exhibition);
            }
        }

        return exhibitions;
    }

    private List<ArtworkData> GetArtworks()
    {
        return FirebaseLoader.Artworks == null ? null : FirebaseLoader.Artworks.Where(artwork => artwork.artists.Contains(artist)).ToList();
    }
}
