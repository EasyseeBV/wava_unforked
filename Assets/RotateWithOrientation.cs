using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateWithOrientation : MonoBehaviour
{
    private ScreenOrientation currentOrientation;

    public Vector2 PortraitPos;
    RectTransform rect;

    void Start()
    {
        currentOrientation = Screen.orientation;
        rect = GetComponent<RectTransform>();
    }

    void Update()
    {
        if (Screen.orientation != currentOrientation)
        {
            Debug.Log("Orientation has changed!");
            currentOrientation = Screen.orientation;
            ChangeTransforms(currentOrientation);
        }
    }

    public void ChangeTransforms(ScreenOrientation orientation) {
        print(orientation);
        switch (orientation) {
            case ScreenOrientation.Portrait:
                rect.anchorMin = new Vector2(0.5f, 0);
                rect.anchorMax = new Vector2(0.5f, 0);
                rect.anchoredPosition = PortraitPos;
                break;
            case ScreenOrientation.PortraitUpsideDown:
                rect.anchorMin = new Vector2(0.5f, 1);
                rect.anchorMax = new Vector2(0.5f, 1);
                rect.anchoredPosition = new Vector2(PortraitPos.x * -1, PortraitPos.y * -1);
                break;
            case ScreenOrientation.LandscapeLeft:
                rect.anchorMin = new Vector2(0, 0.5f);
                rect.anchorMax = new Vector2(0, 0.5f);
                rect.anchoredPosition = new Vector2(PortraitPos.y , PortraitPos.x * -1);
                break;
            case ScreenOrientation.LandscapeRight:
                rect.anchorMin = new Vector2(1, 0.5f);
                rect.anchorMax = new Vector2(1, 0.5f);
                rect.anchoredPosition = new Vector2(PortraitPos.y*-1, PortraitPos.x);
                break;
        }
    }
}