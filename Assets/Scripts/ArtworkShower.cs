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
                ARPhoto.sprite = images.Count > 0 ? images[0] : null;
            }
            else ARPhoto.sprite = null;
        }
        catch (Exception e)
        {
            ARPhoto.sprite = null;
            Debug.Log("Failed to set ArtworkShower image: " + e);
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
