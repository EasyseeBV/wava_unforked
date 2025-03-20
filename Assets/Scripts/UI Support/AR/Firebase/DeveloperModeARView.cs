using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Vector3 = UnityEngine.Vector3;

public class DeveloperModeARView : MonoBehaviour
{
    public enum ARTransformView
    {
        Position,
        Scale,
        Rotation
    }

    public enum ARAxis
    {
        X,
        Y,
        Z
    }
    
    [Header("References")]
    [SerializeField] private GameObject content;
    [SerializeField] private GameObject cameraButtonContent;
    [SerializeField] private Button enableDeveloperWindowButton;
    [Space]
    [SerializeField] private DeveloperButton[] developerButtons;
    [SerializeField] private DeveloperSaveButton developerSaveButton;
    [SerializeField] private DeveloperResetButton developerResetButton;

    [Header("Transform View")]
    [SerializeField] private GameObject transformContent;
    [SerializeField] private TMP_Text transformTypeLabel;
    [SerializeField] private TMP_InputField xInputField;
    [SerializeField] private TMP_InputField yInputField;
    [SerializeField] private TMP_InputField zInputField;
    
    private DeveloperButton cachedRecentlyUsedButton = null;
    private ARTransformView currentView;
    private ArTapper ar;
    
    private List<TransformsData> editedTransformDatas = new List<TransformsData>();

    private int viewNumber = 0;
    
    private void Awake()
    {
        if (!Debug.isDebugBuild)
        {
            content.SetActive(false);
            enableDeveloperWindowButton.gameObject.SetActive(false);
            return;
        }

        ar = FindObjectOfType<ArTapper>();

        foreach (var devButton in developerButtons)
        {
            devButton.AddOnClickListener(DeveloperButtonClicked);
        }
        
        enableDeveloperWindowButton.onClick.AddListener(() => EnableDeveloperWindow(true));
        
        xInputField.onEndEdit.AddListener(value => WriteData(float.Parse(value), ARAxis.X));
        yInputField.onEndEdit.AddListener(value => WriteData(float.Parse(value), ARAxis.Y));
        zInputField.onEndEdit.AddListener(value => WriteData(float.Parse(value), ARAxis.Z));
    }

    private void OnEnable() => developerResetButton.OnClick += OnReset;
    private void OnDisable() => developerResetButton.OnClick -= OnReset;

    private void EnableDeveloperWindow(bool state)
    {
        content.SetActive(state);
        cameraButtonContent.SetActive(!state);

        foreach (var data in ArTapper.ArtworkToPlace.content_list.Select(mediaContentData => mediaContentData.transforms).Select(originalData => new TransformsData()
                 {
                     position_offset = new PositionOffset()
                     {
                         x_offset = originalData.position_offset.x_offset,
                         y_offset = originalData.position_offset.y_offset,
                         z_offset = originalData.position_offset.z_offset
                     },
                     scale = new Scale()
                     {
                         x_scale = originalData.scale.x_scale,
                         y_scale = originalData.scale.y_scale,
                         z_scale = originalData.scale.z_scale
                     },
                     rotation = new Rotation()
                     {
                         x_rotation = originalData.rotation.x_rotation,
                         y_rotation = originalData.rotation.y_rotation,
                         z_rotation = originalData.rotation.z_rotation
                     }
                 }))
        {
            editedTransformDatas.Add(data);
        }
    }

    private void DeveloperButtonClicked(ARTransformView view, DeveloperButton devButton)
    {
        currentView = view;
        
        if (!devButton.ToggledOn)
        {
            cachedRecentlyUsedButton?.Untoggle();
            cachedRecentlyUsedButton = null;
            transformContent.SetActive(false);
            return;
        }
        
        cachedRecentlyUsedButton?.Untoggle();
        cachedRecentlyUsedButton = devButton;
        
        transformContent.SetActive(true);
        transformTypeLabel.text = view.ToString();

        UpdateInputFields();
    }

    public void WriteData(float value, ARAxis axis)
    {
        if (editedTransformDatas.Count < viewNumber) return;
        
        developerSaveButton.SetIsSavable(true);
        developerResetButton.SetIsSavable(true);
        
        ar.contentDict.TryGetValue(viewNumber, out var obj);
        
        switch (currentView)
        {
            case ARTransformView.Position:
                switch (axis)
                {
                    case ARAxis.X:
                        if(obj) obj.transform.localPosition = new Vector3(value, obj.transform.localPosition.y, obj.transform.localPosition.z);
                        editedTransformDatas[viewNumber].position_offset.x_offset = value;
                        break;
                    case ARAxis.Y:
                        if(obj) obj.transform.localPosition = new Vector3(obj.transform.localPosition.x, value, obj.transform.localPosition.z);
                        editedTransformDatas[viewNumber].position_offset.y_offset = value;
                        break;
                    case ARAxis.Z:
                        if(obj) obj.transform.localPosition = new Vector3(obj.transform.localPosition.x, obj.transform.localPosition.y, value);
                        editedTransformDatas[viewNumber].position_offset.z_offset = value;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(axis), axis, null);
                }
                break;
            case ARTransformView.Scale:
                switch (axis)
                {
                    case ARAxis.X:
                        if(obj) obj.transform.localScale = new Vector3(value, obj.transform.localScale.y, obj.transform.localScale.z);
                        editedTransformDatas[viewNumber].scale.x_scale = value;
                        break;
                    case ARAxis.Y:
                        if(obj) obj.transform.localScale = new Vector3(obj.transform.localScale.x, value, obj.transform.localScale.z);
                        editedTransformDatas[viewNumber].scale.y_scale = value;
                        break;
                    case ARAxis.Z:
                        if(obj) obj.transform.localScale = new Vector3(obj.transform.localScale.x, obj.transform.localScale.y, value);
                        editedTransformDatas[viewNumber].scale.z_scale = value;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(axis), axis, null);
                }
                break;
            case ARTransformView.Rotation:
                switch (axis)
                {
                    // rotation is done below instead (no benefit, just lazy)
                    case ARAxis.X:
                        editedTransformDatas[viewNumber].rotation.x_rotation = value;
                        break;
                    case ARAxis.Y:
                        editedTransformDatas[viewNumber].rotation.y_rotation = value;
                        break;
                    case ARAxis.Z:
                        editedTransformDatas[viewNumber].rotation.z_rotation = value;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(axis), axis, null);
                }
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        if(obj) obj.transform.rotation = Quaternion.Euler(editedTransformDatas[viewNumber].rotation.x_rotation, editedTransformDatas[viewNumber].rotation.y_rotation,editedTransformDatas[viewNumber].rotation.z_rotation);
    }

    private void OnSave()
    {
        // get all editable data
        // convert to dictionary <string "content_list", object editedList>
        // Get document ref: db.Collection("artworks").Document("ArTapper.ArtworkToPlace.id");
        // document ref.UpdateAsync(dictionary).ContinueWithOnMainThread
        // notify user on screen that the data has been uploaded
    }

    private void OnReset()
    {
        editedTransformDatas.Clear();
        foreach (var data in ArTapper.ArtworkToPlace.content_list.Select(mediaContentData => mediaContentData.transforms).Select(originalData => new TransformsData()
                 {
                     position_offset = new PositionOffset()
                     {
                         x_offset = originalData.position_offset.x_offset,
                         y_offset = originalData.position_offset.y_offset,
                         z_offset = originalData.position_offset.z_offset
                     },
                     scale = new Scale()
                     {
                         x_scale = originalData.scale.x_scale,
                         y_scale = originalData.scale.y_scale,
                         z_scale = originalData.scale.z_scale
                     },
                     rotation = new Rotation()
                     {
                         x_rotation = originalData.rotation.x_rotation,
                         y_rotation = originalData.rotation.y_rotation,
                         z_rotation = originalData.rotation.z_rotation
                     }
                 }))
        {
            editedTransformDatas.Add(data);
        }

        for (int i = 0; i < ar.contentDict.Keys.Count; i++)
        {
            var obj = ar.contentDict[ar.contentDict.Keys.ElementAt(i)];
            
            obj.transform.localPosition = new Vector3(ArTapper.ArtworkToPlace.content_list[i].transforms.position_offset.x_offset, 
                                                    ArTapper.ArtworkToPlace.content_list[i].transforms.position_offset.y_offset, 
                                                    ArTapper.ArtworkToPlace.content_list[i].transforms.position_offset.z_offset);
            
            obj.transform.localScale = new Vector3(ArTapper.ArtworkToPlace.content_list[i].transforms.scale.x_scale, 
                                                ArTapper.ArtworkToPlace.content_list[i].transforms.scale.y_scale, 
                                                ArTapper.ArtworkToPlace.content_list[i].transforms.scale.z_scale);
            
            obj.transform.rotation = Quaternion.Euler(ArTapper.ArtworkToPlace.content_list[i].transforms.rotation.x_rotation, ArTapper.ArtworkToPlace.content_list[i].transforms.rotation.y_rotation, ArTapper.ArtworkToPlace.content_list[i].transforms.rotation.z_rotation);
        }

        UpdateInputFields();
    }

    private void UpdateInputFields()
    {
        var transformData = editedTransformDatas[viewNumber];
        
        xInputField.text = currentView switch
        {
            ARTransformView.Position => transformData.position_offset.x_offset.ToString(CultureInfo.InvariantCulture),
            ARTransformView.Scale => transformData.scale.x_scale.ToString(CultureInfo.InvariantCulture),
            ARTransformView.Rotation => transformData.rotation.x_rotation.ToString(CultureInfo.InvariantCulture),
            _ => "0"
        };
        
        yInputField.text = currentView switch
        {
            ARTransformView.Position => transformData.position_offset.y_offset.ToString(CultureInfo.InvariantCulture),
            ARTransformView.Scale => transformData.scale.y_scale.ToString(CultureInfo.InvariantCulture),
            ARTransformView.Rotation => transformData.rotation.y_rotation.ToString(CultureInfo.InvariantCulture),
            _ => "0"
        };
        
        zInputField.text = currentView switch
        {
            ARTransformView.Position => transformData.position_offset.z_offset.ToString(CultureInfo.InvariantCulture),
            ARTransformView.Scale => transformData.scale.z_scale.ToString(CultureInfo.InvariantCulture),
            ARTransformView.Rotation => transformData.rotation.z_rotation.ToString(CultureInfo.InvariantCulture),
            _ => "0"
        };
    }
}