using AlmostEngine.Screenshot;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;

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
    private Sprite cachedSprite = null;

    private void Awake()
    {
        heartButton.onClick.AddListener(ToggleLike);
        shareButton.onClick.AddListener(Share);
    }

    public void Open(Sprite sprite)
    {
        cachedSprite = sprite;
        savedName = sprite.name;
        photoLabel.text = savedName;
        photoImage.sprite = sprite;
        SetLiked(PlayerPrefs.GetInt(sprite.name, 0));
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
        Texture2D texture = cachedSprite.texture;

        // Encode texture to PNG
        byte[] pngData = texture.EncodeToPNG();
        
        // Save the image to persistent data path
        string filePath =
            ScreenshotNameParser.ParsePath(ScreenshotNameParser.DestinationFolder.PICTURES_FOLDER, "WAVA");// Path.Combine(PhotosPage.FolderPath, cachedSprite.name);
        File.WriteAllBytes(filePath, pngData);

        // Use Native Share to share the image
        ShareUtils.ShareImage(texture, cachedSprite.name, "WAVA");
        //ShareUtils.ShareImage(cachedSprite.texture, cachedSprite.name, "WAVA", "Screenshot taken from WAVA");
    }
}