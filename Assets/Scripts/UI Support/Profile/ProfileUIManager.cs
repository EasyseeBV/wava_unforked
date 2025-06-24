using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Android;

public class ProfileUIManager : MonoBehaviour
{
    public static ProfileUIManager Instance;
    
    private enum MenuNavigation
    {
        Default,
        Photos,
        Favorites
    }

    [Header("Details")]
    public ItemDetailsUI galleryItemDetailsUI;
    
    private MenuNavigation currentMenu = MenuNavigation.Default;
    
    private const float MENU_BAR_PHOTOS = -171.5f;
    private const float MENU_BAR_LIKED = 0;

    private List<ProfileFilter.Filter> filters = new();

    private void Awake()
    {
        if (!Instance) Instance = this;
        else Destroy(gameObject);
        
#if UNITY_ANDROID
        if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead))
        {
            var callbacks = new PermissionCallbacks();
            callbacks.PermissionDenied += PermissionCallbacks_PermissionDenied;
            callbacks.PermissionGranted += PermissionCallbacks_PermissionGranted;
            callbacks.PermissionDeniedAndDontAskAgain += PermissionCallbacks_PermissionDeniedAndDontAskAgain;
            Permission.RequestUserPermission(Permission.ExternalStorageRead, callbacks);
        }
        
#elif UNITY_IOS
        if (!Application.HasUserAuthorization(UserAuthorization.WebCam))
        {
            Application.RequestUserAuthorization(UserAuthorization.WebCam);
        }
#endif
        
        ChangeMenu(MenuNavigation.Photos);
    }
    
    private void PermissionCallbacks_PermissionDeniedAndDontAskAgain(string permissionName)
    {
        Debug.Log($"{permissionName} PermissionDeniedAndDontAskAgain");
    }

    private void PermissionCallbacks_PermissionGranted(string permissionName)
    {
        Debug.Log($"{permissionName} PermissionCallbacks_PermissionGranted");
    }

    private void PermissionCallbacks_PermissionDenied(string permissionName)
    {
        Debug.Log($"{permissionName} PermissionCallbacks_PermissionDenied");
    }
    
    private void ChangeMenu(MenuNavigation menu)
    {
        if (currentMenu == menu) return;
        
        currentMenu = menu;
    }

    public void AddFilter(ProfileFilter.Filter filter)
    {
        if (filters.Contains(filter)) return;
        
        filters.Add(filter);
    }

    public void RemoveFilter(ProfileFilter.Filter filter)
    {
        if (!filters.Contains(filter)) return;

        filters.Remove(filter);
    }
}
