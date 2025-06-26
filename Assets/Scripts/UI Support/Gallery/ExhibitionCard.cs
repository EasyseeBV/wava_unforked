using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class ExhibitionCard : MonoBehaviour
{
    [HideInInspector] public ExhibitionData exhibition;
    
    [Header("Single cover image")]
    [SerializeField] GameObject singleCoverImageContainer;
    [SerializeField] Image singleImage;
    [SerializeField] AspectRatioFitter singleImageAspect;

    [Header("Multiple cover images")]
    [SerializeField] GameObject multipleCoverImagesContainer;
    [SerializeField] Image mainImage;
    [SerializeField] AspectRatioFitter mainImageAspect;
    [SerializeField] Image topImage;
    [SerializeField] AspectRatioFitter topImageAspect;
    [SerializeField] Image bottomImage;
    [SerializeField] AspectRatioFitter bottomImageAspect;

    [Header("Text references")]
    [SerializeField] TextMeshProUGUI exhibitionTitleText;
    [SerializeField] TextMeshProUGUI yearText;
    [SerializeField] TextMeshProUGUI locationText;

    [Header("Download status")]
    [SerializeField] Image downloadStatusImage;
    [SerializeField] Color defaultColor;
    [SerializeField] Color downloadedColor;

    [Header("Other references")]
    [SerializeField] Button viewExhibitionButton;
    [SerializeField] LoadingCircle loadingCircle;

    protected virtual void Awake()
    {
        if (viewExhibitionButton)
            viewExhibitionButton.onClick.AddListener(OpenExhibitionPage);
        
        loadingCircle?.BeginLoading();
    }

    public async void Init(ExhibitionData point)
    {
        if (point == null)
        {
            Debug.Log("Provided exhibiton data was empty");
            gameObject.SetActive(false);
            return;
        }

        try
        {
            exhibition = point;
            exhibitionTitleText.text = point.title;
            locationText.text = point.year + " · " + point.location;

            if (point.exhibition_image_references.Count >= 3)
            {
                singleCoverImageContainer.SetActive(false);
                multipleCoverImagesContainer.SetActive(true);

                var images = await point.GetImages(3);
                
                mainImage.sprite = images.Count >= 0 ? images[0] : null;
                topImage.sprite = images.Count >= 1 ? images[1] : null;
                bottomImage.sprite = images.Count >= 2 ? images[2] : null;


                // Update aspect ratios.
                if (mainImage.sprite != null)
                {
                    var width = mainImage.sprite.rect.width;
                    var height = mainImage.sprite.rect.height;
                    mainImageAspect.aspectRatio = width / height;
                }

                if (topImage.sprite != null)
                {
                    var width = topImage.sprite.rect.width;
                    var height = topImage.sprite.rect.height;
                    topImageAspect.aspectRatio = width / height;
                }

                if (bottomImage.sprite != null)
                {
                    var width = bottomImage.sprite.rect.width;
                    var height = bottomImage.sprite.rect.height;
                    bottomImageAspect.aspectRatio = width / height;
                }
            }
            else if (point.artworks.Count >= 3 && point.artworks[0].artwork_image_references.Count > 0 
                                               && point.artworks[1].artwork_image_references.Count > 0 
                                               && point.artworks[2].artwork_image_references.Count > 0)
            {
                singleCoverImageContainer.SetActive(false);
                multipleCoverImagesContainer.SetActive(true);

                // Set sprites.
                var mainSprites = await point.artworks[0].GetImages(1);
                var topSprites = await point.artworks[1].GetImages(1);
                var bottomSprites = await point.artworks[2].GetImages(1);
                
                mainImage.sprite = mainSprites[0];
                topImage.sprite = topSprites[0];
                bottomImage.sprite = bottomSprites[0];


                // Update aspect ratios.
                var width = mainImage.sprite.rect.width;
                var height = mainImage.sprite.rect.height;
                mainImageAspect.aspectRatio = width / height;

                width = topImage.sprite.rect.width;
                height = topImage.sprite.rect.height;
                topImageAspect.aspectRatio = width / height;

                width = bottomImage.sprite.rect.width;
                height = bottomImage.sprite.rect.height;
                bottomImageAspect.aspectRatio = width / height;

            }
            else if(point.exhibition_image_references.Count > 0)
            {
                singleCoverImageContainer.SetActive(true);
                multipleCoverImagesContainer.SetActive(false);
                var images = await point.GetImages(1);
                singleImage.sprite = images.Count >= 0 ? images[0] : null;
                if (singleImage.sprite != null)
                {
                    var image0AspectRatio = singleImage.sprite.rect.width / singleImage.sprite.rect.height;
                    singleImage.GetComponent<AspectRatioFitter>().aspectRatio = image0AspectRatio;
                }
            }
            else
            {
                singleCoverImageContainer.SetActive(true);
                multipleCoverImagesContainer.SetActive(false);
                singleImage.sprite = null;
            }

            loadingCircle?.StopLoading();

            UpdateDownloadStatus();
        }
        catch(Exception e)
        {
            Debug.LogError("Failed to fill ExhibitionCard: " + e);
        }
    }

    public void UpdateDownloadStatus()
    {
        if (exhibition == null)
            return;

        if (DownloadManager.ExhibitionIsDownloaded(exhibition) && downloadStatusImage != null)
            downloadStatusImage.color = downloadedColor;
        else if (downloadStatusImage != null)
            downloadStatusImage.color = defaultColor;
    }

    protected void OpenExhibitionPage()
    {
        if (exhibition == null) return;
        
        if(ArtworkUIManager.Instance != null)
            ArtworkUIManager.Instance.OpenDetailedInformation(exhibition);
        else
        {
            ArtworkUIManager.SelectedExhibition = exhibition;
            SceneManager.LoadScene("Exhibition&Art");
        }
    }
}
