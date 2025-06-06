using AlmostEngine.Screenshot;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using UnityNative.Sharing;

public class ProfilePhotoDetails : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI photoLabel;
    [SerializeField] private Image photoImage;
    [SerializeField] private Button shareButton;

    [Header("Like Buttons")]
    [SerializeField] protected Button heartButton;
    [SerializeField] protected Image heartImage;
    [SerializeField] protected Sprite unlikedSprite;
    [SerializeField] protected Sprite likedSprite;

    private string savedName = null;

    private UserPhoto openedUserPhoto;

    private void Awake()
    {
        heartButton.onClick.AddListener(ToggleLike);
        shareButton.onClick.AddListener(Share);
    }

    public void Open(UserPhoto userPhoto)
    {
        openedUserPhoto = userPhoto;
        savedName = userPhoto.CachedSprite.name;
        photoLabel.text = savedName;
        photoImage.sprite = userPhoto.CachedSprite;
        SetLiked(PlayerPrefs.GetInt(userPhoto.CachedSprite.name, 0));
        Debug.Log("Can share: " + ShareUtils.CanShare());
    }

    public void SetVideoPlayerVideo(UserVideo userVideo)
    {
        // Open the view for the video player.

    }

    private void ToggleLike()
    {
        int liked = PlayerPrefs.GetInt(savedName, 0);
        PlayerPrefs.SetInt(savedName, liked == 0 ? 1 : 0);
        PlayerPrefs.Save();
        SetLiked(liked == 0 ? 1 : 0);
    }

    private void SetLiked(int n)
    {
        heartImage.sprite = n == 0 ? unlikedSprite : likedSprite;
    }

    private void Share()
    {
        var share = UnityNativeSharing.Create();
        share.ShareScreenshotAndText("WAVA", openedUserPhoto.Path);
        //UnityNativeSharing.ShareScreenshotAndText("WAVA", Path.GetFileName(openedUserPhoto.Path), true);
        //ShareUtils.ShareImage(openedUserPhoto.CachedSprite.texture, Path.GetFileName(openedUserPhoto.Path), "Screenshot taken from WAVA.");
    }
}