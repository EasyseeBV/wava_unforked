using UnityEngine;

public class AutoLoadDisplay : MonoBehaviour
{
    public static DisplayView View = DisplayView.Artworks;

    [SerializeField] private ArtworkUIManager artworkUIManager;
    
    private void Awake()
    {
        if(View == DisplayView.Exhibitions) artworkUIManager.InitExhibitions();
        else if(View == DisplayView.Artists) artworkUIManager.InitArtists();
        else artworkUIManager.InitArtworks();

        View = DisplayView.Artworks;
    }
}

[System.Serializable]
public enum DisplayView
{
    Artworks,
    Exhibitions,
    Artists
}