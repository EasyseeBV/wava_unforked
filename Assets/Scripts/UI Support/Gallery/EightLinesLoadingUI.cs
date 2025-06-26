using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EightLinesLoadingUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    List<Image> _linesImages;

    [Header("Settings")]
    [SerializeField]
    float _animationSpeed;

    void Update()
    {
        int startIndex = Mathf.FloorToInt((Time.time * _animationSpeed) % 8);

        for (int i = 7; i >= 0; i--)
        {
            var lineImage = _linesImages[startIndex];
            startIndex = startIndex == 7 ? 0 : startIndex + 1;

            var lerpValue = (i + 1) / (float)8;
            lineImage.color = Color.Lerp(Color.black, Color.white, lerpValue);
        }
    }
}
