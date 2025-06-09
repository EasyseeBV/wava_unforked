using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AlmostEngine.Screenshot;
using TMPro;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.UI;

public class GalleryItemsInstantiator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private UserPhoto userPhotoPrefab;
    [SerializeField] private VideoGalleryItemUI _videoGalleryItemPrefab;
    [SerializeField] private Transform _galleryItemsContainer;
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
    private List<VideoGalleryItemUI> videos = new();
    
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
        if (!layoutAreasToRefresh.Contains(_galleryItemsContainer as RectTransform))
        {
            layoutAreasToRefresh.Add(_galleryItemsContainer as RectTransform);    
        }
        
        if (gameObject.activeInHierarchy) StartCoroutine(LoadAllMedia());
    }

    private IEnumerator LoadAllMedia()
    {
        string path = screenshotManager.GetExportPath();


        if (!Directory.Exists(path))
        {
            Debug.LogError("Directory does not exist: " + path);
            if (infoLabel) infoLabel.text = "No Images or Videos";
            refreshButton?.gameObject.SetActive(true);
            yield break;
        }
        else
        {
            if (infoLabel) infoLabel.text = "Loading...";
            refreshButton?.gameObject.SetActive(false);
        }
        
        if (infoLabel != null) infoLabel.text = "";
        
        string[] imageFiles = Directory.GetFiles(path, "*.png");
        string[] videoFiles = Directory.GetFiles(path, "*.mp4");

        // If nothing has changed (same number of images + videos as before), skip reloading
        int totalFileCount = imageFiles.Length + videoFiles.Length;
        int previousCount = photos.Count /*+ videos.Count if you track videos*/;
        if (totalFileCount == previousCount)
        {
            // Assume nothing changed—bail out
            yield break;
        }

        // Otherwise, wipe out old items
        foreach (var photo in photos)
        {
            photo.gameObject.SetActive(false);
        }
        photos.Clear();
        
        foreach (var vid in videos)
        {
             vid.gameObject.SetActive(false);
        }
        videos.Clear();

        // Load all image sprites asynchronously
        List<Sprite> sprites = new List<Sprite>();
        if (imageFiles.Length != AppCache.LocalGallery.Count)
        {
            // Load each image only if it's not already cached
            foreach (var imgPath in imageFiles)
            {
                yield return StartCoroutine(LoadImage(imgPath, sprites));
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
        if (countLabel)
            countLabel.text = "";

        // Instantiate gallery items for photos.
        for (int i = 0; i < sprites.Count; i++)
        {
            UserPhoto photo = Instantiate(userPhotoPrefab, _galleryItemsContainer);
            photo.Init(sprites[i], imageFiles[i]);
            photos.Add(photo);
        }

        // Instantiate gallery items for videos.
        for (int i = 0; i < videoFiles.Length; i++)
        {
            // Instantiate the video gallery item.
            var videoGalleryItem = Instantiate(_videoGalleryItemPrefab, _galleryItemsContainer);

            // Set its video.
            string videoPath = videoFiles[i];
            videoGalleryItem.SetVideoToShow(videoPath);
        }

        // Show the refresh button if no items are present.
        refreshButton?.gameObject.SetActive(videoFiles.Length == 0 && sprites.Count == 0);

        // Commented for now; don't see the purpose.
        //StartCoroutine(LateRebuild());
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
