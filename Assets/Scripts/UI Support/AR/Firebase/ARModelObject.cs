using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ARModelObject : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject modelLocation;
    [SerializeField] private GameObject content;

    private List<GameObject> cachedModels = new();
    private List<MeshRenderer> cachedMeshRenderers;
    private List<MediaContentData> mediaContentData = new();

    public void Assign(GameObject model, MediaContentData contentData)
    {
        cachedModels.Add(model);
        mediaContentData.Add(contentData);
        var cachedMeshRenderer = model.GetComponentInChildren<MeshRenderer>();
        cachedMeshRenderer.transform.localPosition = Vector3.zero;
        cachedMeshRenderer.enabled = false;
        cachedMeshRenderers.Add(cachedMeshRenderer);
    }

    public void Show()
    {
        content.SetActive(true);

        for (int i = 0; i < cachedModels.Count; i++)
        {
            cachedModels[i].transform.SetParent(modelLocation.transform);
        
            cachedMeshRenderers[i].enabled = true;
        
            // Apply position offset
            Vector3 offset = new Vector3(
                mediaContentData[i].position_offset.x_offset,
                mediaContentData[i].position_offset.y_offset,
                mediaContentData[i].position_offset.z_offset
            );
            cachedModels[i].transform.position += offset; // You may choose to set or add this offset based on your needs
    
            // Apply rotation (assuming the rotation value is in degrees around the Y axis)
            cachedModels[i].transform.rotation = Quaternion.Euler(0f, mediaContentData[i].rotation, 0f);
    
            // Apply scale
            Vector3 newScale = new Vector3(
                mediaContentData[i].scale.x_scale,
                mediaContentData[i].scale.y_scale,
                mediaContentData[i].scale.z_scale
            );
            cachedModels[i].transform.localScale = newScale;
        }
    }
}