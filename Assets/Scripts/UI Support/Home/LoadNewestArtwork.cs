using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Messy.Definitions;
using UnityEngine;

public class LoadNewestArtwork : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GalleryCard galleryCard;
    [SerializeField] private int showCount = 4;
    [SerializeField] private Transform parent;

    [Header("Custom loading")]
    [SerializeField] private bool loadCustomArtwork = false;
    [SerializeField] private ARPointSO[] customArtworkOrder;
    
    void Start()
    {
        var orderedList = new List<ARPointSO>(0);

        if (!loadCustomArtwork)
        {
            var listArtworks = new List<ARPointSO>();
            
            foreach (ARPointSO point in ARInfoManager.ExhibitionsSO.SelectMany(s => s.ArtWorks)
                         .Where(t => t.ArtworkImages.Count != 0))
            {
                listArtworks.Add(point);
            }
            
            orderedList = listArtworks.OrderByDescending(point => point.Year).ToList();
        }
        else
        {
            orderedList = new List<ARPointSO>(customArtworkOrder);
        }

        
        for (int i = 0; i < showCount; i++)
        {
            if (i >= orderedList.Count) return;

            var card = Instantiate(galleryCard, parent);
            card.gameObject.SetActive(true);
            card.LoadARPoint(orderedList[i]);
        }
    }
}