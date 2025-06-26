using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ArtistDetailsPanel : MonoBehaviour
{
    private ArtistData artist;
    
    private enum MenuNavigation
    {
        Default,
        Artworks,
        Exhibitions
    }

    [Header("Header info")]
    [SerializeField] private TextMeshProUGUI artistNameText;
    [SerializeField] private TextMeshProUGUI locationLabel;
    [SerializeField] private Image profileIcon;
    [SerializeField] private AspectRatioFitter profileAspectRatio;

    [Header("Mid section")]
    [SerializeField] private TextMeshProUGUI descriptionText;

    [Header("Menu")]
    [SerializeField] private Button artworksButton;
    [SerializeField] private Button exhibitionButton;
    [SerializeField] private UnderlinedSelectionUI underlinedSelectionUI;
    [Space]
    [SerializeField] private Transform artworksAndExhibitionsContainer;
    [SerializeField] private ArtworkShower artworkShowerPrefab;
    [SerializeField] private ExhibitionCard exhibitionCardPrefab;

    [Header("Other references")]
    [SerializeField] private Button closeButton;
    [SerializeField] private List<RectTransform> rebuildLayout;

    [SerializeField] private HorizontalSwipeDetector swipeDetector;

    private MenuNavigation currentMenu = MenuNavigation.Default;

    void Awake()
    {
        closeButton.onClick.AddListener(() => gameObject.SetActive(false));

        artworksButton.onClick.AddListener(() => ChangeMenu(MenuNavigation.Artworks));
        exhibitionButton.onClick.AddListener(() => ChangeMenu(MenuNavigation.Exhibitions));
    }

    private void OnEnable()
    {
        swipeDetector.SwipedLeft += OnSwipedLeft;
        swipeDetector.SwipedRight += OnSwipedRight;
    }

    private void OnDisable()
    {
        swipeDetector.SwipedLeft -= OnSwipedLeft;
        swipeDetector.SwipedRight -= OnSwipedRight;
    }

    void OnSwipedLeft(Vector2 startPosition)
    {
        // Check if touch was performed above container.
        if (!RectTransformUtility.RectangleContainsScreenPoint(artworksAndExhibitionsContainer as RectTransform, startPosition))
            return;

        if (currentMenu == MenuNavigation.Artworks)
            ChangeMenu(MenuNavigation.Exhibitions);
    }

    void OnSwipedRight(Vector2 startPosition)
    {
        // Check if touch was performed above container.
        if (!RectTransformUtility.RectangleContainsScreenPoint(artworksAndExhibitionsContainer as RectTransform, startPosition))
            return;

        if (currentMenu == MenuNavigation.Exhibitions)
            ChangeMenu(MenuNavigation.Artworks);
    }

    private void ChangeMenu(MenuNavigation menu)
    {
        if (currentMenu == menu)
            return;

        foreach (Transform child in artworksAndExhibitionsContainer)
        {
            Destroy(child.gameObject);
        }
        
        switch(menu)
        {
            case MenuNavigation.Artworks:
                var artworks = GetArtworks();
                for (int i = 0; i < artworks.Count; i++)
                {
                    ArtworkShower artwork = Instantiate(artworkShowerPrefab, artworksAndExhibitionsContainer);
                    artwork.Init(artworks[i], true);
                }
                underlinedSelectionUI.ShowAsSelected(0);

                break;
            case MenuNavigation.Exhibitions:
                var exhibitions = GetExhibitions();
                for (int i = 0; i < exhibitions.Count; i++)
                {
                    ExhibitionCard exhibition = Instantiate(exhibitionCardPrefab, artworksAndExhibitionsContainer);
                    exhibition.Init(exhibitions[i]);
                }
                underlinedSelectionUI.ShowAsSelected(1);

                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(menu), menu, null);
        }
        
        currentMenu = menu;


        // Rebuild layout.
        this.InvokeNextFrame(() =>
        {
            for (int i = 0; i < rebuildLayout.Count; i++)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(rebuildLayout[i]);
            }
        });
    }
    
    public async void Fill(ArtistData artist)
    {
        this.artist = artist;
        
        Clear();

        artistNameText.text = artist.title;
        descriptionText.text = artist.description;
        profileIcon.sprite = await artist.GetIcon();

        // Update aspect ratio of artist photo.
        var texture = profileIcon.sprite.texture;
        var aspectRatio = texture.width / (float) texture.height;
        profileAspectRatio.aspectRatio = aspectRatio;


        ChangeMenu(MenuNavigation.Artworks);

        underlinedSelectionUI.Setup();

        underlinedSelectionUI.FinishAnimationsImmediately();
    }

    private void Clear()
    {
        foreach (Transform child in artworksAndExhibitionsContainer)
        {
            Destroy(child.gameObject);
        }

        currentMenu = MenuNavigation.Default;
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
