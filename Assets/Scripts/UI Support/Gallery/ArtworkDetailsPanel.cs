using System;
using System.Collections;
using System.Collections.Generic;
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
    [SerializeField] private Image galleryImagePrefab;
    [Space]
    [SerializeField] private Transform indicatorArea;
    [SerializeField] private GameObject indicatorImage;
    [SerializeField] private Color activeColor;
    [SerializeField] private Color inactiveColor;

    [Header("Interactions")]
    [SerializeField] private Button showOnMapButton;
    
    [Header("Artists")] 
    [SerializeField] private Transform artistArea;
    [SerializeField] private ArtistContainer artistContainer;

    [Header("Exhibition")]
    [SerializeField] private ExhibitionCard exhibitionCard;

    private List<Image> indicators = new();

    protected override void Setup()
    {
        base.Setup();
        heartButton.onClick.AddListener(LikeArtwork);
        scrollSnapper.OnPanelCentered.AddListener(ChangeIndicator);
    }

    public void Fill(ArtworkData artwork)
    {
        this.artwork = artwork;
        
        Clear();

        for (int i = 0; i < artwork.artwork_images.Count; i++)
        {
            Image artworkImage = scrollSnapper.AddToBack(galleryImagePrefab.gameObject).GetComponent<Image>();
            artworkImage.sprite = artwork.artwork_images[i];

            Image indicator = Instantiate(indicatorImage, indicatorArea).GetComponentInChildren<Image>();
            indicator.color = inactiveColor;
            indicators.Add(indicator);
        }

        for (int i = 0; i < artwork.artists.Count; i++)
        {
            ArtistContainer container = Instantiate(artistContainer, artistArea);
            container.gameObject.SetActive(true);
            container.Assign(artwork.artists[i]);
        }

        ExhibitionData exhb = GetExhibition();
        exhibitionCard.Init(exhb);
        
        contentTitleLabel.text = artwork.title;
        fullLengthDescription = artwork.description;
        TruncateText();
        
        // heartImage.sprite = artwork.Liked ? likedSprite : unlikedSprite;
        
        showOnMapButton.onClick.RemoveAllListeners();
        showOnMapButton.onClick.AddListener(() =>
        {
            SelectionMenu.SelectedARPoint = artwork;
            SceneManager.LoadScene("Map");
        });
        
        scrollSnapper.Setup();
        ChangeIndicator(0, 0);
        
        StartCoroutine(LateRebuild());
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

    private ExhibitionData GetExhibition()
    {
        if (FirebaseLoader.Exhibitions == null) return null;

        foreach (var exhibition in FirebaseLoader.Exhibitions)
        {
            if (exhibition.artworks.Contains(artwork)) return exhibition;
        }

        return null;
    }

    private void ChangeIndicator(int newIndex,int oldIndex)
    {
        if (indicators.Count <= 0) return;
        
        indicators[oldIndex].color = inactiveColor;
        indicators[newIndex].color = activeColor;
    }
}