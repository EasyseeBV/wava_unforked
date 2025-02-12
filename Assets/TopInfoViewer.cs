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

    private void Start()
    {
        _camera = Camera.main;
    }

    public void ShowInfoOnTop(bool Has3Dmodel)
    {
        if (ArTapper.ArtworkToPlace != null)
        {
            IsObject.SetActive(Has3Dmodel);
            IsMusic.SetActive(!Has3Dmodel);
            Title.text = ArTapper.ArtworkToPlace.title;
            Maker.text = "by " + (ArTapper.ArtworkToPlace.artists.Count > 0 ? ArTapper.ArtworkToPlace.artists[0].title : "");
        }
    }
    public Camera targetCamera;
    private Camera _camera;

    private void Update()
    {
        if (_camera != null)
        {
            // Rotate the UI element to face the camera
            Vector3 targetPosition = transform.position + (transform.position - _camera.transform.position);
            transform.LookAt(targetPosition, Vector3.up);
        }
    }
}