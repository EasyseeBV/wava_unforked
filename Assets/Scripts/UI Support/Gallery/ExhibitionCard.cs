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

    protected virtual void Awake()
    {
        if(interactionButton) interactionButton.onClick.AddListener(OpenExhibitionPage);
    }

    public void Init(ExhibitionData point)
    {
        if (point == null)
        {
            gameObject.SetActive(false);
            return;
        }
        
        exhibition = point;

        if (point.exhibition_images.Count >= 3)
        {
            singleCoverImageObject.SetActive(false);
            multCoverImageObject.SetActive(true);
            
            image0.sprite = point.exhibition_images[0];
            image1.sprite = point.exhibition_images[1];
            image2.sprite = point.exhibition_images[2];
        }
        else if (point.artworks.Count >= 3 && point.artworks[0].artwork_images.Count > 0 
                                           && point.artworks[1].artwork_images.Count > 0 
                                           && point.artworks[2].artwork_images.Count > 0)
        {
            singleCoverImageObject.SetActive(false);
            multCoverImageObject.SetActive(true);
            
            image0.sprite = point.artworks[0].artwork_images[0];
            image1.sprite = point.artworks[0].artwork_images[0];
            image2.sprite = point.artworks[0].artwork_images[0];
        }
        else if(point.exhibition_images.Count > 0)
        {
            singleCoverImageObject.SetActive(true);
            multCoverImageObject.SetActive(false);
            singleImage.sprite = point.exhibition_images[0];
        }
        else
        {
            singleCoverImageObject.SetActive(true);
            multCoverImageObject.SetActive(false);
            singleImage.sprite = null;
        }

        titleLabel.text = point.title;
        yearLocationLabel.text = point.year + " · " + point.location;
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
