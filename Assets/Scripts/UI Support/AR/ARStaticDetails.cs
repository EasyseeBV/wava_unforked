using System;
using System.Collections;
using System.Collections.Generic;
using Messy.Definitions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ARStaticDetails : MonoBehaviour
{
    [Header("Text References")] 
    [SerializeField] private TextMeshProUGUI contentTitleLabel;
    [SerializeField] private TextMeshProUGUI contentDescriptionLabel;
    [SerializeField] private Button contentDescriptionButton;
    [SerializeField] private bool lateUpdateText;
    [SerializeField] private TMP_Text exhibitionLabel;
    [SerializeField] private TMP_Text artistLabel;
    [SerializeField] private TMP_Text dateLabel;
    
    [Header("Artwork By")]
    [SerializeField] private ArtistContainer artistContainer;

    [Header("Exhibition")]
    [SerializeField] private ExhibitionCard exhibitionCard;

    /*[Header("AR Layout Elements")]
    [SerializeField] private RectTransform textLayoutElement;*/

    private string fullLengthDescription;
    private bool readingMore = false;
    
    private string currText;
    private bool textNeedsUpdate = true;

    private ArtworkData artwork;

    private void Awake()
    {
        if (lateUpdateText) textNeedsUpdate = true;
        contentDescriptionButton?.onClick.AddListener(ToggleReadMore);
    }

    public void Open(ArtworkData artwork)
    {
        if (artwork == null)
        {
            Debug.LogWarning("Null ARPointSO... aborting");
            return;
        }
        
        this.artwork = artwork;

        //readingMore = false;
        contentTitleLabel.text = artwork.title;
        //fullLengthDescription = artwork.description;
        contentDescriptionLabel.text = artwork.description;
        //TruncateText();

        if (artwork.artists.Count > 0)
        {
            artistContainer?.Assign(artwork.artists[0]);
            artistLabel.text = artwork.artists[0].title;
        }

        var connectedExhibition = artwork.hotspot?.ConnectedExhibition ?? FirebaseLoader.GetConnectedExhibition(artwork);
        if (connectedExhibition != null)
        {
            exhibitionCard.Init(connectedExhibition);
            exhibitionLabel.text = connectedExhibition.title;
        }
        else
        {
            exhibitionCard.gameObject.SetActive(false);
        }

        dateLabel.text = artwork.year.ToString();
    }

    private void WaitForExhibition()
    {
        if (artwork.hotspot == null)
        {
            var exhibition = FirebaseLoader.GetConnectedExhibition(artwork);
            exhibitionCard.Init(exhibition);
        }
        else
        {
            artwork.hotspot.ConnectedExhibition = FirebaseLoader.GetConnectedExhibition(artwork);;
            exhibitionCard.Init(artwork.hotspot.ConnectedExhibition);
        }
    }
    
    private void TruncateText()
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
            fullLengthDescription ??= artwork.description;
            int min = 0;
            int max = fullLengthDescription.Length;
            string truncatedText = fullLengthDescription;

            while (min < max)
            {
                int mid = (int)Mathf.Clamp((min + max) / 2, 0, Mathf.Infinity);
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
            int len = (int)Mathf.Clamp(min - 1, 0, Mathf.Infinity);
            truncatedText = fullLengthDescription.Substring(0, len) + " <color=black>...read more</color>";;
            contentDescriptionLabel.text = truncatedText;
            currText = truncatedText;
        }
        else
        {
            // Text fits within 3 lines, so no need to truncate
            contentDescriptionLabel.text = displayedText;
            currText = displayedText;
        }
        
        StartCoroutine(LateUpdateText());
    }

    private IEnumerator LateUpdateText()
    {
        yield return new WaitForEndOfFrame();
        contentDescriptionLabel.text = currText;
        yield return new WaitForEndOfFrame();
        //LayoutRebuilder.ForceRebuildLayoutImmediate(textLayoutElement);        
        yield return new WaitForEndOfFrame();
        //yield return new WaitForEndOfFrame();
        //Canvas.ForceUpdateCanvases();
    }
    
    private void ToggleReadMore()
    {
        Debug.LogError("ToggleReadMore() called, but not implemented");
        return;
        readingMore = !readingMore;

        if (readingMore)
        {
            contentDescriptionLabel.text = fullLengthDescription;
            currText = fullLengthDescription;
            StartCoroutine(LateUpdateText());
        }
        else TruncateText();
    }
}
