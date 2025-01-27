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

    void Start()
    {
        var orderedList = new List<ArtworkData>(0);
        
        var listArtworks = new List<ArtworkData>();
        
        foreach (ArtworkData point in FirebaseLoader.Exhibitions.SelectMany(s => s.artworks)
                     .Where(t => t.artwork_images.Count != 0))
        {
            listArtworks.Add(point);
        }
        
        orderedList = listArtworks.OrderByDescending(point => point.year).ToList();
        
        for (int i = 0; i < showCount; i++)
        {
            if (i >= orderedList.Count) return;

            var card = Instantiate(galleryCard, parent);
            card.gameObject.SetActive(true);
            card.LoadARPoint(orderedList[i]);
        }
    }
}