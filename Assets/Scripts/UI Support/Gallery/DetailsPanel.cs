using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DetailsPanel : MonoBehaviour
{
    [Header("Rebuild layout")]
    [SerializeField] protected List<RectTransform> rebuildLayout;
    [SerializeField] protected RectTransform contentLayoutGroup;
    
    [Header("Text References")] 
    [SerializeField] protected TextMeshProUGUI contentTitleLabel;
    [SerializeField] protected TextMeshProUGUI contentDescriptionLabel;
    [SerializeField] protected Button contentDescriptionButton;
    //[SerializeField] protected RectTransform textAreaTransform;
    [SerializeField] protected bool lateUpdateText;

    [Header("General")]
    [SerializeField] private Button closeButton;
    
    protected string fullLengthDescription;
    protected bool readingMore = false;

    private string currText;
    private bool textNeedsUpdate = false;

    private void Awake()
    {
        Setup();
    }

    protected virtual void Setup()
    {
        if (lateUpdateText) textNeedsUpdate = true;
        //contentDescriptionButton.onClick.AddListener(ToggleReadMore);
        closeButton.onClick.AddListener(Close);
    }

    protected virtual void Close()
    {
        gameObject.SetActive(false);
    }

    private void ToggleReadMore()
    {
        readingMore = !readingMore;

        if (readingMore) contentDescriptionLabel.text = fullLengthDescription;
        else TruncateText();

        StartCoroutine(LateRebuild());
    }
    
    protected void TruncateText()
    {
        // Force the text to update and generate geometry
        contentDescriptionLabel.ForceMeshUpdate();

        // Store the original text without truncation
        string displayedText = fullLengthDescription;

        // Temporarily set the text to get initial info
        contentDescriptionLabel.text = displayedText;
        contentDescriptionLabel.ForceMeshUpdate(true, true);
        
        // Check if the text exceeds 3 lines
        if (contentDescriptionLabel.textInfo.lineCount > 3)
        {
            // Start binary search to find the maximum length that fits within 3 lines
            int min = 0;
            int max = fullLengthDescription.Length;
            string truncatedText = fullLengthDescription;

            while (min < max)
            {
                int mid = (min + max) / 2;
                truncatedText = fullLengthDescription.Substring(0, mid) + " <color=black>...read more</color>";

                contentDescriptionLabel.text = truncatedText;
                contentDescriptionLabel.ForceMeshUpdate();

                if (contentDescriptionLabel.textInfo.lineCount > 3)
                {
                    max = mid;
                }
                else
                {
                    min = mid + 1;
                }
            }

            // Ensure the final text does not exceed 3 lines
            truncatedText = fullLengthDescription.Substring(0, min - 1) + " <color=black>...read more</color>";;
            contentDescriptionLabel.text = truncatedText;
            currText = truncatedText;
        }
        else
        {
            // Text fits within 3 lines, so no need to truncate
            contentDescriptionLabel.text = displayedText;
            currText = displayedText;
        }

        if (textNeedsUpdate)
        {
            textNeedsUpdate = false;
            StartCoroutine(LateUpdateText());
        }
    }

    private IEnumerator LateUpdateText()
    {
        yield return new WaitForEndOfFrame();
        contentDescriptionLabel.text = currText;
    }

    protected IEnumerator LateRebuild()
    {
        yield return new WaitForEndOfFrame();
        
        //LayoutRebuilder.ForceRebuildLayoutImmediate(textAreaTransform);
        //LayoutRebuilder.ForceRebuildLayoutImmediate(contentLayoutGroup);
    }
}