using System;
using System.Collections;
using System.Collections.Generic;
using Messy.Definitions;
using UnityEngine;
using UnityEngine.UI;

public class GalleryFilter : MonoBehaviour
{
    public enum Filter
    {
        Any,
        NewestToOldest,
        OldestToNewest,
        Exhibitions,
        Location,
        RecentlyAdded
    }

    [Serializable]
    private class FilterItem
    {
        public Toggle itemToggle;
        public Filter filter;
    }

    [Header("Dependencies")]
    [SerializeField] private ArtworkUIManager artworkUIManager;
    
    [Header("Filters")]
    [SerializeField] private List<FilterItem> filterItems;
    
    private bool filterSwapping = false;

    private void Awake()
    {
        foreach (var filterItem in filterItems)
        {
            filterItem.itemToggle.onValueChanged.AddListener(state =>
            {
                if (filterSwapping) return;

                filterSwapping = true;
                
                foreach (var item in filterItems)
                {
                    if (filterItem != item)
                    {
                        item.itemToggle.isOn = false;
                    }
                }

                artworkUIManager.CurrentFilter = filterItem.filter;

                artworkUIManager.ApplySorting();
                
                filterSwapping = false;
            });
        }
    }
}
