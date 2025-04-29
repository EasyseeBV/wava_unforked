using System;
using System.Collections;
using System.Collections.Generic;
using Messy.Definitions;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class ExhibitionCard : MonoBehaviour
{
    [HideInInspector] public ExhibitionData exhibition;
    
    [Header("References")]
    [SerializeField] private GameObject singleCoverImageObject;
    [SerializeField] private GameObject multCoverImageObject;
    [Space]
    [SerializeField] private Image singleImage;
    [SerializeField] private RectTransform singleImageParent;
    [Space]
    [SerializeField] private Image image0;
    [SerializeField] private RectTransform image0Parent;
    [SerializeField] private Image image1;
    [SerializeField] private RectTransform image1Parent;
    [SerializeField] private Image image2;
    [SerializeField] private RectTransform image2Parent;
    [Space]
    [SerializeField] private TextMeshProUGUI titleLabel;
    [SerializeField] private TextMeshProUGUI yearLocationLabel;
    [Space]
    [SerializeField] private Button interactionButton;
    [Space]
    [SerializeField] private LoadingCircle loadingCircle;

    protected virtual void Awake()
    {
        if(interactionButton) interactionButton.onClick.AddListener(OpenExhibitionPage);
        
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
            
            titleLabel.text = point.title;
            yearLocationLabel.text = point.year + " · " + point.location;

            if (point.exhibition_image_references.Count >= 3)
            {
                singleCoverImageObject.SetActive(false);
                multCoverImageObject.SetActive(true);

                var images = await point.GetImages(3);
                
                image0.sprite = images.Count >= 0 ? images[0] : null;
                image1.sprite = images.Count >= 1 ? images[1] : null;
                image2.sprite = images.Count >= 2 ? images[2] : null;
            }
            else if (point.artworks.Count >= 3 && point.artworks[0].artwork_image_references.Count > 0 
                                               && point.artworks[1].artwork_image_references.Count > 0 
                                               && point.artworks[2].artwork_image_references.Count > 0)
            {
                singleCoverImageObject.SetActive(false);
                multCoverImageObject.SetActive(true);

                var artworkImages1 = await point.artworks[0].GetImages(1);
                var artworkImages2 = await point.artworks[1].GetImages(1);
                var artworkImages3 = await point.artworks[2].GetImages(1);
                
                image0.sprite = artworkImages1[0];
                image1.sprite = artworkImages2[0];
                image2.sprite = artworkImages3[0];
            }
            else if(point.exhibition_image_references.Count > 0)
            {
                singleCoverImageObject.SetActive(true);
                multCoverImageObject.SetActive(false);
                var images = await point.GetImages(1);
                singleImage.sprite = images.Count >= 0 ? images[0] : null;
            }
            else
            {
                singleCoverImageObject.SetActive(true);
                multCoverImageObject.SetActive(false);
                singleImage.sprite = null;
            }

            if (image0.sprite != null && image1.sprite != null && image2.sprite != null)
            {
                var image0AspectRatio = image0.sprite.rect.width / image0.sprite.rect.height;
                image0.GetComponent<AspectRatioFitter>().aspectRatio = image0AspectRatio;
                image0.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                image0.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                image0.rectTransform.pivot = new Vector2(0.5f, 0.5f);
                image0.rectTransform.anchoredPosition = Vector2.zero;
                
                var image1AspectRatio = image1.sprite.rect.width / image1.sprite.rect.height;
                image1.GetComponent<AspectRatioFitter>().aspectRatio = image1AspectRatio;
                image1.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                image1.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                image1.rectTransform.pivot = new Vector2(0.5f, 0.5f);
                image1.rectTransform.anchoredPosition = Vector2.zero;
                
                var image2AspectRatio = image2.sprite.rect.width / image2.sprite.rect.height;
                image2.GetComponent<AspectRatioFitter>().aspectRatio = image2AspectRatio;
                image2.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                image2.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                image2.rectTransform.pivot = new Vector2(0.5f, 0.5f);
                image2.rectTransform.anchoredPosition = Vector2.zero;
            }
            
            loadingCircle?.StopLoading();
        }
        catch(Exception e)
        {
            Debug.LogError("Failed to fill ExhibitionCard: " + e);
        }
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
