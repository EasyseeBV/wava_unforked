using UnityEngine;

public class MapFilterEndAnimation : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private MapFilterToggle toggler;
    
    private const string DISPLAY_ANIMATION_KEY = "Display";

    public void NotifyMapToggle()
    {
        if (!animator.GetBool(DISPLAY_ANIMATION_KEY))
        {
            toggler.OnAnimationComplete();
        }
    }
}
