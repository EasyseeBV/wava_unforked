using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Messy.Definitions;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ArtistContainer : MonoBehaviour
{
    [Header("References")] 
    [SerializeField] private Image profilePicture;
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

        profilePicture.sprite = await artist.GetIcon();
        artistNameLabel.text = artist.title;
        int works = GetArtistWorkCount();
        artworkCountLabel.text = works == 1 ? "1 Artwork" : $"{GetArtistWorkCount()} Artworks";
        artistPageButton.onClick.AddListener(OpenArtistPage);
    }

    private int GetArtistWorkCount()
    {
        return FirebaseLoader.Artworks == null ? 0 : FirebaseLoader.Artworks.Count(artwork => artwork.artists.Contains(artist));
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
