using System.Collections;
using System.Collections.Generic;
using Messy.Definitions;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Linq;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.XR.ARFoundation;

public class ArtworkShower : MonoBehaviour
{
    public Image ARPhoto;
    public TextMeshProUGUI Title;
    public TextMeshProUGUI Artist;
    public TextMeshProUGUI Location;
    public TextMeshProUGUI Year;
    public Button DetailButton;
    public Button ViewButton;
    [Space]
    public TextMeshProUGUI exhibitionTitle;
    [HideInInspector] public ARPointSO cachedARPointSO;

    private void Awake()
    {
        ViewButton.onClick.AddListener(OpenDetails);
        DetailButton.onClick.AddListener(OpenDetails);
    }

    public void Init(ARPointSO point) {
        ARPhoto.sprite = point.ArtworkImages[0];
        Title.text = point.Title;
        Artist.text = point.Artist;
        //Location.text = point.Location;
        Year.text = point.Year;

        foreach (var exhibitionSO in ARInfoManager.ExhibitionsSO.Where(exhibitionSO => exhibitionSO.ArtWorks.Contains(point)))
        {
            exhibitionTitle.text = exhibitionSO.Title;
            break;
        }
        
        cachedARPointSO = point;
    }

    private void OpenDetails()
    {
        if (!cachedARPointSO) return;
        
        if(ArtworkUIManager.Instance != null)
            ArtworkUIManager.Instance.OpenDetailedInformation(cachedARPointSO);
        else
        {
            ArtworkUIManager.SelectedArtwork = cachedARPointSO;
            SceneManager.LoadScene("Exhibition&Art");
        }
    }
}
