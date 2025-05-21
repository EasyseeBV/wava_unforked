using System.Collections;
using System.Collections.Generic;
using Messy.Definitions;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.XR.ARFoundation;

public class ArtworkShower : MonoBehaviour
{
    public Image ARPhoto;
    public TextMeshProUGUI Title;
    public TextMeshProUGUI Artist;
    public TextMeshProUGUI Location;
    public TextMeshProUGUI Year;
    public Button DetailButton;
    public Button ViewButton;
    [Space]
    public TextMeshProUGUI exhibitionTitle;
    
    public ArtworkData cachedArtwork { get; set; }

    private void Awake()
    {
        ViewButton.onClick.AddListener(OpenDetails);
        DetailButton.onClick.AddListener(OpenDetails);
    }

    public void Init(ArtworkData artwork) 
    {
        if (artwork == null)
        {
            gameObject.SetActive(false);
            return;
        }
        
        Title.text = artwork.title;
        Artist.text = artwork.artists.Count > 0 ? artwork.artists[0].title : null;

        //Location.text = point.Location;
        Year.text = artwork.year.ToString();

        foreach (var exhibition in FirebaseLoader.Exhibitions.Where(exhibition => exhibition.artworks.Contains(artwork)))
        {
            exhibitionTitle.text = exhibition.title;
            break;
        }
        
        cachedArtwork = artwork;

        SetImage(artwork);
    }

    private async Task SetImage(ArtworkData artwork)
    {
        try
        {
            if (artwork.artwork_image_references.Count > 0)
            {
                var images = await artwork.GetImages(1);

                if (images.Count <= 0)
                {
                    Debug.Log($"Removed artwork from display, could not get any images: OfflineMode status: [{FirebaseLoader.OfflineMode}]");
                    ARPhoto.sprite = null;
                    gameObject.SetActive(false);
                    return;
                }
                
                ARPhoto.sprite = images.Count > 0 ? images[0] : null;

                if (ARPhoto.sprite != null)
                {
                    var imageAspectRatio = ARPhoto.sprite.rect.width / ARPhoto.sprite.rect.height;
                    ARPhoto.GetComponent<AspectRatioFitter>().aspectRatio = imageAspectRatio;
                }
            }
            else
            {
                Debug.Log($"Removed artwork from display, could not get any images: OfflineMode status: [{FirebaseLoader.OfflineMode}]");
                ARPhoto.sprite = null;
                gameObject.SetActive(false);
            }
        }
        catch (Exception e)
        {
            ARPhoto.sprite = null;
            Debug.Log($"Failed to set ArtworkShower image: {e} | OfflineMode status: [{FirebaseLoader.OfflineMode}]");
            gameObject.SetActive(false);
        }
    }

    private void OpenDetails()
    {
        if (cachedArtwork == null) return;
        
        if(ArtworkUIManager.Instance != null)
            ArtworkUIManager.Instance.OpenDetailedInformation(cachedArtwork);
        else
        {
            ArtworkUIManager.SelectedArtwork = cachedArtwork;
            SceneManager.LoadScene("Exhibition&Art");
        }
    }
}
