using UnityEngine;
using UnityEngine.UI;

public class OpenURLButton : MonoBehaviour
{
    [SerializeField] private string url;
    [SerializeField] private Button urlButton;

    private void Awake()
    {
        urlButton.onClick.AddListener(() => Application.OpenURL(url));
    }

    private void OnValidate()
    {
        if (urlButton == null)
        {
            urlButton = GetComponent<Button>();
            if (urlButton == null)
            {
                urlButton = GetComponentInChildren<Button>();
            }
        }
    }
}
