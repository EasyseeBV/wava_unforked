using DanielLochner.Assets.SimpleScrollSnap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ExhibitionDetailsPanel : MonoBehaviour
{
    private ExhibitionData exhibition;
    
    private enum MenuNavigation
    {
        Default,
        Artworks,
        Artists
    }

    [SerializeField] private List<RectTransform> rebuildLayout;
    [SerializeField] private Button closeButton;
    [SerializeField] private TextMeshProUGUI exhibitionTitleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI durationText;
    [SerializeField] private TextMeshProUGUI locationText;
    [SerializeField] private Button downloadButton;
    [SerializeField] private DownloadButtonUI downloadButtonUI;
    [SerializeField] private UnderlinedSelectionUI ArtworksArtistMenu;

    [Header("Image gallery")] 
    [SerializeField] private SimpleScrollSnap scrollSnapper;
    [SerializeField] private GameObject galleryImagePrefab;
    [SerializeField] private PointsAndLineUI galleryIndicator;
    
    [Header("Menus")]
    [SerializeField] private Button artworksButton;
    [SerializeField] private Button artistsButton;
    [SerializeField] private Transform artworksAndArtistsContainer;
    [SerializeField] private ArtworkShower artworkShowerPrefab;
    [SerializeField] private ArtistContainer artistContainerPrefab;
    
    private MenuNavigation currentMenu = MenuNavigation.Default;

    void Awake()
    {
        closeButton.onClick.AddListener(() => gameObject.SetActive(false));

        downloadButton.onClick.AddListener(() => {

            downloadButtonUI.ShowAsDownloading();
            downloadButton.interactable = false;

            // Create intermediate variable for following callback.
            var callbackExhibition = exhibition;

            _ = DownloadManager.DownloadExhibition(exhibition, (_) =>
            {
                if (exhibition != callbackExhibition)
                    return;

                downloadButtonUI.ShowAsDownloading();
                downloadButton.interactable = false;

            }, (result) =>
            {
                if (exhibition != callbackExhibition)
                    return;

                if (result == UnityWebRequest.Result.Success)
                {
                    downloadButtonUI.ShowAsDownloadFinished();
                    downloadButton.interactable = false;
                }
                else
                {
                    downloadButtonUI.ShowAsReadyForDownload();
                    downloadButton.interactable = true;
                }

                ArtworkUIManager.Instance.UpdateCardDownloadStatusForExhibition(callbackExhibition);
            });
        });

        artworksButton.onClick.AddListener(() => ChangeMenu(MenuNavigation.Artworks));
        artistsButton.onClick.AddListener(() => ChangeMenu(MenuNavigation.Artists));
        scrollSnapper.OnPanelCentered.AddListener(ChangeIndicator);
    }

    private async void ChangeMenu(MenuNavigation menu)
    {
        if (currentMenu == menu)
            return;

        foreach (Transform child in artworksAndArtistsContainer)
        {
            Destroy(child.gameObject);
        }

        switch(menu)
        {
            case MenuNavigation.Artworks:
                var artworks = await GetArtworks();
                for (int i = 0; i < artworks.Count; i++)
                {
                    ArtworkShower artwork = Instantiate(artworkShowerPrefab, artworksAndArtistsContainer);
                    artwork.Init(artworks[i], true);
                }
                ArtworksArtistMenu.ShowAsSelected(0);

                break;
            case MenuNavigation.Artists:
                var artists = await GetArtists();
                for (int i = 0; i < artists.Count; i++)
                {
                    ArtistContainer container = Instantiate(artistContainerPrefab, artworksAndArtistsContainer);
                    container.Assign(artists[i]);
                }
                ArtworksArtistMenu.ShowAsSelected(1);

                break;
        }
        
        currentMenu = menu;


        // Rebuild layout.
        for (int i = 0; i < rebuildLayout.Count; i++)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(rebuildLayout[i]);
        }

        this.InvokeNextFrame(() =>
        {
            for (int i = 0; i < rebuildLayout.Count; i++)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(rebuildLayout[i]);
            }
        });
    }

    public void Fill(ExhibitionData exhibition)
    {
        this.exhibition = exhibition;
        
        Clear();

        exhibitionTitleText.text = exhibition.title;
        durationText.text = exhibition.year.ToString();
        locationText.text = exhibition.location.ToString();

        descriptionText.text = exhibition.description;
        


        ChangeMenu(MenuNavigation.Artworks);
        ArtworksArtistMenu.FinishAnimationsImmediately();


        scrollSnapper.Setup();
        ChangeIndicator(0, 0);

        FillImages();


        galleryIndicator.SetPointCount(scrollSnapper.NumberOfPanels);
        galleryIndicator.SetSelectedPointIndex(0);
        galleryIndicator.FinishAnimationsImmediately();


        // Update appearance of download button.
        if (DownloadManager.ExhibitionIsDownloaded(exhibition))
        {
            downloadButtonUI.ShowAsDownloadFinished();
            downloadButton.interactable = false;
        }
        else
        {
            downloadButtonUI.ShowAsReadyForDownload();
            downloadButton.interactable = true;
        }


        // Rebuild layout.
        for (int i = 0; i < rebuildLayout.Count; i++)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(rebuildLayout[i]);
        }

        this.InvokeNextFrame(() =>
        {
            for (int i = 0; i < rebuildLayout.Count; i++)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(rebuildLayout[i]);
            }
        });
    }

    private async void FillImages()
    {
        try
        {
            var images = await exhibition.GetAllImages();
            foreach (var spr in images)
            {
                Image artworkImage = scrollSnapper.AddToBack(galleryImagePrefab.gameObject).GetComponentInChildren<Image>();
                artworkImage.sprite = spr;
                var aspectRatioFitter = artworkImage.GetComponent<AspectRatioFitter>();
                var aspectRatio = spr.rect.width / spr.rect.height;
                aspectRatioFitter.aspectRatio = aspectRatio;
            }
        }
        catch (Exception e)
        {
            Debug.Log("Failed to fill exhibition image: " + e);
        }
    }

    private void Clear()
    {
        scrollSnapper.RemoveAll();
        
        foreach (Transform child in artworksAndArtistsContainer)
        {
            Destroy(child.gameObject);
        }
        
        currentMenu = MenuNavigation.Default;
    }

    private void ChangeIndicator(int newIndex,int oldIndex)
    {
        galleryIndicator.SetSelectedPointIndex(newIndex);
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
