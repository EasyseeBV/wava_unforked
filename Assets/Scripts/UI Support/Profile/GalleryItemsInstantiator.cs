using AlmostEngine.Screenshot;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.UI;

public class GalleryItemsInstantiator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PhotoItemUI photoItemPrefab;
    [SerializeField] private VideoItemUI _videoItemPrefab;
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
    
    private List<PhotoItemUI> photoItemUIs = new();
    private List<VideoItemUI> videoItemUIs = new();

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
        
        if (gameObject.activeInHierarchy) StartCoroutine(LoadMediaFilesAndUpdateUI());
    }

    private IEnumerator LoadMediaFilesAndUpdateUI()
    {
        // Load all photo sprites.
        var photoSprites = new List<Sprite>();

        // - Retrieve the names of all photo files.
        var photoPaths = new string[0];

        // - - Check if the directory exists.
        if (Directory.Exists(AppCache.GalleryFolder))
        { // - The directory exists; get the file names.
            photoPaths = Directory.GetFiles(AppCache.GalleryFolder, "*.png");
        }

        // - Retrieve sprite for each photo file path.
        // - - If the number of file names matches the number of cached sprites then we expect them to be the same.
        if (photoPaths.Length == AppCache.LocalGallery.Count)
        {
            photoSprites = AppCache.LocalGallery.Values.ToList();
        }
        else
        {
            // Load each image individually.
            foreach (var path in photoPaths)
            {
                yield return LoadImage(path, photoSprites);
            }
        }


        // Read the video paths.
        var videoPaths = Directory.GetFiles(AppCache.GalleryFolder, "*.mp4").ToList();//VideoPathStore.ReadPaths();

        /*
        // TODO: REMOVE THE FOLLOWING HACKY TEST
        var testPath = "file:///storage/emulated/0/Android/media/com.wava.ar.game/WAVA/WavaTest.mp4";

        if (!videoPaths.Contains(testPath))
        {
            VideoPathStore.StorePath(testPath);
            videoPaths = VideoPathStore.ReadPaths();
        }
        */


        // Convert each path that is an android media store uri alias into a real media store uri.
        for (int i = 0; i < videoPaths.Count; i++)
        {
            //Debug.Log($"Found this video path: {videoPaths[i]}");

            videoPaths[i] = VideoPathStore.ConvertToMediaStoreURIIfAlias(videoPaths[i]);

            //Debug.Log($"Converted the video path to: {videoPaths[i]}");
        }


        // Hide or show the refresh button.
        if (photoPaths.Length + videoPaths.Count == 0)
        {
            infoLabel.gameObject.SetActive(true);
            refreshButton.gameObject.SetActive(true);
        }
        else
        {
            infoLabel.gameObject.SetActive(false);
            refreshButton.gameObject.SetActive(false);
        }


        // Update the photo ui items.
        for (int i = 0; i < photoSprites.Count; i++)
        {
            var sprite = photoSprites[i];
            var photoPath = photoPaths[i];

            // If a ui item exists: reuse it.
            if (i < photoItemUIs.Count)
            {
                var photoItemUI = photoItemUIs[i];
                photoItemUI.gameObject.SetActive(true);
                photoItemUI.Setup(sprite, photoPath);
            }
            else
            {
                // Otherwise: Instantiate a new UI element.
                var photoItemUI = Instantiate(photoItemPrefab, _galleryItemsContainer);

                photoItemUI.Setup(sprite, photoPath);

                // Store the element.
                photoItemUIs.Add(photoItemUI);
            }
        }

        // - Hide all photo ui elements that are too many.
        for (int i = photoSprites.Count; i < photoItemUIs.Count; i++)
        {
            var photoItemUI = photoItemUIs[i];
            photoItemUI.gameObject.SetActive(false);
        }



        // Update the video ui items.
        for (int i = 0; i < videoPaths.Count; i++)
        {
            var videoPath = videoPaths[i];

            // Reuse the video item if it exists.
            if (i < videoItemUIs.Count)
            {
                var videoItemUI = videoItemUIs[i];
                videoItemUI.gameObject.SetActive(true);
                videoItemUI.SetVideoToShow(videoPath);
            }
            else
            {
                // Otherwise: Instantiate a new UI element.
                var videoItemUI = Instantiate(_videoItemPrefab, _galleryItemsContainer);

                videoItemUI.SetVideoToShow(videoPath);

                // Store the element.
                videoItemUIs.Add(videoItemUI);
            }
        }

        // - Hide all video ui elements that are too many.
        for (int i = videoPaths.Count; i < videoItemUIs.Count; i++)
        {
            var videoitemUI = videoItemUIs[i];
            videoitemUI.gameObject.SetActive(false);
        }



        // Commented for now; don't see the purpose.
        //StartCoroutine(LateRebuild());
    }

    /// <summary>
    /// If a file with the specified path is cached then it's added to the sprites list. If not, creates a sprite, caches it, then adds it to the list.
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="sprites"></param>
    /// <returns></returns>
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
