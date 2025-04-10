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
    [SerializeField] private RectTransform artworkParentMask;
    [SerializeField] private TMP_Text artworkLabel;
    [SerializeField] private TMP_Text artistLabel;
    [SerializeField] private TMP_Text yearLabel;
    [SerializeField] private Button button;
    [SerializeField] private LoadingCircle loadingCircle;

    public ExhibitionData sourceExhibition {get; private set;}

    private void Awake()
    {
        loadingCircle?.BeginLoading();
    }

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
            
            artworkLabel.text = point.title;
            artistLabel.text = point.artists.Count > 0 ? point.artists[0].title : null;
            yearLabel.text = point.year.ToString();
            
            if (point.artwork_image_references.Count > 0)
            {
                var images = await point.GetImages(1);
                artworkImage.sprite = images[0];
                UpdateCover(); // adjusts the sizing of the image
            }
            else artworkImage.sprite = null;
            
            loadingCircle?.StopLoading();
        }
        catch (Exception e)
        {
            Debug.Log("Failed to load ARPoint: " + e);
        }
    }

    private void UpdateCover()
    {
        if (artworkImage.sprite == null) return;
        
        artworkImage.preserveAspect = true;

        // Get the RectTransform of the artwork image.
        RectTransform imageRect = artworkImage.GetComponent<RectTransform>();

        // Retrieve the original sprite dimensions.
        float spriteWidth = artworkImage.sprite.rect.width;
        float spriteHeight = artworkImage.sprite.rect.height;

        // Get the available size from the mask's RectTransform.
        Vector2 maskSize = artworkParentMask.rect.size;

        // Compute the scale factor needed.
        // We take the larger ratio so the image always completely covers the mask.
        float scaleFactor = Mathf.Max(maskSize.x / spriteWidth, maskSize.y / spriteHeight);

        // Calculate the new size for the image.
        float newWidth = spriteWidth * scaleFactor;
        float newHeight = spriteHeight * scaleFactor;

        // Set the sizeDelta of the image’s RectTransform.
        imageRect.sizeDelta = new Vector2(newWidth, newHeight);

        // Ensure the image is centered.
        imageRect.anchoredPosition = Vector2.zero;
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
