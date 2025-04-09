using UnityEngine;

public class AutoLoadDisplay : MonoBehaviour
{
    public static DisplayView View = DisplayView.Artworks;

    [SerializeField] private ArtworkUIManager artworkUIManager;
    
    private void Awake()
    {
        Debug.Log("Auto-loading display");

        if (View == DisplayView.Exhibitions)
        {
            artworkUIManager.InitExhibitions();
        }
        else if (View == DisplayView.Artists)
        {
            artworkUIManager.InitArtists();
        }
        else if (View == DisplayView.Artworks)
        {
            artworkUIManager.InitArtworks();
        }

        /*
        View = DisplayView.Artworks;*/
    }
}

[System.Serializable]
public enum DisplayView
{
    Artworks,
    Exhibitions,
    Artists
}