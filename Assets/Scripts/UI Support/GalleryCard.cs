using System;
using System.Collections;
using System.Collections.Generic;
using Messy.Definitions;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GalleryCard : MonoBehaviour
{
    [Header("Loading")]
    public ARPointSO arPoint;
    [SerializeField] private bool loadAssignedARPoint;
    
    [Header("UI References")]
    [SerializeField] private Image artworkImage;
    [SerializeField] private TMP_Text artworkLabel;
    [SerializeField] private TMP_Text artistLabel;
    [SerializeField] private TMP_Text yearLabel;
    [SerializeField] private Button button;

    [HideInInspector] public ExhibitionSO sourceExhibition;

    private void Start()
    {
        if (loadAssignedARPoint) LoadARPoint(arPoint);
        if(button) button.onClick.AddListener(GoToGallery);
    }

    public void LoadARPoint(ARPointSO point)
    {
        if (point == null)
        {
            Debug.LogWarning("Trying to fill a GalleryCard with a missing ARPointSO, please assign the ARPoint you wish to show", this);
            gameObject.SetActive(false);
            return;    
        }

        arPoint = point;
        
        artworkImage.sprite = point.ARMapImage;
        artworkLabel.text = point.Title;
        artistLabel.text = point.Artist;
        yearLabel.text = point.Year;
    }

    private void GoToGallery()
    {
        if (!arPoint) return;
        
        if(ArtworkUIManager.Instance != null)
            ArtworkUIManager.Instance.OpenDetailedInformation(arPoint);
        else
        {
            ArtworkUIManager.SelectedArtwork = arPoint;
            SceneManager.LoadScene("Exhibition&Art");
        }
    }
}
