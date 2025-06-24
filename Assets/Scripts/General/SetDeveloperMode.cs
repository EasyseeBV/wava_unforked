using UnityEngine;

public class SetDeveloperMode : MonoBehaviour
{
    [SerializeField] private bool setDeveloperMode = false;
    [SerializeField] private bool setAdminMode = false;

    private static bool once = false;
    
    private void OnEnable()
    {
        if (once) return;
        once = true;
        AppSettings.DeveloperMode = setDeveloperMode;
        
#if UNITY_EDITOR
        //AppSettings.DeveloperMode = true;
#endif
    }
}