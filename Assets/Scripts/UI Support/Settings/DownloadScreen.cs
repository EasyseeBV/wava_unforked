using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class DownloadScreen : MonoBehaviour
{
    [SerializeField] private GameObject content;
    [SerializeField] private Button exitButton;
    [SerializeField] private DownloadCachedButton downloadCachedButton;
    [SerializeField] private Transform layout;
    
    private List<DownloadCachedButton> downloadButtons = new List<DownloadCachedButton>();

    private void Awake()
    {
        downloadCachedButton.gameObject.SetActive(false);
        exitButton.onClick.AddListener(Close);
    }

    public void Open()
    {
        foreach (var downloadButton in downloadButtons)
        {
            downloadButton.gameObject.SetActive(false);
        }
        downloadButtons.Clear();

        // Iterate through cached artwork downloads and create a button for each that has media
        /*foreach (var download in AppCache.ArtworkDownloads)
        {
            if (download.mediaPaths != null && download.mediaPaths.Count > 0)
            {
                // Find the associated ArtworkData using the artwork_id
                ArtworkData artwork = FirebaseLoader.Artworks.FirstOrDefault(a => a.id == download.artwork_id);
                if (artwork != null)
                {
                    DownloadCachedButton newButton = Instantiate(downloadCachedButton, layout);
                    newButton.gameObject.SetActive(true);
                    newButton.LoadData(artwork);
                    downloadButtons.Add(newButton);
                }
            }
        }*/
        
        content.SetActive(true);
    }

    public void Close()
    {
        content.SetActive(false);
    }
}
