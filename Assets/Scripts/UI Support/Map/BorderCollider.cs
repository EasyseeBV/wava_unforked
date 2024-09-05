using System;
using UnityEngine;

public class BorderCollider : MonoBehaviour
{
    [SerializeField] private HotspotManager hotspotManager;
    [SerializeField] private Transform parentTransform;
    
    private float targetScale;

    private void Awake()
    {
        targetScale = 10.2f;
    }

    private void Update()
    {
        //transform.localScale = new Vector3 (targetScale/parentTransform.localScale.x, targetScale/parentTransform.localScale.y,targetScale/parentTransform.localScale.z);
    }

    private void OnEnable()
    {
        HotspotManager.OnDistanceValidated += UpdateScale;
    }
    
    private void OnDisable()
    {
        HotspotManager.OnDistanceValidated -= UpdateScale;
    }

    private void UpdateScale(float target)
    {
        targetScale = target * 100;
        
        /*Vector3 relativeScale = new Vector3(
            initialParentScale.x / parentTransform.localScale.x,
            initialParentScale.y / parentTransform.localScale.y,
            initialParentScale.z / parentTransform.localScale.z
        );

        transform.localScale = relativeScale * targetScale;*/
    }
    
    private void OnValidate()
    {
        if (!hotspotManager) hotspotManager = GetComponentInParent<HotspotManager>();
    }
}
