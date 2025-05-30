using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Messy.Definitions;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ArtistContainer : MonoBehaviour
{
    [Header("References")] 
    [SerializeField] private Image profilePicture;
    [SerializeField] private RectTransform profilePictureParent;
    [SerializeField] private TextMeshProUGUI artistNameLabel;
    [SerializeField] private TextMeshProUGUI artworkCountLabel;
    [SerializeField] private Button artistPageButton;

    [HideInInspector] public ArtistData artist;
    
    public async void Assign(ArtistData artist)
    {
        if (artist == null)
        {
            Debug.LogWarning("Empty Artist Provided");
            gameObject.SetActive(false);
            return;
        }
        
        this.artist = artist;

        artistNameLabel.text = artist.title;
        int works = GetArtistWorkCount();
        artworkCountLabel.text = works == 1 ? "1 Artwork" : $"{GetArtistWorkCount()} Artworks";
        artistPageButton.onClick.AddListener(OpenArtistPage);
        SetImage();
    }
    
    private async Task SetImage()
    {
        try
        {
            if (!string.IsNullOrEmpty(artist.icon))
            {
                var image = await artist.GetIcon();
                profilePicture.sprite = image;

                if (profilePicture.sprite != null)
                {
                    var imageAspectRatio = profilePicture.sprite.rect.width / profilePicture.sprite.rect.height;
                    profilePicture.GetComponent<AspectRatioFitter>().aspectRatio = imageAspectRatio;
                }
            }
            else profilePicture.sprite = null;
        }
        catch (Exception e)
        {
            profilePicture.sprite = null;
            Debug.Log("Failed to set ArtistContainer icon image: " + e);
        }
    }

    private int GetArtistWorkCount()
    {
        int works = 0;
        foreach (var artwork in FirebaseLoader.Artworks)
        {
            if (artwork.artists.Contains(artist))
            {
                works++;
                continue;
            }

            if (artwork.artists.Any(artistData => artistData.id == artist.id || artistData.title == artist.title))
            {
                works++;
            }
        }

        return works;
    }

    private void OpenArtistPage()
    {
        if (artist == null) return;
        
        if(ArtworkUIManager.Instance != null)
            ArtworkUIManager.Instance.OpenDetailedInformation(artist);
        else
        {
            ArtworkUIManager.SelectedArtist = artist;
            SceneManager.LoadScene("Exhibition&Art");
        }
    }
}
