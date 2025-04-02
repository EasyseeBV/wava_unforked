using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StatusText : MonoBehaviour
{
    [SerializeField] private TMP_Text statusLabel;

    public void SetText(string text)
    {
        statusLabel.text = text;
    }
}
