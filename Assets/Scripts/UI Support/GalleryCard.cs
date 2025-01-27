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

    public void LoadARPoint(ArtworkData point)
    {
        if (point == null)
        {
            Debug.LogWarning("Trying to fill a GalleryCard with a missing ARPointSO, please assign the ARPoint you wish to show", this);
            gameObject.SetActive(false);
            return;    
        }

        artwork = point;

        Debug.LogError("DISABLED LOADING ARTWORK COVER IMAGE");
        artworkImage.sprite = point.artwork_images[0];//artwork_cover_image;
        artworkLabel.text = point.title;
        artistLabel.text = point.artists.Count > 0 ? point.artists[0].title : null;
        yearLabel.text = point.year.ToString();
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
