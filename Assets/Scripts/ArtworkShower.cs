using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class ArtworkShower : MonoBehaviour
{
    public Image ArtworkImage;
    public AspectRatioFitter ArtworkAspectRatioFitter;
    public TextMeshProUGUI ArtworkTitleText;
    public TextMeshProUGUI ArtistNameText;
    public TextMeshProUGUI YearText;
    public Button ExhibitionButton;
    public Button ViewArtworkButton;
    [Space]
    public TextSlider ExhibitionTitleTextSlider;
    [SerializeField] private LoadingCircle loadingCircle;

    [Header("Download status")]
    [SerializeField] Image downloadStatusImage;
    [SerializeField] Color defaultColor;
    [SerializeField] Color downloadedColor;

    [Header("Archived-related")]
    [SerializeField] List<CanvasGroup> canvasGroups;
    [SerializeField] GameObject archivedOverlay;
    [SerializeField] float archivedAlpha;

    public bool IsLoading { get; set; }
    
    public ArtworkData cachedArtwork { get; set; }

    private void Awake()
    {
        ViewArtworkButton.onClick.AddListener(OpenDetails);
        ExhibitionButton.onClick.AddListener(OpenDetails);
        loadingCircle.gameObject.SetActive(false);
    }

    public void Init(ArtworkData artwork, bool loadImage) 
    {
        if (artwork == null)
        {
            gameObject.SetActive(false);
            return;
        }
        
        ArtworkTitleText.text = artwork.title;
        ArtistNameText.text = artwork.artists.Count > 0 ? artwork.artists[0].title : null;

        YearText.text = artwork.year.ToString();

        foreach (var exhibition in FirebaseLoader.Exhibitions.Where(exhibition => exhibition.artworks.Contains(artwork)))
        {
            ExhibitionTitleTextSlider.SetTextAndResetAnimation(exhibition.title);
            break;
        }
        
        cachedArtwork = artwork;

        if (loadImage) SetImage(artwork);


        // Show if artwork is archived.
        bool isArchived = false;

        archivedOverlay.SetActive(isArchived);

        foreach (var canvasGroup in canvasGroups)
        {
            canvasGroup.alpha = isArchived ? archivedAlpha : 1f;
        }


        UpdateDownloadStatus();
    }

    public void SetImage()
    {
        Debug.Log("Loading image...");
        
        if (ArtworkImage.sprite != null) return;
        
        IsLoading = true;
        loadingCircle.gameObject.SetActive(true);
        loadingCircle.BeginLoading();
        SetImage(cachedArtwork);
    }
    
    public void UpdateDownloadStatus()
    {
        if (cachedArtwork == null)
            return;

        if (DownloadManager.ArtworkIsDownloaded(cachedArtwork))
            downloadStatusImage.color = downloadedColor;
        else
            downloadStatusImage.color = defaultColor;
    }

    private async Task SetImage(ArtworkData artwork)
    {
        try
        {
            if (artwork.artwork_image_references.Count > 0)
            {
                var images = await artwork.GetImages(1);

                if (images.Count <= 0)
                {
                    Debug.Log($"Removed artwork from display, could not get any images: OfflineMode status: [{FirebaseLoader.OfflineMode}]");
                    ArtworkImage.sprite = null;
                    gameObject.SetActive(false);
                    return;
                }
                
                loadingCircle.StopLoading();
                ArtworkImage.sprite = images.Count > 0 ? images[0] : null;

                if (ArtworkImage.sprite != null)
                {
                    var imageAspectRatio = ArtworkImage.sprite.rect.width / ArtworkImage.sprite.rect.height;

                    ArtworkAspectRatioFitter.aspectRatio = imageAspectRatio;
                }
                else
                {
                    Debug.Log($"Removed artwork [{artwork.title}] from gallery as it's image failed to load");
                }
            }
            else
            {
                Debug.Log($"Removed artwork from display, could not get any images: OfflineMode status: [{FirebaseLoader.OfflineMode}]");
                ArtworkImage.sprite = null;
                gameObject.SetActive(false);
            }
        }
        catch (Exception e)
        {
            ArtworkImage.sprite = null;
            Debug.Log($"Failed to set ArtworkShower image: {e} | OfflineMode status: [{FirebaseLoader.OfflineMode}]");
            gameObject.SetActive(false);
        }
    }

    private void OpenDetails()
    {
        if (cachedArtwork == null) return;
        
        if(ArtworkUIManager.Instance != null)
            ArtworkUIManager.Instance.OpenDetailedInformation(cachedArtwork);
        else
        {
            ArtworkUIManager.SelectedArtwork = cachedArtwork;
            SceneManager.LoadScene("Exhibition&Art");
        }
    }
}
