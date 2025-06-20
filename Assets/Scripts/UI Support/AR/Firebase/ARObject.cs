using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TriLibCore.Extensions;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.XR.ARFoundation;

public class ARObject : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform placementParent;
    [SerializeField] private Transform videoPlacementArea;
    [SerializeField] private GameObject content;
    [SerializeField] private Renderer shadowPlane;
    [SerializeField] private ARUIImage arUIImageTemplate;
    [SerializeField] private GameObject rootObject;
    [SerializeField] private GameObject occlusionPlane;

    [Header("Templates")]
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private GameObject audioImageCanvas;
    
    private List<GameObject> models = new List<GameObject>();
    private List<ARUIImage> uis = new List<ARUIImage>();
    private List<VideoPlayer> videoPlayers = new List<VideoPlayer>();
    private List<AudioSource> audioSources = new List<AudioSource>();
    private List<GameObject> audioSourceUIs = new List<GameObject>();

    private bool showPreset = false;
    private GameObject presetObject = null;

    private bool showing = false;
    private ARAnchor anchor;
    private static readonly int MainTex = Shader.PropertyToID("_MainTex");

    private void Awake()
    {
        rootObject.SetActive(false);
    }

    private void OnEnable()
    {
        DeveloperModeARView.OnShowRootToggled += ToggleShowRoot;
    }

    private void OnDisable()
    {
        DeveloperModeARView.OnShowRootToggled -= ToggleShowRoot;
    }

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
        
        ApplyOffsets(obj, contentData);
        
        if (anchor) anchor.enabled = false;
        
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
    public VideoPlayer Add(MediaContentData mediaContentData, string url, Action<VideoPlayer> onComplete)
    {
        var player = Instantiate(videoPlayer, videoPlacementArea);
        player.gameObject.SetActive(true);
        
        ApplyOffsets(player.gameObject, mediaContentData);
        
        videoPlayers.Add(player);
        
        player.url = url;
        player.Prepare();
        
        VideoPlayer.EventHandler handler = null;
        handler = (VideoPlayer vp) =>
        {
            vp.prepareCompleted -= handler;
            //vp.renderMode = VideoRenderMode.RenderTexture;
            //vp.gameObject.GetComponent<MeshRenderer>().material.SetTexture(MainTex, vp.targetTexture);
            onComplete.Invoke(vp);
        };

        player.prepareCompleted += handler;

        return player;
    }

    // Adding Audio
    public AudioSource Add(AudioClip clip, MediaContentData contentData)
    {
        if (clip == null)
        {
            Debug.LogError("AudioClip failed to load");
        }
        
        var source = Instantiate(audioSource, placementParent);
        source.clip = clip;
        
        var obj = source.gameObject;
        
        ApplyOffsets(obj, contentData);

        var audioCanvas = Instantiate(audioImageCanvas, obj.transform.position, Quaternion.identity);
        audioCanvas.gameObject.SetActive(false);
        audioCanvas.transform.SetParent(placementParent, true);
        audioSourceUIs.Add(audioCanvas);
        
        if (Camera.main)
        {
            // Get the camera's position and maintain the audioCanvas's Y position.
            Vector3 targetPosition = Camera.main.transform.position;
            targetPosition.y = audioCanvas.transform.position.y;
    
            // Rotate the audioCanvas to face the targetPosition using only Y-axis rotation.
            audioCanvas.transform.LookAt(targetPosition);
        }
        
        audioSources.Add(source);

        return source;
    }

    // Adding UI
    public GameObject Add(Sprite sprite, MediaContentData contentData)
    {
        var ui = Instantiate(arUIImageTemplate, placementParent);
        ui.gameObject.SetActive(true);
        ui.AssignSprite(sprite);
        
        if (contentData == null || contentData.transforms.position_offset == null || contentData.transforms.scale == null) {
            Debug.LogError("MediaContentData or one of its properties is null.");
            return ui.gameObject;
        }
    
        ApplyOffsets(ui.gameObject, contentData);
        
        uis.Add(ui);

        return ui.gameObject;
    }

    private void ApplyOffsets(GameObject obj, MediaContentData contentData)
    {
        // Apply additional rotation by multiplying the current rotation with a new rotation offset
        Quaternion additionalRotation = Quaternion.Euler(
            contentData.transforms.rotation.x_rotation,
            contentData.transforms.rotation.y_rotation,
            contentData.transforms.rotation.z_rotation
        );
        obj.transform.localRotation = additionalRotation;

        // Increment the current scale by the specified additive scale values
        Vector3 additionalScale = new Vector3(
            contentData.transforms.scale.x_scale,
            contentData.transforms.scale.y_scale,
            contentData.transforms.scale.z_scale
        );
        obj.transform.localScale = additionalScale;

        // Increment the current position by the specified position offset
        Vector3 additionalOffset = new Vector3(
            contentData.transforms.position_offset.x_offset,
            contentData.transforms.position_offset.y_offset,
            contentData.transforms.position_offset.z_offset
        );
        obj.transform.localPosition = additionalOffset;
    }

    public void Show(bool hideShadow)
    {
        content.SetActive(true);

        bool videosPrepared = true;

        if (AppSettings.DeveloperMode && DeveloperModeARView.ShowRoot)
        {
            rootObject.SetActive(DeveloperModeARView.ShowRoot);
            rootObject.AddComponent<ARAnchor>();
        }
        
        if (showPreset)
        {
            Debug.Log("showing preset: " + presetObject.name, presetObject);
            presetObject.SetActive(true);
            presetObject.AddComponent<ARAnchor>();
        }
        
        foreach (var vp in videoPlayers)
        {
            if (vp.isPrepared)
            {
                vp.gameObject.GetComponent<MeshRenderer>().enabled = true;
                vp.Play();
                vp.gameObject.AddComponent<ARAnchor>();
            }
            else videosPrepared = false;
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

            var modelAnimation = GetComponentInChildren<Animation>();

            if (modelAnimation)
            {
                var clips = modelAnimation.GetAllAnimationClips();

                foreach (var clip in clips)
                {
                    clip.wrapMode = WrapMode.Loop;
                    modelAnimation.AddClip(clip, clip.name);
                }

                if (clips.Count > 0)
                {
                    modelAnimation.clip = clips[0];
                    modelAnimation.Play(clips[0].name);
                }
            }
            
            obj.AddComponent<ARAnchor>();
        }

        foreach (var source in audioSources)
        {
            source.Play();
            source.gameObject.AddComponent<ARAnchor>();
        }

        foreach (var sourceUI in audioSourceUIs)
        {
            sourceUI.SetActive(true);
        }

        foreach (var ui in uis)
        {
            ui.Show();
            ui.gameObject.AddComponent<ARAnchor>();
        }
        
        // Adjust the shadow plane so its top is at the bottom of the spawned objects.
        //AdjustShadowPlane();

        if (videosPrepared) CompleteShow();
        else StartCoroutine(WaitForVideos());
    }
    
    private IEnumerator WaitForVideos()
    {
        yield return new WaitUntil(() => videoPlayers.All(vp => vp.isPrepared));

        foreach (var vp in videoPlayers)
        {
            if (!vp.GetComponent<ARAnchor>())
            {
                vp.gameObject.AddComponent<ARAnchor>();
            }
        }
        CompleteShow();
    }

    private void CompleteShow()
    {
        shadowPlane.gameObject.SetActive(true);
        //gameObject.AddComponent<ARAnchor>();
        showing = true;
        occlusionPlane.SetActive(true);
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
        
        shadowPlane.gameObject.SetActive(true);
        
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

    private void ToggleShowRoot(bool state)
    {
        if (AppSettings.DeveloperMode && showing)
        {
            rootObject.gameObject.SetActive(state);
        }
    }
}
