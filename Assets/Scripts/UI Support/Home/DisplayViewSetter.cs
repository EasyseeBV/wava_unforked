using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class DisplayViewSetter : MonoBehaviour
{
    [SerializeField] private DisplayView view;
    [SerializeField] private SimpleSceneLoader sceneLoader;

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(SetDisplay);
    }

    public void SetDisplay()
    {
        AutoLoadDisplay.View = view;
        sceneLoader.LoadScene("Exhibition&Art");
    }
}
