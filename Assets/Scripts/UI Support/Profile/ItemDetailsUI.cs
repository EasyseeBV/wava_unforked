using AlmostEngine.Screenshot;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityNative.Sharing;
using NativeShareNamespace;

public class ItemDetailsUI : MonoBehaviour
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

    [SerializeField]

    string mediaFilePath;

    /*
    [Header("Like Buttons")]
    [SerializeField] protected Button heartButton;
    [SerializeField] protected Image heartImage;
    [SerializeField] protected Sprite unlikedSprite;
    [SerializeField] protected Sprite likedSprite;
    */

    private string savedName = null;

    private PhotoItemUI openedUserPhoto;

    private void Awake()
    {
        //heartButton.onClick.AddListener(ToggleLike);
        shareButton.onClick.AddListener(Share);
    }


    private void OnEnable()
    {
        backButton.onClick.AddListener(CloseDetailsPage);
    }

    private void OnDisable()
    {
        backButton.onClick.RemoveListener(CloseDetailsPage);
    }


    public void Open(PhotoItemUI userPhoto)
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

    public void SetPhotoSpriteToShow(Sprite sprite, string filePath)
    {
        // Hide the advanced video player.
        advancedVideoPlayer.gameObject.SetActive(false);

        // Show the image.
        photoImage.gameObject.SetActive(true);
        photoImage.sprite = sprite;

        // Set the correct aspect ratio for the image.
        var aspectRatio = sprite.texture.width / (float)sprite.texture.height;
        photoAspectRatioFitter.aspectRatio = aspectRatio;

        // Store the media path for social sharing.
        mediaFilePath = filePath;
    }

    public void SetPathOfVideoToShow(string videoPath)
    {
        // Hide the image.
        photoImage.gameObject.SetActive(false);

        // Show the advanced video player and set the video.
        advancedVideoPlayer.gameObject.SetActive(true);
        advancedVideoPlayer.SetVideoPath(videoPath);

        // Store the media path for social sharing.
        mediaFilePath = videoPath;
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

    void Share()
    {
        Debug.Log($"Pressed share button! Attempting to share media at path: {mediaFilePath}");

        new NativeShare().AddFile(mediaFilePath).Share();

        //var share = UnityNativeSharing.Create();
        //share.ShareScreenshotAndText("WAVA", openedUserPhoto.Path);

        //UnityNativeSharing.ShareScreenshotAndText("WAVA", Path.GetFileName(openedUserPhoto.Path), true);
        //ShareUtils.ShareImage(openedUserPhoto.CachedSprite.texture, Path.GetFileName(openedUserPhoto.Path), "Screenshot taken from WAVA.");
    }
}