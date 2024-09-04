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
        this.artist = artist;

        if(artist.ArtistIcon != null) 
            profilePicture.sprite = artist.ArtistIcon;
        artistNameLabel.text = artist.Title;
        artworkCountLabel.text = GetArtistWorkCount();
        artistPageButton.onClick.AddListener(OpenArtistPage);
    }

    private string GetArtistWorkCount()
    {
        if (ARInfoManager.ExhibitionsSO == null) return string.Empty;

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

        return count.ToString();
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
