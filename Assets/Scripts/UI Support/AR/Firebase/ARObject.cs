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
    
    private List<MediaContentData> modelMediaContentData = new List<MediaContentData>();
    private List<MediaContentData> videoMediaContentData = new List<MediaContentData>();
    
    private List<GameObject> models = new List<GameObject>();
    private List<VideoPlayer> videoPlayers = new List<VideoPlayer>();
    private List<AudioSource> audioSources = new List<AudioSource>();
    
    // Adding Models
    public void Add(GameObject obj, MediaContentData contentData)
    {
        models.Add(obj);
        modelMediaContentData.Add(contentData);
        
        // Apply position offset
        Vector3 offset = new Vector3(
            contentData.position_offset.x_offset,
            contentData.position_offset.y_offset,
            contentData.position_offset.z_offset
        );
        obj.transform.position += offset; // You may choose to set or add this offset based on your needs
    
        // Apply rotation (assuming the rotation value is in degrees around the Y axis)
        obj.transform.rotation = Quaternion.Euler(0f, contentData.rotation, 0f);
    
        // Apply scale
        Vector3 newScale = new Vector3(
            contentData.scale.x_scale,
            contentData.scale.y_scale,
            contentData.scale.z_scale
        );
        obj.transform.localScale = newScale;
        
        obj.GetComponentInChildren<MeshRenderer>().enabled = false;
    }

    // Adding Video
    public void Add(MediaContentData mediaContentData, Action<VideoPlayer> onComplete)
    {
        var player = Instantiate(videoPlayer);
        videoPlayers.Add(player);
        player.url = mediaContentData.media_content;
        player.Prepare();
        
        // Apply position offset
        Vector3 offset = new Vector3(
            mediaContentData.position_offset.x_offset,
            mediaContentData.position_offset.y_offset,
            mediaContentData.position_offset.z_offset
        );
        player.gameObject.transform.position += offset; // You may choose to set or add this offset based on your needs
    
        // Apply rotation (assuming the rotation value is in degrees around the Y axis)
        player.gameObject.transform.rotation = Quaternion.Euler(0f, mediaContentData.rotation, 0f);
    
        // Apply scale
        Vector3 newScale = new Vector3(
            mediaContentData.scale.x_scale,
            mediaContentData.scale.y_scale,
            mediaContentData.scale.z_scale
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
            obj.GetComponent<MeshRenderer>().enabled = true;
        }

        foreach (var source in audioSources)
        {
            source.Play();
        }
    }
}