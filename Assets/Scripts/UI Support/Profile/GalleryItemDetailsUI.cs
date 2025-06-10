using AlmostEngine.Screenshot;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityNative.Sharing;

public class GalleryItemDetailsUI : MonoBehaviour
{
    [SerializeField]
    GameObject detailsContainer;

    [SerializeField]
    TextMeshProUGUI mediaNameText;

    [SerializeField]
    Image photoImage;

    [SerializeField]
    AspectRatioFitter photoAspectRatioFitter;

    [SerializeField]
    AdvancedVideoPlayer advancedVideoPlayer;

    [SerializeField]
    Button shareButton;

    [SerializeField]
    Button backButton;

    /*
    [Header("Like Buttons")]
    [SerializeField] protected Button heartButton;
    [SerializeField] protected Image heartImage;
    [SerializeField] protected Sprite unlikedSprite;
    [SerializeField] protected Sprite likedSprite;
    */

    private string savedName = null;

    private PhotoGalleryItemUI openedUserPhoto;

    /*
    private void Awake()
    {
        heartButton.onClick.AddListener(ToggleLike);
        shareButton.onClick.AddListener(Share);
    }
    */

    private void OnEnable()
    {
        backButton.onClick.AddListener(CloseDetailsPage);
    }

    private void OnDisable()
    {
        backButton.onClick.RemoveListener(CloseDetailsPage);
    }


    public void Open(PhotoGalleryItemUI userPhoto)
    {


        /*
        openedUserPhoto = userPhoto;
        savedName = userPhoto.CachedSprite.name;
        photoLabel.text = savedName;
        photoImage.sprite = userPhoto.CachedSprite;
        SetLiked(PlayerPrefs.GetInt(userPhoto.CachedSprite.name, 0));
        Debug.Log("Can share: " + ShareUtils.CanShare());
        */
    }


    public void OpenDetailsPage()
    {
        detailsContainer.SetActive(true);
    }

    public void CloseDetailsPage()
    {
        detailsContainer.SetActive(false);
    }

    public void SetPhotoSpriteToShow(Sprite sprite)
    {
        // Hide the advanced video player.
        advancedVideoPlayer.gameObject.SetActive(false);

        // Show the image.
        photoImage.gameObject.SetActive(true);
        photoImage.sprite = sprite;

        // Set the correct aspect ratio for the image.
        var aspectRatio = sprite.texture.width / (float)sprite.texture.height;
        photoAspectRatioFitter.aspectRatio = aspectRatio;
    }

    public void SetPathOfVideoToShow(string videoPath)
    {
        // Hide the image.
        photoImage.gameObject.SetActive(false);

        // Show the advanced video player and set the video.
        advancedVideoPlayer.gameObject.SetActive(true);
        advancedVideoPlayer.SetVideoPath(videoPath);
    }

    /*
    private void ToggleLike()
    {
        int liked = PlayerPrefs.GetInt(savedName, 0);
        PlayerPrefs.SetInt(savedName, liked == 0 ? 1 : 0);
        PlayerPrefs.Save();
        SetLiked(liked == 0 ? 1 : 0);
    }
    */

    /*
    private void SetLiked(int n)
    {
        heartImage.sprite = n == 0 ? unlikedSprite : likedSprite;
    }
    */

    private void Share()
    {
        var share = UnityNativeSharing.Create();
        share.ShareScreenshotAndText("WAVA", openedUserPhoto.Path);
        //UnityNativeSharing.ShareScreenshotAndText("WAVA", Path.GetFileName(openedUserPhoto.Path), true);
        //ShareUtils.ShareImage(openedUserPhoto.CachedSprite.texture, Path.GetFileName(openedUserPhoto.Path), "Screenshot taken from WAVA.");
    }

}