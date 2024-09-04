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
    [HideInInspector] public ExhibitionSO exhibition;
    
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

    public void Init(ExhibitionSO point)
    {
        exhibition = point;

        if (point.ExhibitionImages.Count >= 3)
        {
            singleCoverImageObject.SetActive(false);
            multCoverImageObject.SetActive(true);
            
            image0.sprite = point.ExhibitionImages[0];
            image1.sprite = point.ExhibitionImages[1];
            image2.sprite = point.ExhibitionImages[2];
        }
        else if (point.ArtWorks.Count >= 3)
        {
            singleCoverImageObject.SetActive(false);
            multCoverImageObject.SetActive(true);
            
            image0.sprite = point.ArtWorks[0].ArtworkImages[0];
            image1.sprite = point.ArtWorks[1].ArtworkImages[0];
            image2.sprite = point.ArtWorks[2].ArtworkImages[0];
        }
        else if(point.ExhibitionImages.Count > 0)
        {
            singleCoverImageObject.SetActive(true);
            multCoverImageObject.SetActive(false);
            singleImage.sprite = point.ExhibitionImages[0];
        }
        else
        {
            singleCoverImageObject.SetActive(true);
            multCoverImageObject.SetActive(false);
            singleImage.sprite = null;
        }

        titleLabel.text = point.Title;
        yearLocationLabel.text = point.Year + " · " + point.Location;
    }

    protected void OpenExhibitionPage()
    {
        if (!exhibition) return;
        
        if(ArtworkUIManager.Instance != null)
            ArtworkUIManager.Instance.OpenDetailedInformation(exhibition);
        else
        {
            ArtworkUIManager.SelectedExhibition = exhibition;
            SceneManager.LoadScene("Exhibition&Art");
        }
    }
}
