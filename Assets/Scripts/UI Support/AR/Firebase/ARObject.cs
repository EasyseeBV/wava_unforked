using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class ARObject : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform placementParent;
    [SerializeField] private Transform videoPlacementArea;
    [SerializeField] private GameObject content;
    [SerializeField] private Renderer shadowPlane;
    [SerializeField] private ARUIImage arUIImageTemplate;

    [Header("Templates")]
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private AudioSource audioSource;
    
    private List<GameObject> models = new List<GameObject>();
    private List<ARUIImage> uis = new List<ARUIImage>();
    private List<VideoPlayer> videoPlayers = new List<VideoPlayer>();
    private List<AudioSource> audioSources = new List<AudioSource>();

    private bool showPreset = false;
    private GameObject presetObject = null;
    
    // Adding Models
    public void Add(GameObject obj, MediaContentData contentData)
    {
        obj.SetActive(true);
        
        models.Add(obj);
        
        obj.transform.SetParent(placementParent);
        
        if (contentData == null || contentData.transforms.position_offset == null || contentData.transforms.scale == null) {
            Debug.LogError("MediaContentData or one of its properties is null.");
            return;
        }
    
        // Apply rotation (assuming the rotation value is in degrees around the Y axis)
        obj.transform.localRotation = Quaternion.Euler(contentData.transforms.rotation.x_rotation, contentData.transforms.rotation.y_rotation, contentData.transforms.rotation.z_rotation);
    
        // Apply scale
        Vector3 newScale = new Vector3(
            contentData.transforms.scale.x_scale,
            contentData.transforms.scale.y_scale,
            contentData.transforms.scale.z_scale
        );
        obj.transform.localScale = newScale;
        
        // Apply position offset
        Vector3 offset = new Vector3(
            contentData.transforms.position_offset.x_offset,
            contentData.transforms.position_offset.y_offset,
            contentData.transforms.position_offset.z_offset
        );
        obj.transform.localPosition = offset;

        if (obj.TryGetComponent<MeshRenderer>(out var mesh))
        {
            mesh.transform.localPosition = Vector3.zero;
            mesh.enabled = false;
        }
        else
        {
            var meshInChildren = obj.GetComponentInChildren<MeshRenderer>();
            if (meshInChildren)
            {
                meshInChildren.transform.localPosition = Vector3.zero;
                meshInChildren.enabled = false;
            }
        }
        
        obj.transform.localPosition = offset;
        
        obj.SetActive(false);
    }

    public void Add(GameObject obj)
    {
        showPreset = true;
        obj.transform.SetParent(placementParent, false);
        obj.SetActive(false);
        Debug.Log("Added obj", obj);
        presetObject = obj;
    }

    // Adding Video
    public void Add(MediaContentData mediaContentData, string url, Action<VideoPlayer> onComplete)
    {
        Debug.Log("Adding and preparing video...");
        var player = Instantiate(videoPlayer, videoPlacementArea);
        videoPlayers.Add(player);
        player.url = url;
        player.Prepare();
        
        // Apply position offset
        Vector3 offset = new Vector3(
            mediaContentData.transforms.position_offset.x_offset,
            mediaContentData.transforms.position_offset.y_offset,
            mediaContentData.transforms.position_offset.z_offset
        );
        player.gameObject.transform.position += offset;
    
        // Apply rotation (assuming the rotation value is in degrees around the Y axis)
        player.gameObject.transform.rotation = Quaternion.Euler(mediaContentData.transforms.rotation.x_rotation, mediaContentData.transforms.rotation.y_rotation, mediaContentData.transforms.rotation.z_rotation);
    
        // Apply scale
        Vector3 newScale = new Vector3(
            mediaContentData.transforms.scale.x_scale,
            mediaContentData.transforms.scale.y_scale,
            mediaContentData.transforms.scale.z_scale
        );
        player.gameObject.transform.localScale = newScale;
        
        VideoPlayer.EventHandler handler = null;
        handler = (VideoPlayer vp) =>
        {
            player.prepareCompleted -= handler;
            player.gameObject.transform.SetParent(placementParent);
            onComplete.Invoke(vp);
        };

        player.prepareCompleted += handler;
    }

    // Adding Audio
    public void Add(AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogError("AudioClip failed to load");
        }
        
        var source = Instantiate(audioSource, placementParent);
        source.clip = clip;
        audioSources.Add(source);
    }

    public GameObject Add(Sprite sprite, MediaContentData contentData)
    {
        var ui = Instantiate(arUIImageTemplate, placementParent);
        ui.gameObject.SetActive(true);
        ui.AssignSprite(sprite);
        
        if (contentData == null || contentData.transforms.position_offset == null || contentData.transforms.scale == null) {
            Debug.LogError("MediaContentData or one of its properties is null.");
            return ui.gameObject;
        }
    
        // Apply rotation (assuming the rotation value is in degrees around the Y axis)
        ui.transform.localRotation = Quaternion.Euler(contentData.transforms.rotation.x_rotation, contentData.transforms.rotation.y_rotation, contentData.transforms.rotation.z_rotation);
    
        // Apply scale
        Debug.Log($"x{contentData.transforms.scale.x_scale}, y{contentData.transforms.scale.y_scale}, z{contentData.transforms.scale.z_scale}");
        Vector3 newScale = new Vector3(
            contentData.transforms.scale.x_scale,
            contentData.transforms.scale.y_scale,
            contentData.transforms.scale.z_scale
        );
        ui.transform.localScale = newScale;
        
        // Apply position offset
        Vector3 offset = new Vector3(
            contentData.transforms.position_offset.x_offset,
            contentData.transforms.position_offset.y_offset,
            contentData.transforms.position_offset.z_offset
        );
        ui.transform.localPosition = offset;
        
        uis.Add(ui);

        return ui.gameObject;
    }

    public void Show()
    {
        content.SetActive(true);
        
        if (showPreset)
        {
            Debug.Log("showing preset: " + presetObject.name, presetObject);
            presetObject.SetActive(true);
        }
        
        foreach (var vp in videoPlayers)
        {
            vp.gameObject.GetComponent<MeshRenderer>().enabled = true;
            vp.Play();
        }

        foreach (var obj in models)
        {
            obj.SetActive(true);
            
            if (obj.TryGetComponent<MeshRenderer>(out var mesh))
            {
                mesh.enabled = true;
            }
            else
            {
                var meshInChildren = obj.GetComponentInChildren<MeshRenderer>();
                if (meshInChildren)
                {
                    meshInChildren.enabled = true;
                }
            }
        }

        foreach (var source in audioSources)
        {
            source.Play();
        }

        foreach (var ui in uis)
        {
            ui.Show();
        }
        
        // Adjust the shadow plane so its top is at the bottom of the spawned objects.
        AdjustShadowPlane();
    }
    
    // This method computes the combined bounds of all child renderers under placementParent,
    // excluding the shadow plane itself, and repositions the shadow plane accordingly.
    private void AdjustShadowPlane()
    {
        Renderer[] renderers = placementParent.GetComponentsInChildren<Renderer>();
        List<Renderer> objectRenderers = new List<Renderer>();
        foreach (Renderer rend in renderers)
        {
            if (rend != shadowPlane)
                objectRenderers.Add(rend);
        }
        
        if (objectRenderers.Count == 0)
            return;
        
        Bounds combinedBounds = objectRenderers[0].bounds;
        foreach (Renderer rend in objectRenderers)
        {
            combinedBounds.Encapsulate(rend.bounds);
        }
        
        // Get the lowest Y point of the combined bounds.
        float lowestY = combinedBounds.min.y;
        
        // Get the height of the shadow plane (assuming its pivot is at its center).
        float planeHeight = shadowPlane.bounds.size.y;
        
        // Adjust the shadow plane so that its top (center + half its height) sits at the lowest Y.
        Vector3 currentPos = shadowPlane.transform.position;
        shadowPlane.transform.position = new Vector3(
            currentPos.x, 
            lowestY - planeHeight / 2, 
            currentPos.z
        );
    }
}
