using System;
using Messy.Definitions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class HotspotManager : MonoBehaviour
{   
    //Replaced ARPoint with ARPointSO
    [HideInInspector] public ARPointSO arPoint;
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
    
    [Header("UI Designs V2")]
    public MeshRenderer BorderRingMesh;
    public Material SelectedHotspotMat;
    public Material SelectedHotspotInRangeMat;
    public GameObject Shadow;
    
    [HideInInspector] public bool selected = false;
    [HideInInspector] public MarkerGroup markerGroup;
    public bool InPlayerRange => BorderRingMesh.enabled && BorderRingMesh.material == SelectedHotspotInRangeMat;

    [HideInInspector]
    public ExhibitionSO ConnectedExhibition;

    //[HideInInspector]
    public NavigationMaker Navigation;
    
    [Header("Zoom Settings")]
    public float MinZoom;
    public float DetailedZoom = 18;
    bool ZoomedOut;
    [HideInInspector] public float ZoomLevel = 16;
    public static float Zoom = 16;
    public static event Action<float> OnDistanceValidated;
    
    private bool inReach = false;
    
    //Replaced ARPoint with ARPointSO
    public void Init(ARPointSO point) {
        arPoint = point;
        if (arPoint.PlaceTextRight)
            RightTitle.text = arPoint.Title;
        else
            LeftTitle.text = arPoint.Title;

        //EnableInfo(true);
        if (ConnectedExhibition != null) {
            Logo.material.color = ConnectedExhibition.Color;
        }
        BackgroundAR.sprite = point.ARMapBackgroundImage;
        if (point.ARMapImage)
            ARObjectImage.sprite = point.ARMapImage;
    }

    private void OnEnable() {
        if (arPoint != null)
            OnChangeGpsPosition(_distance);
    }

    public void Update() {
        Color c = ConnectedExhibition.Color;
        c.a = 1f - ARObjectImage.color.a;
        Logo.material.color = c;
    }

    public void OnChangeGpsPosition(float distance) {
        _distance = distance;
        if (arPoint.PlaceTextRight)
            RightDistance.text = string.Format("{0}km", distance.ToString("F1"));
        else
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
    
    bool isShowingText;
    public void EnableInfo(bool ShouldEnable) {
        isShowingText = ShouldEnable;
        
        /*
         // Legacy code to show info about each hotspot
        if (arPoint.PlaceTextRight) {
            LeftDistance.enabled = false;
            LeftTitle.enabled = false;
            RightTitle.enabled = ShouldEnable;
            RightDistance.enabled = ShouldEnable;
        } else {
            LeftDistance.enabled = ShouldEnable;
            LeftTitle.enabled = ShouldEnable;
            RightTitle.enabled = false;
            RightDistance.enabled = false;
        }
        */
        
        if (markerGroup != null)
        {
            markerGroup.zoomedIn = ShouldEnable;
            markerGroup.ShowArtworkPoints(ShouldEnable, true);
        }
    }

    public void SetReach(bool InReach)
    {
        inReach = InReach;

        if (!ZoomedOut && InReach)
        {
            BorderRingMesh.enabled = true;
            BorderRingMesh.material = SelectedHotspotInRangeMat;
            if(selected) SelectionMenu.Instance.Open(this, true);
        }
        else if(!InReach)
        {
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
        float distance = arPoint.MaxDistance > 0.01f ? arPoint.MaxDistance : 0.12f;
        
        bool state = _distance <= distance;
        if (state) OnDistanceValidated?.Invoke(distance);
        
        return state;
    }

    public void SetFormat(float zoomLevel)
    {
        ZoomLevel = zoomLevel;
        Zoom = ZoomLevel;
        
        OnDistanceValidated?.Invoke(arPoint.MaxDistance > 0.01f ? arPoint.MaxDistance : 0.12f);
        
        bool zoomed = zoomLevel < MinZoom;
        
        if (zoomed && !ZoomedOut) 
            SetStage(true);
        else if (!zoomed && ZoomedOut) 
            SetStage(false);
        
        if (zoomLevel >= DetailedZoom) 
            EnableInfo(true);
        else 
            EnableInfo(CanShow);
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

    public void Unfocus() {
        EnableInfo(false);
        ShowSelectionBorder(false);
    }

    public static bool Touched;

    public void OnTouch() 
    {
        Touched = true;
        StartCoroutine(UnTouch());
        
        if (ZoomedOut) Navigation.FastTravelToPoint(new Vector2((float)arPoint.Longitude, (float)arPoint.Latitude), 17f, 2f);
        else if (CanShow) StartAR(arPoint);
        else 
        {
            if (isShowingText)
            {
                Navigation.SetCustomNavigation(new Vector2((float)arPoint.Longitude, (float)arPoint.Latitude), arPoint.Title, this);
                ShowSelectionBorder(true);
            }
            else EnableInfo(true);
        }
    }

    public IEnumerator UnTouch() {
        yield return new WaitForSeconds(0.5f);
        Touched = false;
    }

    private void ShowSelectionBorder(bool state)
    {
        selected = state;
        markerGroup.SelectedGroup = state;
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
    public void StartAR(ARPointSO point)
    {
        if (MapTutorialManager.TutorialActive) return;
        
        ArTapper.ARPointToPlace = point;
        ArTapper.PlaceDirectly = point.PlayARObjectDirectly;
        ArTapper.DistanceWhenActivated = _distance;

        SceneManager.LoadScene("AR");
        /*if(string.IsNullOrEmpty(point.AlternateScene)){
            Debug.Log( "Loading Default AR Scene");
            SceneManager.LoadScene("AR");
        }else{
            Debug.Log( "Loading Alternate Scene "+point.AlternateScene );
            SceneManager.LoadScene(point.AlternateScene);
        }*/
    }

    public ARPointSO GetHotspotARPointSO()
    {
        return arPoint;
    }
}
