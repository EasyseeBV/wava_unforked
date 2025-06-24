using System;
using Messy.Definitions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.IO;
using System.Threading.Tasks;

public class HotspotManager : MonoBehaviour
{   
    //Replaced ARPoint with ARPointSO
    [HideInInspector] public ArtworkData artwork;
    public TextMeshPro LeftTitle;
    public TextMeshPro LeftDistance;
    public TextMeshPro RightTitle;
    public TextMeshPro RightDistance;
    public float _distance = 100f;
    public bool CanShow = false;
    public GameObject ARObject;
    public Image BackgroundAR;
    public Image ARObjectImage;
    public MeshRenderer Logo;
    public GameObject Parent;
    
    [Header("UI Designs V2")]
    public MeshRenderer BorderRingMesh;
    public Material SelectedHotspotMat;
    public Material SelectedHotspotInRangeMat;
    public GameObject Shadow;

    [Header("UI Designs V3")]
    [SerializeField] private Image backgroundARImage;
    
    [Header("Runtime")]
    public bool selected = false;
    public MarkerGroup markerGroup;
    public bool inPlayerRange = false;

    [HideInInspector]
    public ExhibitionData ConnectedExhibition;

    //[HideInInspector]
    public NavigationMaker Navigation;
    
    [Header("Zoom Settings")]
    public float MinZoom;
    public float DetailedZoom = 18;
    bool ZoomedOut;
    [HideInInspector] public float ZoomLevel = 16;
    public static float Zoom = 16;
    public static event Action<float> OnDistanceValidated;
    public static event Action OnOfflineModeNoLocalInstance;
    
    private bool inReach = false;
    
    
    //Replaced ARPoint with ARPointSO
    public void Init(ArtworkData point) 
    {
        artwork = point;
        LeftTitle.text = artwork.title;
    }

    private void OnEnable() 
    {
        if (artwork != null)
            OnChangeGpsPosition(_distance);
    }
    
    public void OnChangeGpsPosition(float distance) 
    {
        _distance = distance;
        LeftDistance.text = string.Format("{0}km", distance.ToString("F1"));

        if (backgroundARImage.sprite == null)
        {
            if (artwork.marker.inMapView) LoadBackgroundImage();
        }

        if (CanShow && !IsClose()) 
        {
            CanShow = false;
            SetReach(false);
        }
    }

    private async Task LoadBackgroundImage()
    {
        try
        {
            var sprites = await artwork.GetImages(1);
            if (sprites is { Count: > 0 } && sprites[0] != null)
            {
                if (backgroundARImage ==null) return;
                backgroundARImage.sprite = sprites[0];
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message);
        }
    }

    public void SetReach(bool InReach)
    {
        inReach = InReach;

        if (inReach)
        {
            BorderRingMesh.enabled = true;
            BorderRingMesh.material = SelectedHotspotInRangeMat;
            inPlayerRange = true;
            
            if (ColorUtility.TryParseHtmlString("#00FFC5", out Color hexColor)) Logo.material.color = hexColor;
            else Logo.material.color = Color.green;
            
            SelectionMenu.Instance.SelectHotspot(this, true);
        }
        else
        {
            inPlayerRange = false;
            if (selected)
            {
                BorderRingMesh.enabled = true; 
                BorderRingMesh.material = SelectedHotspotMat;
                SelectionMenu.Instance.SelectHotspot(this, false);
            }
            else
            { 
                BorderRingMesh.enabled = false;
            }

            Logo.material.color = Color.white;
        }
    }

    public bool IsClose()
    {
        float distance = artwork.max_distance > 0.01f ? artwork.max_distance : 0.12f;

        bool state = DistanceValidator.InRange(artwork); //_distance <= distance;
        if (state) OnDistanceValidated?.Invoke(distance);
        
        return state;
    }

    public void SetFormat(float zoomLevel)
    {
        ZoomLevel = zoomLevel;
        Zoom = ZoomLevel;
        
        OnDistanceValidated?.Invoke(artwork.max_distance > 0.01f ? artwork.max_distance : 0.12f);
        
        bool zoomed = zoomLevel < MinZoom;
        
        if (zoomed && !ZoomedOut) 
            SetStage(true);
        else if (!zoomed && ZoomedOut) 
            SetStage(false);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="IsZoomedOut">Should it be zoomed out?</param>
    void SetStage(bool IsZoomedOut) 
    {
        if (ZoomedOut == IsZoomedOut) return;
        ZoomedOut = IsZoomedOut;
        if (IsZoomedOut) 
        {
            ARObject.SetActive(false);
        } 
        else 
        {
            ARObject.SetActive(true);
            if (CanShow) SetReach(true);
        }
    }

    public void Unfocus() 
    {
        ShowSelectionBorder(false);
    }

    public static bool Touched;

    public void OnTouch() 
    {
        Touched = true;
        StartCoroutine(UnTouch());
        
        if (ZoomedOut) Navigation.FastTravelToPoint(new Vector2((float)artwork.longitude, (float)artwork.latitude), 17f, 2f);
        else if (CanShow)
        {
            StartAR(artwork);
        }
        else 
        {
            if (selected)
            {
                //GetDirections();
                //Navigation.SetCustomNavigation(new Vector2((float)arPoint.Longitude, (float)arPoint.Latitude), arPoint.Title, this);
            }
            else
            {
                ShowSelectionBorder(true);
            }
        }
    }

    public IEnumerator UnTouch() 
    {
        yield return new WaitForSeconds(0.25f);
        Touched = false;
    }

    public void ShowSelectionBorder(bool state)
    {
        selected = state;
        //markerGroup.SelectedGroup = state;
        BorderRingMesh.enabled = state;
        BorderRingMesh.material = SelectedHotspotMat;
        if (state)
        {
            SelectionMenu.Instance?.SelectHotspot(this, inReach);
        }
    }


    //Replaced ARPoint with ARPointSO
    public void StartAR(ArtworkData artwork)
    {
        // needs to be reenabled
        if (MapTutorialManager.TutorialActive) return;
        if (!CanOpenARScene(artwork))
        {
            return;
        }
        
        ARLoader.Open(artwork);
    }

    private bool CanOpenARScene(ArtworkData artwork)
    {
        // If the user is online
        if (!FirebaseLoader.OfflineMode && Application.internetReachability != NetworkReachability.NotReachable) return true;
        
        // if the artwork is a preset, without content
        if (!string.IsNullOrEmpty(artwork.preset) && artwork.content_list.Count <= 0) return true;

        foreach (var content in artwork.content_list)
        {
            var uri = new Uri(content.media_content);
            string encodedPath = uri.AbsolutePath;
            string decodedPath = Uri.UnescapeDataString(encodedPath);
            string fileName = Path.GetFileName(decodedPath);
            string localPath = Path.Combine(AppCache.ContentFolder, fileName);
            
            // if the file does not exist locally, return
            if (!File.Exists(localPath))
            {
                OnOfflineModeNoLocalInstance?.Invoke();
                return false;
            }
        }
        
        return true;
    }

    public ArtworkData GetHotspotArtwork()
    {
        return artwork;
    }

    private void OnDisable()
    {
        UnityEngine.Debug.Log("OnDisable");
    }
}
