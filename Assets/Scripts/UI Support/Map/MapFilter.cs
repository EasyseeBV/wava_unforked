using System;
using System.Collections.Generic;
using System.Linq;
using Messy.Definitions;
using UnityEngine;
using UnityEngine.UI;

public class MapFilter : MonoBehaviour
{
    private enum Filter
    {
        Any,
        Demo,
        StadelMuseum,
        ExhibitionLiked
    }

    [Serializable]
    private class FilterItem
    {
        public Toggle itemToggle;
        public Button itemButton;
        public Filter filter;
    }

    [Header("Filters")]
    [SerializeField] private List<FilterItem> filterItems;

    [Header("Image References")]
    [SerializeField] private Image filterIcon;
    [SerializeField] private Sprite noFiltersAppliedSprite;
    [SerializeField] private Sprite filtersAppliedSprite;
    
    private List<Filter> activeFilters = new();

    private void Awake()
    {
        foreach (var filterItem in filterItems)
        {
            filterItem.itemToggle.onValueChanged.AddListener(state => AddFilter(filterItem, state));
            filterItem.itemButton.onClick.AddListener(() =>
            {
                filterItem.itemToggle.isOn = false;
            });
            
            filterItem.itemButton.gameObject.SetActive(false);
        }
    }

    private void AddFilter(FilterItem filterItem, bool state)
    {
        if (state) activeFilters.Add(filterItem.filter);
        else activeFilters.Remove(filterItem.filter);
        
        filterItem.itemButton.gameObject.SetActive(state);

        filterIcon.sprite = activeFilters.Count > 0 ? filtersAppliedSprite : noFiltersAppliedSprite;
        
        FilterMap();
    }

    private void FilterMap()
    {
        foreach (var exhibition in FirebaseLoader.Exhibitions)
        {
            foreach (var artwork in exhibition.artworks)
            {
                FilterArtwork(exhibition, artwork);
            }
        }
    }

    private void FilterArtwork(ExhibitionData exhibition, ArtworkData artwork)
    {
        if (activeFilters.Count <= 0)
        {
            artwork.marker.instance.gameObject.SetActive(true);
            return;
        }

        bool state = true;
        
        foreach (var filter in activeFilters)
        {
            if (state == false)
            {
                artwork.marker.instance.SetActive(false);
                return;
            }
            
            switch (filter)
            {
                case Filter.Any:
                    state = true;
                    break;
                case Filter.Demo:
                    
                    state = exhibition.artists.Any(a => a.title.Equals("WAVA", System.StringComparison.OrdinalIgnoreCase));;
                    break;
                case Filter.StadelMuseum:
                    state = artwork.location == "Frankfurt";
                    break;
                case Filter.ExhibitionLiked:
                    //state = exhibition.Liked;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(filter), filter, null);
            }
        }
        
        artwork.marker.instance.SetActive(state);
    }
}
