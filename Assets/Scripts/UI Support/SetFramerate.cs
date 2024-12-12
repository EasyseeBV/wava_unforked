using UnityEngine;

public class SetFramerate : MonoBehaviour
{
    [System.Serializable]
    private enum Framerate
    {
        High,
        Default
    }

    [SerializeField] private Framerate framerate;

    private void Awake()
    {
        switch (framerate)
        {
            case Framerate.High:
                FrameRateManager.SetHighFrameRate();
                break;
            case Framerate.Default:
                FrameRateManager.SetDefaultFrameRate();
                break;
            default:
                FrameRateManager.SetDefaultFrameRate();
                Debug.Log($"Framerate [{framerate}] not implemented");
                break;
        }
    }
}
