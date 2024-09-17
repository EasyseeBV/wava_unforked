using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PhotosPage : MonoBehaviour
{
    [SerializeField] private UserPhoto userPhotoPrefab;
    [SerializeField] private Transform photosLayoutArea;
    [SerializeField] private TMP_Text infoLabel;
    [SerializeField] private TMP_Text countLabel;
    [SerializeField] private ContentSizeFitter contentSizeFitter;
    [SerializeField] private List<RectTransform> layoutAreasToRefresh = new();
    [SerializeField] private Button refreshButton;

    private const string FOLDER_PATH = "/storage/emulated/0/Pictures/WAVA/";

    private void Awake()
    {
        refreshButton?.onClick.AddListener(Open);
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
        foreach (Transform t in photosLayoutArea)
        {
            if(t.GetComponent<UserPhoto>()) t.gameObject.SetActive(false);
        }
    }

    IEnumerator LoadAllImages()
    {
        string path = FOLDER_PATH;
        if (!Directory.Exists(path))
        {
            Debug.LogError("Directory does not exist: " + path);
            
            if (countLabel) countLabel.text = "image count: failed";
            if (infoLabel) infoLabel.text = "No Images";
            refreshButton?.gameObject.SetActive(true);
            
            yield break;
        }
        else
        {
            if (countLabel) countLabel.text = "Loading...";
            if (infoLabel) infoLabel.text = "Loading...";
            refreshButton?.gameObject.SetActive(false);
        }

        if (infoLabel != null)
        {
            infoLabel.text = "";
        }
        
        string[] files = Directory.GetFiles(path, "*.png"); // Assuming PNG images, you can add other formats if needed.
        List<Sprite> sprites = new List<Sprite>();
        
        foreach (var t in files)
        {
            yield return StartCoroutine(LoadImage(t, sprites));
        }

        if (infoLabel != null)
        {
            infoLabel.text = "";
        }

        int count = 0;

        foreach (var sprite in sprites)
        {
            count++;
            UserPhoto photo = Instantiate(userPhotoPrefab, photosLayoutArea);
            photo.Init(sprite);
            LayoutRebuilder.ForceRebuildLayoutImmediate(photosLayoutArea as RectTransform); // Force immediate rebuild
        }

        if(count <= 0) refreshButton.gameObject.SetActive(true);
        //if (countLabel) countLabel.text = "image count: " + count;
        StartCoroutine(LateRebuild());
    }

    IEnumerator LoadImage(string filePath, List<Sprite> sprites)
    {
        byte[] fileData = File.ReadAllBytes(filePath);
        Texture2D texture = new Texture2D(2, 2); // Create a temporary texture
        texture.LoadImage(fileData); // Load the image data into the texture

        yield return null;

        // Convert texture to sprite
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        sprite.name = Path.GetFileName(filePath); // Name the sprite after the file

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
            LayoutRebuilder.ForceRebuildLayoutImmediate(l);
        }
    }
}
