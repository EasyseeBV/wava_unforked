using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AlmostEngine.Screenshot;
using TMPro;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class PhotosPage : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private UserPhoto userPhotoPrefab;
    [SerializeField] private Transform photosLayoutArea;
    [SerializeField] private TMP_Text infoLabel;
    [SerializeField] private TMP_Text countLabel;
    [SerializeField] private ContentSizeFitter contentSizeFitter;
    [SerializeField] private List<RectTransform> layoutAreasToRefresh = new();
    [SerializeField] private Button refreshButton;
    [SerializeField] private GameObject gallery;

    [Header("Storage Path Finder")]
    [SerializeField] private ScreenshotManager screenshotManager;
    
    [Header("Debugging")]
    [SerializeField] private bool skipRefreshAllLayouts;
    [SerializeField] private bool preload;
    [SerializeField] private Image closeButtonImage;
    
    private List<UserPhoto> photos = new();
    
    private void Awake()
    {
        refreshButton?.onClick.AddListener(() =>
        {
            if (!Permission.HasUserAuthorizedPermission("android.permission.READ_MEDIA_IMAGES"))
            {
                if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead))
                {
                    var callbacks = new PermissionCallbacks();
                    callbacks.PermissionDenied += PermissionCallbacks_PermissionDenied;
                    callbacks.PermissionGranted += PermissionCallbacks_PermissionGranted;
                    callbacks.PermissionDeniedAndDontAskAgain += PermissionCallbacks_PermissionDeniedAndDontAskAgain;
                    Permission.RequestUserPermission(Permission.ExternalStorageRead, callbacks);
                }
            }
            else
            {
                Open();   
            }
        });

        if (preload)
        {
            gallery.SetActive(true);
            closeButtonImage.enabled = false;
            skipRefreshAllLayouts = false;
            Open();
        }
    }
    
    private void PermissionCallbacks_PermissionDeniedAndDontAskAgain(string permissionName)
    {
        Debug.Log($"{permissionName} PermissionDeniedAndDontAskAgain");
    }

    private void PermissionCallbacks_PermissionGranted(string permissionName)
    {
        Debug.Log($"{permissionName} PermissionCallbacks_PermissionGranted");
        Open();
    }

    private void PermissionCallbacks_PermissionDenied(string permissionName)
    {
        Debug.Log($"{permissionName} PermissionCallbacks_PermissionDenied");
    }

    public void Open()
    {
        if (!layoutAreasToRefresh.Contains(photosLayoutArea as RectTransform))
        {
            layoutAreasToRefresh.Add(photosLayoutArea as RectTransform);    
        }
        
        if(gameObject.activeInHierarchy) StartCoroutine(LoadAllImages());
    }

    public void Close()
    {
        foreach (var photo in photos)
        {
            
        }
    }

    IEnumerator LoadAllImages()
    {
        string path = screenshotManager.GetExportPath();
        if (!Directory.Exists(path))
        {
            Debug.LogError("Directory does not exist: " + path);
            
            if (infoLabel) infoLabel.text = "No Images";
            refreshButton?.gameObject.SetActive(true);
            
            yield break;
        }
        else
        {
            if (infoLabel) infoLabel.text = "Loading...";
            refreshButton?.gameObject.SetActive(false);
        }

        if (infoLabel != null)
        {
            infoLabel.text = "";
        }
        
        string[] files = Directory.GetFiles(path, "*.png");
        if (files.Length != photos.Count)
        {
            foreach (var photo in photos)
            {
                photo.gameObject.SetActive(false);
            }

            photos.Clear();
        }
        else yield break; // all images are already loaded
        
        List<Sprite> sprites = new List<Sprite>();

        if (files.Length != AppCache.LocalGallery.Count)
        {
            foreach (var t in files)
            {
                yield return StartCoroutine(LoadImage(t, sprites));
            }
        }
        else
        {
            sprites = AppCache.LocalGallery.Values.ToList();
        }
        
        if (infoLabel != null)
        {
            infoLabel.text = "";
        }
        
        if (countLabel) countLabel.text = "";

        int count = 0;

        for (int i = 0; i < sprites.Count; i++)
        {
            UserPhoto photo = Instantiate(userPhotoPrefab, photosLayoutArea);
            photo.Init(sprites[i], files[i]);
            photos.Add(photo);
            LayoutRebuilder.ForceRebuildLayoutImmediate(photosLayoutArea as RectTransform); // Force immediate rebuild
        }

        if(sprites.Count <= 0) refreshButton?.gameObject.SetActive(true);
        StartCoroutine(LateRebuild());
    }

    IEnumerator LoadImage(string filePath, List<Sprite> sprites)
    {
        if (AppCache.LocalGallery.ContainsKey(filePath))
        {
            sprites.Add(AppCache.LocalGallery[filePath]);
            yield break;
        }
        
        byte[] fileData = File.ReadAllBytes(filePath);
        Texture2D texture = new Texture2D(2, 2); // Create a temporary texture
        texture.LoadImage(fileData); // Load the image data into the texture

        yield return null;

        // Convert texture to sprite
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        sprite.name = Path.GetFileName(filePath); // Name the sprite after the file

        AppCache.LocalGallery.Add(filePath, sprite);
        
        sprites.Add(sprite);
    }
    
    protected IEnumerator LateRebuild()
    {
        yield return new WaitForEndOfFrame();
        
        // Disable and re-enable the ContentSizeFitter to force a refresh
        if (contentSizeFitter != null)
        {
            contentSizeFitter.enabled = false;
            yield return null; // Wait one frame
            contentSizeFitter.enabled = true;
        }
        
        foreach (var l in layoutAreasToRefresh)
        {
            if (skipRefreshAllLayouts) continue;
            yield return new WaitForEndOfFrame();
            LayoutRebuilder.ForceRebuildLayoutImmediate(l);
        }

        yield return new WaitForEndOfFrame();
        
        if (preload)
        {
            preload = false;
            skipRefreshAllLayouts = true;
            closeButtonImage.enabled = true;
            gallery.SetActive(false);
        }
    }
}
