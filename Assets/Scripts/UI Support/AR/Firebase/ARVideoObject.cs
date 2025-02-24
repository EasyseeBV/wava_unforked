using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class ARVideoObject : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject content;
    [SerializeField] private VideoPlayer videoPlayer;

    private List<VideoPlayer> videoPlayers;
    
    public void PrepareVideo(MediaContentData mediaContentData, Action<VideoPlayer> onComplete)
    {
        content.SetActive(true);
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

    public void Play()
    {
        foreach (var vp in videoPlayers)
        {
            vp.GetComponent<MeshRenderer>().enabled = true;
            vp.Play();
        }
    }
}
