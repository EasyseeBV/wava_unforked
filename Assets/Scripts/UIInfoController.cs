using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class UIInfoController : MonoBehaviour 
{
    private static UIInfoController _instance;
    public new static UIInfoController Instance {
        get {
            return _instance;
        }
    }

    public TextMeshProUGUI InfoBar;
    public Image Logo;

    [Header("Bottom Image Area")] 
    [SerializeField] private GameObject objectBottom;
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI textfieldBottom;
    [SerializeField] private RawImage additionalImageBottom;
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private RectTransform layoutTransform;
    [SerializeField] private Button interactionButton;

    [Header("Bottom default")] 
    [SerializeField] private GameObject bottomDefaultArea;
    [SerializeField] private TextMeshProUGUI bottomDefaultText;

    [Header("References")] 
    public ARTutorialManager arTutorialManager;
    
    private void Awake() {
        _instance = this;
        _instance.transform.GetChild(0).gameObject.SetActive(false);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="textInfo">Fill the text in here</param>
    /// <param name="IconShow"> Tell which icon should be visible (0 = none, 1 = most left, 2 = more to the right, 3 = all the way to the right</param>
    public void SetText(string textInfo, int IconShow) {
        if (textInfo == "") {
            _instance.transform.GetChild(0).gameObject.SetActive(false);
            return;
        } else {
            _instance.transform.GetChild(0).gameObject.SetActive(true);
        }


        InfoBar.text = textInfo;

        switch (IconShow) {
            case 0:
                Logo.enabled = false;
                break;
            case 1:
                Logo.enabled = true;
                Logo.GetComponent<RectTransform>().anchoredPosition = new Vector2(89, 0);
                break;
            case 2:
                Logo.GetComponent<RectTransform>().anchoredPosition = new Vector2(33, 0);
                Logo.enabled = true;
                break;
            case 3:
                Logo.GetComponent<RectTransform>().anchoredPosition = new Vector2(77, 0);
                Logo.enabled = true;
                break;
            default:
                break;
        }
    }

    public void SetDefaultText(string textInfo)
    {
        if (textInfo == string.Empty)
        {
            bottomDefaultArea.SetActive(false);
            return;
        }
        
        _instance.transform.GetChild(0).gameObject.SetActive(false);
        objectBottom.SetActive(false);
        
        bottomDefaultArea.SetActive(true);
        bottomDefaultText.text = textInfo;
        
        Invoke(nameof(ClearText), 2f);
    }

    private void ClearText()
    {
        bottomDefaultArea.SetActive(false);
        bottomDefaultText.text = "";
    }

    public void SetScanText(string shortText, string longText)
    {
        if (shortText == string.Empty)
        {
            objectBottom.SetActive(false);
            return;
        }
        
        objectBottom.SetActive(true);

        this.shortText = shortText;
        this.longText = longText;
        
        ShowShortText();
    }

    private string shortText, longText;

    private void ShowShortText()
    {
        textfieldBottom.text = shortText;
        additionalImageBottom.gameObject.SetActive(false);
        if(videoPlayer.isPlaying) videoPlayer.Stop();
        interactionButton.onClick.RemoveAllListeners();
        interactionButton.onClick.AddListener(ShowLongText);
        
        _instance.transform.GetChild(0).gameObject.SetActive(false);
        bottomDefaultArea.SetActive(false);
        
        LayoutRebuilder.ForceRebuildLayoutImmediate(layoutTransform);
    }

    private void ShowLongText()
    {
        textfieldBottom.text = longText;
        additionalImageBottom.gameObject.SetActive(true);
        videoPlayer.frame = 0;
        videoPlayer.Play();
        interactionButton.onClick.RemoveAllListeners();
        interactionButton.onClick.AddListener(ShowShortText);
        
        _instance.transform.GetChild(0).gameObject.SetActive(false);
        bottomDefaultArea.SetActive(false);
        
        LayoutRebuilder.ForceRebuildLayoutImmediate(layoutTransform);
    }

    public void RemoveAllText()
    {
        _instance.transform.GetChild(0).gameObject.SetActive(false);
        bottomDefaultArea.SetActive(false);
        objectBottom.SetActive(false);
    }

    public void StartCameraTutorial()
    {
        arTutorialManager.StartCameraTutorial();
    }
}
