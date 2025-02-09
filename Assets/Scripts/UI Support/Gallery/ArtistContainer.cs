using System.Collections;
using System.Collections.Generic;
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
    
    public void Assign(ArtistData artist)
    {
        if (artist == null)
        {
            Debug.LogWarning("Empty Artist Provided");
            gameObject.SetActive(false);
            return;
        }
        
        this.artist = artist;

        if(artist.iconImage != null) profilePicture.sprite = artist.iconImage;
        artistNameLabel.text = artist.title;
        int works = GetArtistWorkCount();
        artworkCountLabel.text = works == 1 ? "1 Artwork" : $"{GetArtistWorkCount()} Artworks";
        artistPageButton.onClick.AddListener(OpenArtistPage);
    }

    private int GetArtistWorkCount()
    {
        if (FirebaseLoader.Exhibitions == null) return 0;

        int count = 0;
        foreach (var exh in FirebaseLoader.Exhibitions )
        {
            foreach (var artworks in exh.artworks)
            {
                if (artworks.artists.Contains(artist))
                {
                    count++;
                }
            }
        }

        return count;
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
