using System;
using System.Collections;
using System.Collections.Generic;
using Messy.Definitions;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GalleryCard : MonoBehaviour
{
    [Header("Loading")]
    public ArtworkData artwork;
    [SerializeField] private bool loadAssignedARPoint;
    
    [Header("UI References")]
    [SerializeField] private Image artworkImage;
    [SerializeField] private TMP_Text artworkLabel;
    [SerializeField] private TMP_Text artistLabel;
    [SerializeField] private TMP_Text yearLabel;
    [SerializeField] private Button button;

    public ExhibitionData sourceExhibition {get; private set;}

    private void Start()
    {
        if (loadAssignedARPoint) LoadARPoint(artwork);
        if(button) button.onClick.AddListener(GoToGallery);
    }

    public async void LoadARPoint(ArtworkData point)
    {
        if (point == null)
        {
            Debug.LogWarning("Trying to fill a GalleryCard with a missing ARPointSO, please assign the ARPoint you wish to show", this);
            gameObject.SetActive(false);
            return;    
        }

        try
        {
            artwork = point;

            if (point.artwork_image_references.Count > 0)
            {
                var images = await point.GetImages(1);
                artworkImage.sprite = images[0];
            }
            else artworkImage.sprite = null;
            
            artworkLabel.text = point.title;
            artistLabel.text = point.artists.Count > 0 ? point.artists[0].title : null;
            yearLabel.text = point.year.ToString();
        }
        catch (Exception e)
        {
            Debug.Log("Failed to load ARPoint: " + e);
        }
    }

    private void GoToGallery()
    {
        if (artwork == null) return;
        
        if(ArtworkUIManager.Instance != null)
            ArtworkUIManager.Instance.OpenDetailedInformation(artwork);
        else
        {
            ArtworkUIManager.SelectedArtwork = artwork;
            SceneManager.LoadScene("Exhibition&Art");
        }
    }
}
