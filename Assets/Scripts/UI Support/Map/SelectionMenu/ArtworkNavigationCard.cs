using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ArtworkNavigationCard : MonoBehaviour
{
    [Header("Content")]
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private GameObject minimizedContent;
    [SerializeField] private GameObject maximizedContent;
    
    [Header("Minimal Content")]
    [SerializeField] private float minimizedParentHeight = 66;
    [SerializeField] private CanvasGroup minimizedGroup;
    [SerializeField] private TMP_Text minimizedTitle;
    [SerializeField] private Button minimizedNavigationButton;
    
    [Header("Maximal Content")]
    [SerializeField] private float maximizedParentHeight = 140;
    [SerializeField] private CanvasGroup maximizedGroup;
    [SerializeField] private TextSlider maximizedTitleTextSlider;
    [SerializeField] private TextSlider locationTextSlider;
    [SerializeField] private TextSlider exhibitionAndArtistTextSlider;
    [SerializeField] private Image image;
    [SerializeField] private TMP_Text buttonLabel;
    [SerializeField] private Image labelImage;
    [SerializeField] private Button maximizedNavigationButton;

    [Header("References")]
    [SerializeField] private Button infoButton;
    [SerializeField] private Sprite navigateSprite;
    [SerializeField] private Sprite arSprite;
    
    public ArtworkData CachedArtworkData { get; private set; }

    private void Awake()
    {
        infoButton.onClick.AddListener(() =>
        {
            ArtworkUIManager.SelectedArtwork = CachedArtworkData;
            SceneManager.LoadScene("Exhibition&Art");
        });
    }

    public void Populate(ArtworkData artworkData)
    {
        CachedArtworkData = artworkData;

        minimizedTitle.text = artworkData.title;
        minimizedNavigationButton.onClick.AddListener(() =>
        {
            GetDirections();
        });

        maximizedTitleTextSlider.SetTextAndResetAnimation(artworkData.title);

        locationTextSlider.SetTextAndResetAnimation(artworkData.location);

        var exhibition = FirebaseLoader.GetConnectedExhibition(artworkData)?.title ?? "-";
        var artist = artworkData.artists.Count > 0 ? (artworkData.artists[0]?.title ?? "-") : "-";
        var grayPoint = "<color=#707070>\u25CF</color>";

        exhibitionAndArtistTextSlider.SetTextAndResetAnimation($"{exhibition} {grayPoint} {artist}");


        maximizedNavigationButton.onClick.AddListener(() =>
        {
            GetDirections(); // might need additional functionality in the future
        });
        
        WaitForImage();
    }

    private async Task WaitForImage()
    {
        var sprite = await CachedArtworkData.GetImages(1);

        if (sprite is { Count: > 0 })
        {
            var spr = sprite[0];
            image.sprite = spr;
            
            var aspectRatioFitter = image.GetComponent<AspectRatioFitter>();
            var aspectRatio = spr.rect.width / spr.rect.height;
            aspectRatioFitter.aspectRatio = aspectRatio;
        }
    }

    public void Expand(bool state)
    {
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, state ? maximizedParentHeight : minimizedParentHeight);

        minimizedGroup.DOKill();
        maximizedGroup.DOKill();

        minimizedGroup.alpha = state ? 0f : 1f;
        maximizedGroup.alpha = state ? 1f : 0f;

        minimizedGroup.interactable = !state;
        maximizedGroup.interactable = state;
        
        minimizedGroup.blocksRaycasts = !state;
        maximizedGroup.blocksRaycasts = state;
    }
    
    public void GetDirections()
    {
        string location = CachedArtworkData.latitude + "," + CachedArtworkData.longitude;
        
#if UNITY_ANDROID
        string url = "https://www.google.com/maps/dir/?api=1&destination=" + location;
        Application.OpenURL(url);
#elif UNITY_IOS
            // Apple Maps URL scheme for iOS
            string url = "http://maps.apple.com/?daddr=" + location;
            Application.OpenURL(url);
#endif
    }

    private bool allowedAR = false;

    public void AllowAR()
    {
        allowedAR = true;
        buttonLabel.text = "Open AR-View now";
        labelImage.sprite = arSprite;
        minimizedNavigationButton.onClick.RemoveAllListeners();
        maximizedNavigationButton.onClick.RemoveAllListeners();
        maximizedNavigationButton.onClick.AddListener(OpenARView);
        minimizedNavigationButton.onClick.AddListener(OpenARView);
        OnlineMapsLocationService.instance.OnLocationChanged += OnLocationChanged;
    }

    public void DisallowAR()
    {
        if (!DistanceValidator.InRange(CachedArtworkData)) return;
        
        minimizedNavigationButton.onClick.RemoveAllListeners();
        maximizedNavigationButton.onClick.RemoveAllListeners();
        maximizedNavigationButton.onClick.AddListener(GetDirections);
        minimizedNavigationButton.onClick.AddListener(GetDirections);
        buttonLabel.text = "Navigate";
        labelImage.sprite = navigateSprite;
        allowedAR = false;
        OnlineMapsLocationService.instance.OnLocationChanged -= OnLocationChanged;
    }

    private void OnDisable()
    {
        if (allowedAR) OnlineMapsLocationService.instance.OnLocationChanged -= OnLocationChanged;
    }

    private void OnLocationChanged(Vector2 obj)
    {
        if (!DistanceValidator.InRange(CachedArtworkData))
        {
            DisallowAR();
        }
    }
    
    private void OpenARView()
    {
        ARLoader.Open(CachedArtworkData);
    }

    /*
     * private void OpenArtworkInGallery()
    {
        if (!cachedHotspot) return;
        
        ArtworkUIManager.SelectedArtwork = cachedHotspot.GetHotspotArtwork();
        SceneManager.LoadScene("Exhibition&Art");
    }

    private void OpenArtworkInAR()
    {
        if (!cachedHotspot) return;
        
        ArTapper.ArtworkToPlace = cachedHotspot.GetHotspotArtwork();
        ARLoader.Open(cachedHotspot.GetHotspotArtwork(), cachedDistance);
    }

    private void StartAR()
    {
        if (!cachedHotspot) return;
        
        cachedHotspot.StartAR(cachedHotspot.GetHotspotArtwork());
    }
     */
}
