using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RealSunARSwitcher : MonoBehaviour
{
    [Tooltip("Drag and drop the gameobject that holds RealSunAR here")]
    public RealSunAR realSunAREngine;

    [Tooltip("Optional: If you want a gameobject enabled when you disable RealSunAR then place it here")]
    public GameObject interiorLighting;

    [Tooltip("Click here to force RealSunAR to recalculate the sun when re-activating it")]
    public bool forceSunRecalculation = false;
    private GameObject realSunARGameObject;

    private void Awake()
    {
        realSunARGameObject = realSunAREngine.gameObject as GameObject;
    }

    
    public void Disable_Sun_Enable_Lamps()
    {
        if (forceSunRecalculation) realSunAREngine.Reseter();
        realSunAREngine.killSwitch = true;
        realSunARGameObject.SetActive(false);
        if (interiorLighting != null) interiorLighting.SetActive(true);
        //Add here additional code to be executed during the switch from sunlight to interior lighting
    }

    public void Disable_Lamps_Enable_Sun()
    {
        if (interiorLighting != null) interiorLighting.SetActive(false);
        realSunARGameObject.SetActive(true);
        realSunAREngine.killSwitch = false;
        //Add here additional code to be executed during the switch from interior lighting to sunlight
    }
}
