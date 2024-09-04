using UnityEngine;
using UnityEngine.UI;


public class AnimateHamburgerMenu : MonoBehaviour
{
    public RectTransform Rect;
    public Michsky.MUIP.UIManager UImanager;
    public Image WavaLogo;

    bool isHidden = true;
    bool ShouldBeHidden;
    bool ForeGroundBlack = false;
    public float AnimationSpeed = 2f;
    bool animating;
    float percentage;
    public bool StartBlack;

    private void OnEnable() {
        StopRectAnmation(true);
        if (StartBlack)
            SetForeGroundColorBlack();
        else
            SetForeGroundColorWhite();
    }

    public void SetForeGroundColorBlack() {
        ForeGroundBlack = true;
        if (isHidden)
            UImanager.animatedIconColor = Color.black;
    }

    public void SetForeGroundColorWhite() {
        ForeGroundBlack = false;
        if (isHidden)
            UImanager.animatedIconColor = Color.white;
    }

    public void ShowMenu() {
        StartRectAnimation(false);
    }
    public void HideMenu() {
        StartRectAnimation(true);
    }

    void StartRectAnimation(bool hide) {
        if (!animating) {
            if (isHidden == hide)
                return;
            else
                percentage = hide ? 1 : 0;
        }

        animating = true;
        ShouldBeHidden = hide;
        UpdateRect();
        Rect.gameObject.SetActive(true);
    }

    void UpdateRect() {
        Rect.anchorMax = new Vector2(1, 0.5f);
        Rect.anchorMin = new Vector2(1, 0.5f);
        Rect.sizeDelta = new Vector2(GetComponent<RectTransform>().rect.width, GetComponent<RectTransform>().rect.height);
        Rect.anchoredPosition = new Vector2(Mathf.Lerp(0, GetComponent<RectTransform>().rect.width * -1, percentage), 0);
    }

    void StopRectAnmation(bool hidden) {
        animating = false;
        isHidden = hidden;
        Rect.anchorMax = new Vector2(1, 1);
        Rect.anchorMin = new Vector2(0, 0);
        Rect.sizeDelta = new Vector2(0, 0);
        Rect.anchoredPosition = new Vector2(0, 0);       
        if (hidden)
            Rect.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void FixedUpdate() {
        if (animating) {
            if (ShouldBeHidden) {
                percentage -= Time.fixedDeltaTime * AnimationSpeed;
                UpdateRect();
                if (percentage <= 0.50f) {
                    WavaLogo.enabled = false;
                    if (percentage <= 0.15f) {
                        UImanager.animatedIconColor = ForeGroundBlack ?  Color.black : Color.white;
                        if (percentage <= 0)
                            StopRectAnmation(true);
                    }
                }
            } else {
                percentage += Time.fixedDeltaTime * AnimationSpeed;
                UpdateRect();
                if (percentage >= 0.15f) {
                    UImanager.animatedIconColor = Color.white;
                    if (percentage >= 0.50f) {
                        WavaLogo.enabled = true;
                        if (percentage >= 1)
                            StopRectAnmation(false);
                    }
                }
                
            }
        }
    }
    private void OnDestroy() {
        UImanager.animatedIconColor = StartBlack ? Color.black : Color.white;
    }
}
