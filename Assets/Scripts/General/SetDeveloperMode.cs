using UnityEngine;

public class SetDeveloperMode : MonoBehaviour
{
    [SerializeField] private bool setDeveloperMode = false;

    private void OnEnable()
    {
        AppSettings.DeveloperMode = setDeveloperMode;
        
#if UNITY_EDITOR
        //AppSettings.DeveloperMode = true;
#endif
    }
}