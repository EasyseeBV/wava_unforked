using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DownloadCachedButton : MonoBehaviour
{
    [SerializeField] private Button deleteButton;

    private ArtworkData cachedData;

    private void Awake()
    {
        deleteButton.onClick.AddListener(Delete);
    }

    public void LoadData(ArtworkData data)
    {
        
    }

    private void Delete()
    {
        
    }
}
