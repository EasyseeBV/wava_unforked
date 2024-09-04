using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ARNamebar : MonoBehaviour
{
    [SerializeField] private TMP_Text namebarLabel;
    
    private void OnEnable()
    {
        namebarLabel.text = "";
    }

    public void SetNamebarLabel(string text) => namebarLabel.text = text;
}
