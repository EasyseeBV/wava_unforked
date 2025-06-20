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
    [SerializeField] private TMP_Text maximizedTitle;
    [SerializeField] private TMP_Text locationLabel;
    [SerializeField] private TMP_Text exhibitionLabel;
    [SerializeField] private TMP_Text artistLabel;
    [SerializeField] private Image image;
    [SerializeField] private Button maximizedNavigationButton;

    [Header("References")]
    [SerializeField] private Button infoButton;
    
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
            Debug.Log("Navigation link needs to be implemented - unclear how it should function");
        });
        
        maximizedTitle.text = artworkData.title;
        locationLabel.text = artworkData.location;
        exhibitionLabel.text = FirebaseLoader.GetConnectedExhibition(artworkData)?.title ?? "-";
        artistLabel.text = artworkData.artists.Count > 0 ? (artworkData.artists[0]?.title ?? "-") : "-";
        maximizedNavigationButton.onClick.AddListener(() =>
        {
            Debug.Log("Navigation link needs to be implemented - unclear how it should function");
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
        
        return;
        
        if (state)
        {
            minimizedGroup.alpha = 1f;
            maximizedGroup.alpha = 0f;
            minimizedGroup.interactable = false;
            minimizedGroup.blocksRaycasts = false;
            minimizedGroup.DOFade(0f, 0.1f).SetEase(Ease.OutCubic).OnComplete(() =>
            {
                maximizedGroup.interactable = true;
                maximizedGroup.blocksRaycasts = true;
                maximizedGroup.DOFade(1f, 0.2f).SetEase(Ease.OutCubic);
            });
        }
        else
        {
            maximizedGroup.alpha = 1f;
            maximizedGroup.alpha = 0f;
            maximizedGroup.interactable = false;
            maximizedGroup.blocksRaycasts = false;
            maximizedGroup.DOFade(0f, 0.1f).SetEase(Ease.OutCubic).OnComplete(() =>
            {
                minimizedGroup.interactable = true;
                minimizedGroup.blocksRaycasts = true;
                minimizedGroup.DOFade(1f, 0.2f).SetEase(Ease.OutCubic);
            });
        }

        /*minimizedContent.SetActive(!state);
        maximizedContent.SetActive(state);*/
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
