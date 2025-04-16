using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DanielLochner.Assets.SimpleScrollSnap;
using Messy.Definitions;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ArtworkDetailsPanel : DetailsPanel
{
    private ArtworkData artwork;

    [Header("Gallery Area")] 
    [SerializeField] private SimpleScrollSnap scrollSnapper;
    [SerializeField] private Transform galleryArea;
    [SerializeField] private GameObject galleryImagePrefab;
    [Space]
    [SerializeField] private Transform indicatorArea;
    [SerializeField] private GameObject indicatorImage;
    [SerializeField] private Color activeColor;
    [SerializeField] private Color inactiveColor;

    [Header("Interactions")]
    [SerializeField] private Button showOnMapButton;
    [SerializeField] private Button developerARTest;
    
    [Header("Artists")] 
    [SerializeField] private Transform artistArea;
    [SerializeField] private ArtistContainer artistContainer;
    
    [Header("Download Button")]
    [SerializeField] private Button downloadButton;
    [SerializeField] private GameObject downloadedCheckmark;

    [Header("Exhibition")]
    [SerializeField] private ExhibitionCard exhibitionCard;

    private List<Image> indicators = new();

    protected override void Setup()
    {
        base.Setup();
        heartButton.onClick.AddListener(LikeArtwork);
        scrollSnapper.OnPanelCentered.AddListener(ChangeIndicator);
        downloadButton.onClick.AddListener(() =>
        {
            DownloadArtwork();
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

    protected override void Close()
    {
        ArtworkUIManager.Instance.InitArtworks();
        base.Close();
    }

    public void Fill(ArtworkData artwork)
    {
        this.artwork = artwork;
        
        Clear();

        for (int i = 0; i < artwork.artists.Count; i++)
        {
            ArtistContainer container = Instantiate(artistContainer, artistArea);
            container.gameObject.SetActive(true);
            container.Assign(artwork.artists[i]);
        }

        GetExhibition();
        
        contentTitleLabel.text = artwork.title;
        fullLengthDescription = artwork.description;
        TruncateText();
        
        downloadedCheckmark.SetActive(false);
        CheckArtworkDownload();
        
        showOnMapButton.onClick.RemoveAllListeners();
        showOnMapButton.onClick.AddListener(() =>
        {
            ARMapPointMaker.SelectedArtwork = artwork;
            SceneManager.LoadScene("Map");
        });
        
        scrollSnapper.Setup();
        ChangeIndicator(0, 0);

        SetImages(artwork);
    }

    private async Task SetImages(ArtworkData _artwork)
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

                    Image indicator = Instantiate(indicatorImage, indicatorArea).GetComponentInChildren<Image>();
                    indicator.color = inactiveColor;
                    indicators.Add(indicator);
                }
                else
                {
                    Debug.LogWarning("A null image was loaded from ArtworkDetailsPanel");
                }
            }

            StartCoroutine(LateRebuild());
        }
        catch (Exception e)
        {
            Debug.Log("Failed to load all ArtworkDetailsImages: " + e);
        }
    }

    private void Clear()
    {
        scrollSnapper.RemoveAll();
        
        foreach (Transform child in artistArea)
        {
            Destroy(child.gameObject);
        }
        
        foreach (Transform child in indicatorArea)
        {
            Destroy(child.gameObject);
        }
        
        indicators.Clear();
        readingMore = false;
        fullLengthDescription = string.Empty;
    }

    private void LikeArtwork()
    {
        if (artwork == null) return;

        // artwork.Liked = !artwork.Liked;
        // heartImage.sprite = artwork.Liked ? likedSprite : unlikedSprite;
    }
    
    private void CheckArtworkDownload()
    {
        bool downloaded = false;

        if (artwork != null && artwork.content_list.Count > 0)
        {
            downloaded = true;
            foreach (var content in artwork.content_list)
            {
                try
                {
                    var uri = new Uri(content.media_content);
                    string encodedPath = uri.AbsolutePath;
                    string decodedPath = Uri.UnescapeDataString(encodedPath);
                    string fileName = Path.GetFileName(decodedPath);
                    string localPath = Path.Combine(AppCache.ContentFolder, fileName);
            
                    // if the file does not exist locally, download it
                    if (!File.Exists(localPath))
                    {
                        downloaded = false;
                    }
                }
                catch (Exception e)
                {
                    Debug.Log("Failed to download content: " + e);
                }
            }   
        }
        
        downloadedCheckmark.SetActive(downloaded);
    }

    private async Task DownloadArtwork()
    {
        if (artwork == null || artwork.content_list.Count <= 0) return;
        
        foreach (var content in artwork.content_list)
        {
            try
            {
                var uri = new Uri(content.media_content);
                string encodedPath = uri.AbsolutePath;
                string decodedPath = Uri.UnescapeDataString(encodedPath);
                string fileName = Path.GetFileName(decodedPath);
                string localPath = Path.Combine(AppCache.ContentFolder, fileName);
            
                // if the file does not exist locally, download it
                if (!File.Exists(localPath))
                {
                    downloadedCheckmark.SetActive(true);
                    await FirebaseLoader.DownloadMedia(AppCache.ContentFolder, content.media_content, null);
                }
                else
                {
                    downloadedCheckmark.SetActive(false);
                    File.Delete(localPath);
                }
            }
            catch (Exception e)
            {
                Debug.Log("Failed to download content: " + e);
            }
        }
    }

    private async void GetExhibition()
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
        if (indicators.Count <= 0) return;
        
        indicators[oldIndex].color = inactiveColor;
        indicators[newIndex].color = activeColor;
    }
}