using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AlmostEngine.Screenshot;
using UnityEngine;
using UnityEngine.UI;

public class ARGalleryPage : AnimateInfoBar
{
    [Header("AR Gallery")]
    [SerializeField] private Button toggleButton;
    [SerializeField] private GameObject content;
    [SerializeField] private RectTransform layout;
    [SerializeField] private RectTransform background;
    [SerializeField] private RectTransform galleryParent;
    [SerializeField] private UserPhoto userPhoto;
    [SerializeField] private ScreenshotManager screenshotManager;
    
    private List<UserPhoto> photos = new List<UserPhoto>();
    private Coroutine coroutine;

    private float backgroundHeight = 0;
    private float galleryParentHeight = 0;
    
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
            if (coroutine != null) StopCoroutine(coroutine);
            content.SetActive(true);
            coroutine = StartCoroutine(ShowPhotos());
        }
    }

    protected override void StopRectAnmation(bool hidden)
    {
        base.StopRectAnmation(hidden);
        if(hidden) content.SetActive(false);
    }

    private IEnumerator ShowPhotos()
    {
        string path = screenshotManager.GetExportPath();
        
        if (!Directory.Exists(path))
        {
            Debug.Log("Could not open gallery as the path did not exist");
            yield break;
        }
        
        // Get all files
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
        
        // Collect all images
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

        for (int i = 0; i < sprites.Count; i++)
        {
            UserPhoto photo = Instantiate(userPhoto, layout);
            photo.Init(sprites[i], files[i]);
            photo.IsARView = true;
            photos.Add(photo);
        }

        if (photos.Count > 0)
        {
            int rows = Mathf.CeilToInt( photos.Count / 3f );
            float extra = (175f + 10) * rows;
            
            background.sizeDelta = new Vector2(
                background.sizeDelta.x,
                backgroundHeight + extra
            );

            galleryParent.sizeDelta = new Vector2(
                galleryParent.sizeDelta.x,
                galleryParentHeight + extra
            );
        }

    }
    
    private IEnumerator LoadImage(string filePath, List<Sprite> sprites)
    {
        if (AppCache.LocalGallery.TryGetValue(filePath, out var value))
        {
            sprites.Add(value);
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

}
