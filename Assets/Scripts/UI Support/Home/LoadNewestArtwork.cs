using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Messy.Definitions;
using UnityEngine;

public class LoadNewestArtwork : MonoBehaviour
{
    [SerializeField] private GalleryCard[] galleryCards;
    
    void Start()
    {
        var listArtworks = new List<ARPointSO>();

        foreach (ARPointSO point in ARInfoManager.ExhibitionsSO.SelectMany(s => s.ArtWorks)
                     .Where(t => t.ArtworkImages.Count != 0))
        {
            listArtworks.Add(point);    
        }
        
        var orderedList = listArtworks.OrderByDescending(point => point.Year).ToList();
        
        for (int i = 0; i < galleryCards.Length; i++)
        {
            if (i >= orderedList.Count) galleryCards[i].LoadARPoint(orderedList[^1]);
            
            galleryCards[i].LoadARPoint(orderedList[i]);
        }
    }
}
