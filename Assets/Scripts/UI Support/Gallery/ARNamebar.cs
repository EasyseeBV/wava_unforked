using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ARNamebar : MonoBehaviour
{
    [SerializeField] private GameObject content;
    [SerializeField] private TMP_Text namebarLabel;
    [SerializeField] private float waitToHideTime = 3f;

    private void Awake()
    {
        content.gameObject.SetActive(false);
    }

    public void SetNamebarLabel(string text)
    {
        namebarLabel.text = text;
        content.gameObject.SetActive(true);
        StartCoroutine(HideNamebar());
    }

    private IEnumerator HideNamebar()
    {
        yield return new WaitForSecondsRealtime(waitToHideTime);
        
        content.gameObject.SetActive(false);
    }
}
