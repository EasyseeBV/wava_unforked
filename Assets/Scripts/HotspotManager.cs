using System;
using Messy.Definitions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.IO;

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

        //EnableInfo(true);
        if (ConnectedExhibition != null)
        {
            Color parsedColor = Color.white; // default color
            switch (ConnectedExhibition.color.ToLower())
            {
                case "green":
                    parsedColor = Color.green;
                    break;
                case "red":
                    parsedColor = Color.red;
                    break;
                case "blue":
                    parsedColor = Color.blue;
                    break;
                case "yellow":
                    parsedColor = Color.yellow;
                    break;
                case "purple":
                    parsedColor = new Color(0.5f, 0.1f, 0.9f, 255f);
                    break;
                case "pink":
                    parsedColor = new Color(0.9f, 0.4f, 0.95f, 255f);;
                    break;
                case "orange":
                    parsedColor = new Color(0.95f, 0.35f, 0.1f, 255f);;
                    break;
                default:
                    parsedColor = Color.green;
                    break;
            }
            Logo.material.color = parsedColor;
        }
        else
        {
            Logo.material.color = Color.green;
            Debug.Log("ConnectedExhibition is null");
        }

//        Debug.LogWarning("<color=blue>Disabled background images & color setting</color>");
        
        /*
        BackgroundAR.sprite = point.ARMapBackgroundImage;
        if (point.ARMapImage)
            ARObjectImage.sprite = point.ARMapImage;
        */
    }

    private void OnEnable() {
        if (artwork != null)
            OnChangeGpsPosition(_distance);
    }

    public void Update()
    {
        /*Color c = Color.green;//ConnectedExhibition.Color;
        c.a = 1f - ARObjectImage.color.a;
        Logo.material.color = c;*/
    }

    public void OnChangeGpsPosition(float distance) {
        _distance = distance;
        LeftDistance.text = string.Format("{0}km", distance.ToString("F1"));

        if (CanShow && !IsClose()) {
            CanShow = false;
            SetReach(false);
        }

        if (selected)
        {
            SelectionMenu.Instance.UpdateDistance(_distance);
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
        }

        if (!ZoomedOut && InReach)
        {
            if(selected) SelectionMenu.Instance.Open(this, true);
        }
        else if(!InReach)
        {
            inPlayerRange = false;
            if (selected)
            {
                BorderRingMesh.enabled = true; 
                BorderRingMesh.material = SelectedHotspotMat;
            }
            else
            { 
                BorderRingMesh.enabled = false;
            }
        }
        
        /*
         // Legacy code to display border with an animation
         
        Animator ani = GetComponent<Animator>();
        AnimatorClipInfo[] animatorinfo = ani.GetCurrentAnimatorClipInfo(0);
        if (animatorinfo.Length == 0)
            return;

        if (!ZoomedOut && InReach && !animatorinfo[0].clip.name.Equals("Show")) {
           // EnableInfo(false);
            GetComponent<Animator>().ResetTrigger("Base");
            GetComponent<Animator>().ResetTrigger("Hide");
            GetComponent<Animator>().SetTrigger("Show");
            if(selected) SelectionMenu.Instance.Open(this, true);

        } else if (!InReach && !animatorinfo[0].clip.name.Equals("Hide")) {
            GetComponent<Animator>().ResetTrigger("Show");
            GetComponent<Animator>().ResetTrigger("Base");
            GetComponent<Animator>().SetTrigger("Hide");
            //EnableInfo(true);
        }*/
    }

    public bool IsClose()
    {
        float distance = artwork.max_distance > 0.01f ? artwork.max_distance : 0.12f;
        
        bool state = _distance <= distance;
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
    void SetStage(bool IsZoomedOut) {
        if (ZoomedOut == IsZoomedOut)
            return;
        ZoomedOut = IsZoomedOut;
        if (IsZoomedOut) {
            ARObject.SetActive(false);
            GetComponent<Animator>().ResetTrigger("Show");
            GetComponent<Animator>().ResetTrigger("Hide");
            GetComponent<Animator>().SetTrigger("Base");
            //EnableInfo(false);
        } else {
            ARObject.SetActive(true);

            if (CanShow)
                SetReach(true);
            /*else
                EnableInfo(true);*/
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

    public void GetDirections()
    {
        string location = artwork.latitude + "," + artwork.longitude;
        
#if UNITY_ANDROID
        string url = "https://www.google.com/maps/dir/?api=1&destination=" + location;
        Application.OpenURL(url);
#elif UNITY_IOS
            // Apple Maps URL scheme for iOS
            string url = "http://maps.apple.com/?daddr=" + location;
            Application.OpenURL(url);
#endif
    }

    public IEnumerator UnTouch() {
        yield return new WaitForSeconds(0.5f);
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
            SelectionMenu.Instance?.Open(this, inReach);
            SelectionMenu.Instance?.UpdateDistance(_distance);
        }
        else SelectionMenu.Instance?.Close();
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
        
        ArTapper.ArtworkToPlace = artwork;
        ArTapper.PlaceDirectly = false; //artwork.PlayARObjectDirectly;
        Debug.LogWarning("PlaceObjectDirectly was disabled");
        ArTapper.DistanceWhenActivated = _distance;

        if(string.IsNullOrEmpty(artwork.alt_scene))
        {
            Debug.Log("Loading Default AR Scene");
            SceneManager.LoadScene("ARView");
        }
        else
        {
            Debug.Log("Loading Alternate Scene " + artwork.alt_scene);
            SceneManager.LoadScene(artwork.alt_scene);
        }
    }

    private bool CanOpenARScene(ArtworkData artwork)
    {
        // If the user is online
        if (!FirebaseLoader.OfflineMode) return true;
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
}
