using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AlmostEngine.Screenshot;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ARGalleryPage : AnimateInfoBar
{
    [Header("AR Gallery")]
    [SerializeField] private Button toggleButton;
    [SerializeField] private GameObject content;
    [SerializeField] private RectTransform layout;
    [SerializeField] private RectTransform background;
    [SerializeField] private RectTransform galleryParent;
    [SerializeField] private PhotoItemUI userPhoto;
    [SerializeField] private ScreenshotManager screenshotManager;
    
    private List<PhotoItemUI> photos = new List<PhotoItemUI>();
    private Coroutine coroutine;

    private float backgroundHeight = 0;
    private float galleryParentHeight = 0;
    public static string StoragePath = string.Empty;
    
    protected override void Awake()
    {
        base.Awake();
        backgroundHeight = background.rect.height;
        galleryParentHeight = galleryParent.rect.height;
        toggleButton.onClick.AddListener(TogglePage);
    }

    private void TogglePage()
    {
        Animate();
    }

    protected override void StartRectAnimation(bool hide)
    {
        base.StartRectAnimation(hide);
        if (!hide)
        {
            content.SetActive(true);

            ShowPhotos();
        }
    }

    protected override void StopRectAnmation(bool hidden)
    {
        base.StopRectAnmation(hidden);
        if(hidden) content.SetActive(false);
    }

    private async void ShowPhotos()
    {
        if (StoragePath == string.Empty) StoragePath = screenshotManager.GetExportPath();
        var files = Directory.GetFiles(StoragePath, "*.png");
        
        if (files.Length == photos.Count) return;

        try
        {
            // clear out
            foreach (var p in photos) Destroy(p.gameObject);
            photos.Clear();

            // kick off all loads in parallel
            var loadTasks = files.Select(f => LoadSpriteAsync(f)).ToArray();

            // wait for them all
            Sprite[] sprites = await Task.WhenAll(loadTasks);

            // instantiate on the main thread
            for (int i = 0; i < sprites.Length; i++)
            {
                var photo = Instantiate(userPhoto, layout);
                photo.Setup(sprites[i], files[i]);
                photo.IsARView = true;
                photos.Add(photo);
            }

            LayoutPhotos();
        }
        catch (Exception e)
        {
            content.gameObject.SetActive(false);
            Debug.LogError($"Error loading photos: {e.Message}");
        }
    }
    
    private async Task<Sprite> LoadSpriteAsync(string filePath)
    {
        if (AppCache.LocalGallery.TryGetValue(filePath, out var spr))
        {
            return spr;
        }
        
        byte[] data = await File.ReadAllBytesAsync(filePath);
        var tex = new Texture2D(2, 2);
        tex.LoadImage(data);
        var sprite = Sprite.Create(
            tex,
            new Rect(0, 0, tex.width, tex.height),
            new Vector2(0.5f, 0.5f)
        );
        sprite.name = Path.GetFileName(filePath);
        AppCache.LocalGallery[filePath] = sprite;
        return sprite;
    }

    private void LayoutPhotos()
    {
        int rows = Mathf.CeilToInt( photos.Count / 3f );
        float extra = (175f + 10) * rows;
        background.sizeDelta = new Vector2(background.sizeDelta.x,
            backgroundHeight + extra);
        galleryParent.sizeDelta = new Vector2(galleryParent.sizeDelta.x,
            galleryParentHeight + extra);
    }
}
