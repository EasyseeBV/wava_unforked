using UnityEngine;
using UnityEngine.UI;

public class PhotoGalleryItemUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    Image photoPreviewImage;

    [SerializeField]
    Button openPhotoButton;

    public Sprite CachedSprite { get; private set; }
    public string Path { get; private set; }
    public bool IsARView { get; set; } = false;

    private void Awake()
    {
        openPhotoButton.onClick.AddListener(Open);
    }

    public void Init(Sprite sprite, string path)
    {
        Path = path;
        CachedSprite = sprite;
        photoPreviewImage.sprite = sprite;
    }

    private void Open()
    {
        if (!IsARView)
        {
            if (ProfileUIManager.Instance == null) return;
            
            ProfileUIManager.Instance.galleryItemDetailsUI.OpenDetailsPage();
            ProfileUIManager.Instance.galleryItemDetailsUI.SetPhotoSpriteToShow(CachedSprite);
        }
        else
        {
            if (ARPhotoViewer.Instance == null) return;
            
            ARPhotoViewer.Instance.Open(this);
        }
    }
}
