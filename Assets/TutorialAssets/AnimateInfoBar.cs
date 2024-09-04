using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AnimateInfoBar: MonoBehaviour
{
    public RectTransform Rect;
    
    public float AnimationSpeed = 2f;
    
    protected bool isHidden = true;
    
    private bool shouldBeHidden;
    private bool animating;
    private float percentage;
    private float OriginalPos;

    public List<GameObject> ObjectsToDissapear;
    public Button closeButton;

    protected virtual void Awake()
    {
        closeButton.onClick.AddListener(Animate);
    }

    public void Animate()
    {
        StartRectAnimation(!isHidden);
    }

    // Update is called once per frame
    void FixedUpdate() {
        if (animating) 
        {
            if (shouldBeHidden) 
            {
                percentage -= Time.fixedDeltaTime * AnimationSpeed;
                UpdateRect();
                if (percentage <= 0)
                    StopRectAnmation(true);

            } 
            else 
            {
                percentage += Time.fixedDeltaTime * AnimationSpeed;
                UpdateRect();
                if (percentage >= 1)
                    StopRectAnmation(false);
            }
        }
        
        if (!animating && Rect.anchoredPosition.y < 120 && Input.touchCount == 0 && !Input.GetMouseButton(0)) 
        {
            StartRectAnimation(true);
        }
    }

    protected virtual void StartRectAnimation(bool hide) {
        if (!animating) 
        {
            if (isHidden == hide)
                return;
            
            percentage = hide ? 1 : 0;
        }
        
        if (hide) 
        {
            OriginalPos = Rect.anchoredPosition.y;
        }
        else 
        {
            ObjectsToDissapear.ForEach(t => t.SetActive(false));
            OriginalPos = 0;
        }

        animating = true;
        shouldBeHidden = hide;
        UpdateRect();
        Rect.gameObject.SetActive(true);
    }

    protected virtual void StopRectAnmation(bool hidden) {
        animating = false;
        isHidden = hidden;
        
        if (hidden) {
            ObjectsToDissapear.ForEach(t => t.SetActive(true));
            Rect.gameObject.SetActive(false);
        }
        
        StartCoroutine(LateRebuild());
    }

    void UpdateRect() 
    {
        if (shouldBeHidden)
            Rect.anchoredPosition = new Vector2(0, Mathf.Lerp(0, OriginalPos, percentage));
        else
            Rect.anchoredPosition = new Vector2(0, Mathf.Lerp(0, 120, percentage));
    }
        
    protected IEnumerator LateRebuild()
    {
        yield return new WaitForEndOfFrame();
        
        LayoutRebuilder.ForceRebuildLayoutImmediate(Rect);
    }

}