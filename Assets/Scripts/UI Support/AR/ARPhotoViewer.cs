using UnityEngine;
using UnityEngine.UI;
using UnityNative.Sharing;

public class ARPhotoViewer : MonoBehaviour
{
    public static ARPhotoViewer Instance;

    [SerializeField] private GameObject content;
    [SerializeField] private Image image;
    [SerializeField] private Button returnButton;
    [SerializeField] private Button shareButton;

    private PhotoGalleryItemUI userPhoto;
    private VideoGalleryItemUI userVideo;
    
    private void Awake()
    {
        if (!Instance) Instance = this;
        
        returnButton.onClick.AddListener(Return);
        shareButton.onClick.AddListener(Share);
        content.SetActive(false);
    }

    public void Open(PhotoGalleryItemUI photo)
    {
        content.SetActive(true);
        
        userPhoto = photo;
        image.sprite = photo.CachedSprite;
    }

    public void Open(VideoGalleryItemUI video)
    {
        content.SetActive(true);
        
        userVideo = video;
    }
    
    private void Return() => content.SetActive(false);

    private void Share()
    {
        if (!userPhoto) return;
        var share = UnityNativeSharing.Create();
        share.ShareScreenshotAndText("WAVA", userPhoto.Path);
    }
}
