using System;
using System.Collections;
using System.Collections.Generic;
using Messy.Definitions;
using UnityEngine;
using UnityEngine.UI;

public class FavoritesPage : MonoBehaviour
{
    [Header("Placement Areas")]
    [SerializeField] private Transform artworkArea;
    [SerializeField] private GalleryCard artworkPrefab;
    [Space]
    [SerializeField] private Transform exhibitionArea;
    [SerializeField] private ExhibitionCard exhibitionPrefab;
    [Space]
    [SerializeField] private Transform artistArea;
    [SerializeField] private ArtistContainer artistPrefab;
    [SerializeField] private RectTransform artistLayoutTransform;

    private List<ExhibitionCard> exhibitionCards = new();
    private List<GalleryCard> galleryCards = new();
    private List<ArtistContainer> artistContainers = new();
    
    public void Open()
    {
        if (ARInfoManager.ExhibitionsSO == null) return;

        var exhibitions = ARInfoManager.ExhibitionsSO;
        var cachedArtistList = new List<ArtistSO>();

        foreach (var exhibition in exhibitions)
        {
            if (exhibition.Liked)
            {
                ExhibitionCard card = Instantiate(exhibitionPrefab, exhibitionArea);
                card.Init(exhibition);
                exhibitionCards.Add(card);
            }

            foreach (var artwork in exhibition.ArtWorks)
            {
                if (artwork.Liked)
                {
                    GalleryCard galleryCard = Instantiate(artworkPrefab, artworkArea);
                    galleryCard.LoadARPoint(artwork);
                    galleryCard.sourceExhibition = exhibition;
                    galleryCards.Add(galleryCard);
                }

                foreach (var artist in artwork.Artists)
                {
                    cachedArtistList ??= new();
                    if (artist == null) continue;
                    
                    if (artist.Liked && !cachedArtistList.Contains(artist))
                    {
                        cachedArtistList.Add(artist);
                        ArtistContainer container = Instantiate(artistPrefab, artistArea);
                        container.Assign(artist);
                        artistContainers.Add(container);
                    }
                }
            }
        }

        StartCoroutine(LateRebuild());
    }

    public void Close()
    {
        foreach (Transform t in artworkArea)
        {
            Destroy(t.gameObject);
        }
        
        foreach (Transform t in exhibitionArea)
        {
            Destroy(t.gameObject);
        }
        
        foreach (Transform t in artistArea)
        {
            Destroy(t.gameObject);
        }
        
        exhibitionCards.Clear();
        galleryCards.Clear();
        artistContainers.Clear();
    }

    public void Filter(List<ProfileFilter.Filter> filters)
    {
        foreach (var card in exhibitionCards)
        {
            FilterExhibitionCard(card, filters);
        }

        foreach (var card in galleryCards)
        {
            FilterGalleryCard(card, filters);
        }

        foreach (var artist in artistContainers)
        {
            FilterArtistContainer(artist, filters);
        }
    }

    private void FilterExhibitionCard(ExhibitionCard card, List<ProfileFilter.Filter> filters)
    {
        if (filters.Count <= 0)
        {
            card.gameObject.SetActive(true);
            return;
        }

        bool state = true;
        
        foreach (var filter in filters)
        {
            if (state == false)
            {
                card.gameObject.SetActive(false);
                return;
            }
            
            switch (filter)
            {
                case ProfileFilter.Filter.Any:
                    state = true;
                    break;
                case ProfileFilter.Filter.Demo:
                    state = card.exhibition.Artist == "WAVA";
                    break;
                case ProfileFilter.Filter.StadelMuseum:
                    state = card.exhibition.Location == "Frankfurt";
                    break;
                case ProfileFilter.Filter.ExhibitionLiked:
                    state = card.exhibition.Liked;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(filter), filter, null);
            }
        }
        
        card.gameObject.SetActive(state);
    }
    
    private void FilterGalleryCard(GalleryCard card, List<ProfileFilter.Filter> filters)
    {
        if (filters.Count <= 0)
        {
            card.gameObject.SetActive(true);
            return;
        }

        bool state = true;
        
        foreach (var filter in filters)
        {
            if (state == false)
            {
                card.gameObject.SetActive(false);
                return;
            }
            
            switch (filter)
            {
                case ProfileFilter.Filter.Any:
                    state = true;
                    break;
                case ProfileFilter.Filter.Demo:
                    state = card.sourceExhibition.Artist == "WAVA";
                    break;
                case ProfileFilter.Filter.StadelMuseum:
                    state = card.sourceExhibition.Location == "Frankfurt";
                    break;
                case ProfileFilter.Filter.ExhibitionLiked:
                    state = card.sourceExhibition.Liked;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(filter), filter, null);
            }
        }
        
        card.gameObject.SetActive(state);
    }
    
    private void FilterArtistContainer(ArtistContainer card, List<ProfileFilter.Filter> filters)
    {
        if (filters.Count <= 0)
        {
            card.gameObject.SetActive(true);
            return;
        }

        bool state = true;
        
        foreach (var filter in filters)
        {
            if (state == false)
            {
                card.gameObject.SetActive(false);
                return;
            }
            
            switch (filter)
            {
                case ProfileFilter.Filter.Any:
                    state = true;
                    break;
                case ProfileFilter.Filter.Demo:
                    state = card.artist.Title == "WAVA";
                    break;
                case ProfileFilter.Filter.StadelMuseum:
                    foreach (var exhCard in exhibitionCards)
                    {
                        if (exhCard.exhibition.Location == "Frankfurt" &&
                            exhCard.exhibition.Artist == card.artist.Title)
                        {
                            state = true;
                            break;
                        }

                        state = false;
                    }
                    break;
                case ProfileFilter.Filter.ExhibitionLiked:
                    foreach (var galleryCard in galleryCards)
                    {
                        if (galleryCard.arPoint.Artist == card.artist.Title && galleryCard.sourceExhibition.Liked)
                        {
                            state = true;
                            break;
                        }
                        state = false;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(filter), filter, null);
            }
        }
        
        card.gameObject.SetActive(state);
    }

    protected IEnumerator LateRebuild()
    {
        yield return new WaitForEndOfFrame();
        
        // Fixes issues with anchoring (Unity problem)
        LayoutRebuilder.ForceRebuildLayoutImmediate(artistLayoutTransform);
    }
}