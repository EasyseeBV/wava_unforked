using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class ARObject : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform placementParent;
    [SerializeField] private GameObject content;

    [Header("Templates")]
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private AudioSource audioSource;
    
    private List<GameObject> models = new List<GameObject>();
    private List<VideoPlayer> videoPlayers = new List<VideoPlayer>();
    private List<AudioSource> audioSources = new List<AudioSource>();
    
    // Adding Models
    public void Add(GameObject obj, MediaContentData contentData)
    {
        obj.SetActive(true);
        
        models.Add(obj);
        
        if (contentData == null || contentData.transforms.position_offset == null || contentData.transforms.scale == null) {
            Debug.LogError("MediaContentData or one of its properties is null.");
            return;
        }
        
        // Apply position offset
        Vector3 offset = new Vector3(
            contentData.transforms.position_offset.x_offset,
            contentData.transforms.position_offset.y_offset,
            contentData.transforms.position_offset.z_offset
        );
        obj.transform.position += offset; // You may choose to set or add this offset based on your needs
    
        // Apply rotation (assuming the rotation value is in degrees around the Y axis)
        obj.transform.rotation = Quaternion.Euler(0f, contentData.transforms.rotation, 0f);
    
        // Apply scale
        Debug.Log($"x{contentData.transforms.scale.x_scale}, y{contentData.transforms.scale.y_scale}, z{contentData.transforms.scale.z_scale}");
        Vector3 newScale = new Vector3(
            contentData.transforms.scale.x_scale,
            contentData.transforms.scale.y_scale,
            contentData.transforms.scale.z_scale
        );
        obj.transform.localScale = newScale;

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
        
        obj.transform.SetParent(placementParent);
        obj.SetActive(false);
    }

    // Adding Video
    public void Add(MediaContentData mediaContentData, Action<VideoPlayer> onComplete)
    {
        var player = Instantiate(videoPlayer, placementParent);
        videoPlayers.Add(player);
        player.url = mediaContentData.media_content;
        player.Prepare();
        
        // Apply position offset
        Vector3 offset = new Vector3(
            mediaContentData.transforms.position_offset.x_offset,
            mediaContentData.transforms.position_offset.y_offset,
            mediaContentData.transforms.position_offset.z_offset
        );
        player.gameObject.transform.position += offset; // You may choose to set or add this offset based on your needs
    
        // Apply rotation (assuming the rotation value is in degrees around the Y axis)
        player.gameObject.transform.rotation = Quaternion.Euler(0f, mediaContentData.transforms.rotation, 0f);
    
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
            onComplete.Invoke(vp);
        };

        player.prepareCompleted += handler;
    }

    // Adding Audio
    public void Add(AudioClip clip)
    {
        var source = Instantiate(audioSource);
        source.clip = clip;
        audioSources.Add(source);
    }

    public void Show()
    {
        content.SetActive(true);
        
        foreach (var vp in videoPlayers)
        {
            vp.GetComponent<MeshRenderer>().enabled = true;
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
    }
}