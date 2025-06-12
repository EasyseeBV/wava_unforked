using UnityEngine;
using UnityEngine.UI;

public class PhotoItemUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    Image photoPreviewImage;

    [SerializeField]
    Button openPhotoButton;

    [SerializeField]
    AspectRatioFitter aspectRatioFitter;

    public Sprite CachedSprite { get; private set; }
    public string Path { get; private set; }
    public bool IsARView { get; set; } = false;

    private void Awake()
    {
        openPhotoButton.onClick.AddListener(Open);
    }

    public void Setup(Sprite sprite, string path)
    {
        Path = path;
        CachedSprite = sprite;
        photoPreviewImage.sprite = sprite;


        // Update the aspect ratio.
        aspectRatioFitter.aspectRatio = sprite.texture.width / (float)sprite.texture.height;
    }

    private void Open()
    {
        if (!IsARView)
        {
            if (ProfileUIManager.Instance == null) return;
            
            ProfileUIManager.Instance.galleryItemDetailsUI.OpenDetailsPage();
            ProfileUIManager.Instance.galleryItemDetailsUI.SetPhotoSpriteToShow(CachedSprite, Path);
        }
        else
        {
            if (ARPhotoViewer.Instance == null) return;
            
            ARPhotoViewer.Instance.Open(this);
        }
    }
}
