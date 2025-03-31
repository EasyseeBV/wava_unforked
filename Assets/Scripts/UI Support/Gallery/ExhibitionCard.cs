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
    [SerializeField] private Image image0;
    [SerializeField] private Image image1;
    [SerializeField] private Image image2;
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
            gameObject.SetActive(false);
            return;
        }

        try
        {
            exhibition = point;

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

            loadingCircle?.StopLoading();

            titleLabel.text = point.title;
            yearLocationLabel.text = point.year + " · " + point.location;
        }
        catch(Exception e)
        {
            Debug.Log("Failed to fill ExhibitionCard: " + e);
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
