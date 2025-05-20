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

    [Header("Navigation Menus")]
    [SerializeField] private Button photosButton;
    [SerializeField] private TextMeshProUGUI photosLabel;
    [SerializeField] private Button favoritesButton;
    [SerializeField] private TextMeshProUGUI favoritesLabel;
    [SerializeField] private Transform menuBar;
    [SerializeField] private Color selectedColor;
    [SerializeField] private Color unselectedColor;
    [Space]
    [SerializeField] private GameObject photosMenuObject;
    [SerializeField] private PhotosPage photosPage;
    [SerializeField] private GameObject favoriteMenuObject;
    [SerializeField] private FavoritesPage favoritesPage;

    [Header("Details")]
    public ProfilePhotoDetails photoDetails;
    
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
        
        photosButton.onClick.AddListener(() => ChangeMenu(MenuNavigation.Photos));
        favoritesButton.onClick.AddListener(() => ChangeMenu(MenuNavigation.Favorites));
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

        menuBar.transform.localPosition = new Vector3(
            menu == MenuNavigation.Photos ? MENU_BAR_PHOTOS : MENU_BAR_LIKED,
            menuBar.transform.localPosition.y,
            menuBar.transform.localPosition.z);

        photosLabel.color = menu == MenuNavigation.Photos ? selectedColor : unselectedColor;
        favoritesLabel.color = menu == MenuNavigation.Favorites ? selectedColor : unselectedColor;

        switch (menu)
        {
            case MenuNavigation.Photos:
                favoritesPage.Close();
                favoriteMenuObject.SetActive(false);
                photosMenuObject.SetActive(true);
                photosPage.Open();
                // TODO: filter photos here, if it was wanted in the future
                break;
            case MenuNavigation.Favorites:
                photosPage.Close();
                photosMenuObject.SetActive(false);
                favoriteMenuObject.SetActive(true);
                favoritesPage.Open();
                favoritesPage.Filter(filters);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(menu), menu, null);
        }
        
        currentMenu = menu;
    }

    public void AddFilter(ProfileFilter.Filter filter)
    {
        if (filters.Contains(filter)) return;
        
        filters.Add(filter);
        favoritesPage.Filter(filters);
    }

    public void RemoveFilter(ProfileFilter.Filter filter)
    {
        if (!filters.Contains(filter)) return;

        filters.Remove(filter);
        favoritesPage.Filter(filters);
    }
}
