using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeveloperContentButton : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Button button;
    [SerializeField] private TMP_Text label;

    public void SetValue(int value, Action<int> callback)
    {
        label.text = value.ToString();
        button.onClick.AddListener(() => callback(value));
    }
}
