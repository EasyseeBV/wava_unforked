using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateFAQHeight : MonoBehaviour {
    public RectTransform GetHeight;
    public RectTransform scroll;
    public float ExtraHeight;
    RectTransform rect;

    private void Start() {
        rect = GetComponent<RectTransform>();
    }
    // Update is called once per frame
    void Update()
    {
        if (rect.sizeDelta.y != GetHeight.sizeDelta.y + ExtraHeight) {
            Vector3 scrollPosition = scroll.anchoredPosition;
            scrollPosition.y +=(GetHeight.sizeDelta.y + ExtraHeight - rect.sizeDelta.y);
            scroll.anchoredPosition = scrollPosition;
            rect.sizeDelta = new Vector2(rect.sizeDelta.x, GetHeight.sizeDelta.y + ExtraHeight);
            
        }
    }
}
