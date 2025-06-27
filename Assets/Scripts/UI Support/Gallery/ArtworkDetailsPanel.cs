using DanielLochner.Assets.SimpleScrollSnap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static OnlineMapsZipDecompressor;

public class ArtworkDetailsPanel : MonoBehaviour
{
    private ArtworkData artwork;

    [SerializeField] TextMeshProUGUI artworkTitleText;
    [SerializeField] TextMeshProUGUI artistNameText;
    [SerializeField] TextMeshProUGUI descriptionText;

    [SerializeField] Button closeButton;

    [SerializeField] protected List<RectTransform> rebuildLayout;

    [Header("Gallery Area")] 
    [SerializeField] private SimpleScrollSnap scrollSnapper;
    [SerializeField] private GameObject galleryImagePrefab;
    [SerializeField] private PointsAndLineUI pointsAndLineUI;

    [Header("Interactions")]
    [SerializeField] private Button showOnMapButton;
    [SerializeField] private Button developerARTest;
    
    [Header("Artists")] 
    [SerializeField] private Transform artistArea;
    [SerializeField] private ArtistContainer artistContainerPrefab;
    
    [Header("Download Button")]
    [SerializeField] private Button downloadButton;
    [SerializeField] private DownloadButtonUI downloadButtonUI;

    [Header("Exhibition")]
    [SerializeField] private ExhibitionCard exhibitionCard;

    void Awake()
    {
        closeButton.onClick.AddListener(() => gameObject.SetActive(false));

        scrollSnapper.OnPanelCentered.AddListener(ChangeIndicator);

        downloadButton.onClick.AddListener(() =>
        {
            downloadButtonUI.ShowAsDownloading();
            downloadButton.interactable = false;

            // Create intermediate variable for following callback.
            var callbackArtwork = artwork;

            _ = DownloadManager.DownloadArtwork(artwork, (progress) =>
            {
                if (artwork != callbackArtwork)
                    return;

                downloadButtonUI.ShowAsDownloading();
                downloadButton.interactable = false;

            }, (result) =>
            {
                if (artwork != callbackArtwork)
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

                ArtworkUIManager.Instance.UpdateCardDownloadStatusForArtwork(callbackArtwork);
            });
        });
        
        if (AppSettings.DeveloperMode)
        {
            developerARTest.gameObject.SetActive(true);
            developerARTest.onClick.AddListener(() =>
            {
                ArTapper.ArtworkToPlace = artwork;
                ArTapper.DistanceWhenActivated = 2f;
                SceneManager.LoadSceneAsync(string.IsNullOrEmpty(artwork.alt_scene) ? "ARView" : artwork.alt_scene);
            });
        }
    }

    public void Fill(ArtworkData artwork)
    {
        this.artwork = artwork;

        artworkTitleText.text = artwork.title;
        artistNameText.text = artwork.artists.Count > 0 ? artwork.artists[0].title : string.Empty;
        descriptionText.text = artwork.description;


        // Update artists list.
        scrollSnapper.RemoveAll();

        foreach (Transform child in artistArea)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < artwork.artists.Count; i++)
        {
            ArtistContainer container = Instantiate(artistContainerPrefab, artistArea);
            container.gameObject.SetActive(true);
            container.Assign(artwork.artists[i]);
        }


        showOnMapButton.onClick.RemoveAllListeners();
        showOnMapButton.onClick.AddListener(() =>
        {
            ARMapPointMaker.SelectedArtwork = artwork;
            SceneManager.LoadScene("Map");
        });


        SetupGalleryImages(artwork);

        scrollSnapper.Setup();
        ChangeIndicator(0, 0);

        SetupExhibitionCard();


        // Update appearance of download button.
        if (DownloadManager.ArtworkIsDownloaded(artwork))
        {
            downloadButtonUI.ShowAsDownloadFinished();
            downloadButton.interactable = false;
        }
        else
        {
            downloadButtonUI.ShowAsReadyForDownload();
            downloadButton.interactable = true;
        }

        // - If the download is in progress then the callback will show the button as downloading.


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

    private async Task SetupGalleryImages(ArtworkData _artwork)
    {
        try
        {
            var images = await _artwork.GetAllImages();
            foreach (var spr in images)
            {
                if (spr != null)
                {
                    Image artworkImage = scrollSnapper.AddToBack(galleryImagePrefab.gameObject).GetComponentInChildren<Image>();
                    artworkImage.sprite = spr;
                    var aspectRatioFitter = artworkImage.GetComponent<AspectRatioFitter>();
                    var aspectRatio = spr.rect.width / spr.rect.height;
                    aspectRatioFitter.aspectRatio = aspectRatio;
                }
                else
                {
                    Debug.LogWarning("A null image was loaded from ArtworkDetailsPanel");
                }
            }

            // Set number of indicator points.
            pointsAndLineUI.SetPointCount(images.Count);
            pointsAndLineUI.SetSelectedPointIndex(0);
            pointsAndLineUI.FinishAnimationsImmediately();
        }
        catch (Exception e)
        {
            Debug.Log("Failed to load all ArtworkDetailsImages: " + e);
        }
    }

    private async void SetupExhibitionCard()
    {
        if (FirebaseLoader.Exhibitions == null) return;

        try
        {
            foreach (var exhibition in FirebaseLoader.Exhibitions)
            {
                if (exhibition.artworks.Contains(artwork))
                {
                    exhibitionCard.Init(exhibition);
                    return;
                }
                else
                {
                    if (exhibition.artwork_references.Any(documentReference => documentReference.Id == artwork.id))
                    {
                        exhibition.artworks.Add(artwork);
                        exhibitionCard.Init(exhibition);
                        return;
                    }
                }
            }

            exhibitionCard.Init(await FirebaseLoader.FindRelatedExhibition(artwork.id));
        }
        catch (Exception e)
        {
            Debug.LogWarning("Could not load exhibition: " + e.StackTrace);
        }
    }

    private void ChangeIndicator(int newIndex,int oldIndex)
    {
        pointsAndLineUI.SetSelectedPointIndex(newIndex);
    }
}