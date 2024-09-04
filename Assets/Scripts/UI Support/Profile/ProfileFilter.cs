using System;
using System.Collections;
using System.Collections.Generic;
using Messy.Definitions;
using UnityEngine;
using UnityEngine.UI;

public class ProfileFilter : MonoBehaviour
{
    [Serializable]
    public enum Filter
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
        public Filter filter;
    }

    [Header("Filters")]
    [SerializeField] private List<FilterItem> filterItems;

    [Header("Dependencies")]
    [SerializeField] private ProfileUIManager profileUIManager;

    private void Awake()
    {
        foreach (var filterItem in filterItems)
        {
            filterItem.itemToggle.onValueChanged.AddListener(state => AddFilter(filterItem, state));
        }
    }

    private void AddFilter(FilterItem filterItem, bool state)
    {
        if (state) profileUIManager.AddFilter(filterItem.filter);
        else profileUIManager.RemoveFilter(filterItem.filter);
    }
}