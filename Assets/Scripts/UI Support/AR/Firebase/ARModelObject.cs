using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ARModelObject : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject modelLocation;
    [SerializeField] private GameObject content;

    public void Show(GameObject model, TransformsData transformsData)
    {
        content.SetActive(true);
        model.transform.SetParent(modelLocation.transform);
        Debug.Log(transformsData);
    }
}