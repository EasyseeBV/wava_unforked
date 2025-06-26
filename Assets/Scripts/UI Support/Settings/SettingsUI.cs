using UnityEngine;
using UnityEngine.UI;

public class SettingsUI : MonoBehaviour
{
    [SerializeField]
    HelpCenterUI _helpCenterUI;

    [SerializeField]
    Button _openHelpButton;

    [SerializeField]
    Button _openAboutButton;

    [SerializeField]
    Button _closeHelpButton;

    [SerializeField]
    Button _closeAboutButton;

    [SerializeField]
    GameObject _helpContainer;

    [SerializeField]
    GameObject _aboutContainer;

    private void OnEnable()
    {
        _openHelpButton.onClick.AddListener(OnOpenHelpButtonClicked);
        _openAboutButton.onClick.AddListener(OnOpenAboutButtonClicked);
        _closeHelpButton.onClick.AddListener(OnCloseHelpButtonClicked);
        _closeAboutButton.onClick.AddListener(OnCloseAboutButtonClicked);
    }

    private void OnDisable()
    {
        _openHelpButton.onClick.RemoveListener(OnOpenHelpButtonClicked);
        _openAboutButton.onClick.RemoveListener(OnOpenAboutButtonClicked);
        _closeHelpButton.onClick.RemoveListener(OnCloseHelpButtonClicked);
        _closeAboutButton.onClick.RemoveListener(OnCloseAboutButtonClicked);
    }

    void OnOpenHelpButtonClicked()
    {
        _helpCenterUI.ShowPage();
        _helpCenterUI.ResetUI();
    }

    void OnOpenAboutButtonClicked()
    {
        _aboutContainer.SetActive(true);
    }

    void OnCloseHelpButtonClicked()
    {
        _helpCenterUI.HidePage();
    }

    void OnCloseAboutButtonClicked()
    {
        _aboutContainer.SetActive(false);
    }
}
