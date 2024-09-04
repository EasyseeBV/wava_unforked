using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TopInfoViewer : MonoBehaviour
{
    public GameObject IsObject;
    public GameObject IsMusic;

    public TextMeshProUGUI Title;
    public TextMeshProUGUI Maker;

    public void ShowInfoOnTop(bool Has3Dmodel)
    {
        if (ArTapper.ARPointToPlace != null)
        {
            IsObject.SetActive(Has3Dmodel);
            IsMusic.SetActive(!Has3Dmodel);
            Title.text = ArTapper.ARPointToPlace.Title;
            Maker.text = "by " + ArTapper.ARPointToPlace.Artist;
        }
    }
    public Camera targetCamera;
    private void Update()
    {
        if (Camera.main != null)
        {
            // Rotate the UI element to face the camera
            Vector3 targetPosition = transform.position + (transform.position - Camera.main.transform.position);
            transform.LookAt(targetPosition, Vector3.up);
        }
    }
}