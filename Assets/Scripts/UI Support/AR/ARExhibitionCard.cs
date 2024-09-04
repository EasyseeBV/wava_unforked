using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ARExhibitionCard : ExhibitionCard
{
    [Header("AR Refs")]
    [SerializeField] private Button selectionButton;
    [SerializeField] private GameObject exitConfirmation;
    [SerializeField] private Button exitButton;

    protected override void Awake()
    {
        base.Awake();
        selectionButton.onClick.AddListener(() => exitConfirmation.SetActive(true));
        exitButton.onClick.AddListener(OpenExhibitionPage);
    }
}
