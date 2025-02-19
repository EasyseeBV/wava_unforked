using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ARModelObject : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject modelLocation;
    [SerializeField] private GameObject content;

    private GameObject cachedModel = null;
    private MeshRenderer cachedMeshRenderer;

    public void Assign(GameObject model)
    {
        cachedModel = model;
        cachedMeshRenderer = cachedModel.GetComponentInChildren<MeshRenderer>();
        cachedMeshRenderer.transform.localPosition = Vector3.zero;
        cachedMeshRenderer.enabled = false;
    }

    public void Show(TransformsData transformsData)
    {
        content.SetActive(true);
        cachedModel.transform.SetParent(modelLocation.transform);
        
        cachedMeshRenderer.enabled = true;
        
        // Apply position offset
        Vector3 offset = new Vector3(
            transformsData.position_offset.x_offset,
            transformsData.position_offset.y_offset,
            transformsData.position_offset.z_offset
        );
        cachedModel.transform.position += offset; // You may choose to set or add this offset based on your needs
    
        // Apply rotation (assuming the rotation value is in degrees around the Y axis)
        cachedModel.transform.rotation = Quaternion.Euler(0f, transformsData.rotation, 0f);
    
        // Apply scale
        Vector3 newScale = new Vector3(
            transformsData.scale.x_scale,
            transformsData.scale.y_scale,
            transformsData.scale.z_scale
        );
        cachedModel.transform.localScale = newScale;
    }
}