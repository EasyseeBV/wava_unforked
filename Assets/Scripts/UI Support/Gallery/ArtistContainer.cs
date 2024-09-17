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

    [HideInInspector] public ArtistSO artist;
    
    public void Assign(ArtistSO artist)
    {
        if (artist == null)
        {
            Debug.LogWarning("Empty Artist Provided");
            gameObject.SetActive(false);
            return;
        }
        
        this.artist = artist;

        if(artist.ArtistIcon != null) 
            profilePicture.sprite = artist.ArtistIcon;
        artistNameLabel.text = artist.Title;
        int works = GetArtistWorkCount();
        artworkCountLabel.text = works == 1 ? "1 Artwork" : $"{GetArtistWorkCount()} Artworks";
        artistPageButton.onClick.AddListener(OpenArtistPage);
    }

    private int GetArtistWorkCount()
    {
        if (ARInfoManager.ExhibitionsSO == null) return 0;

        int count = 0;
        foreach (var exh in ARInfoManager.ExhibitionsSO)
        {
            foreach (var artworks in exh.ArtWorks)
            {
                if (artworks.Artists.Contains(artist))
                {
                    count++;
                }
            }
        }

        return count;
    }

    private void OpenArtistPage()
    {
        if (!artist) return;
        
        if(ArtworkUIManager.Instance != null)
            ArtworkUIManager.Instance.OpenDetailedInformation(artist);
        else
        {
            ArtworkUIManager.SelectedArtist = artist;
            SceneManager.LoadScene("Exhibition&Art");
        }
    }
}
