using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Michsky.MUIP
{
    [RequireComponent(typeof(Animator))]
    public class AnimatedIconHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [Header("Settings")]
        public PlayType playType;
        public Animator iconAnimator;

        bool isClicked;

        public UnityEvent ShowMenu;
        public UnityEvent HideMenu;

        public enum PlayType
        {
            Click,
            Hover,
            None
        }

        void Start()
        {
            if (iconAnimator == null)
                iconAnimator = gameObject.GetComponent<Animator>();
        }

        public void PlayIn() { iconAnimator.Play("In"); }
        public void PlayOut() { iconAnimator.Play("Out"); }

        public void ClickEvent()
        {
            if (isClicked == true) { 
                HideMenu.Invoke(); 
                PlayOut();
                isClicked = false; 
            } 
            else { 
                ShowMenu.Invoke(); 
                PlayIn(); 
                isClicked = true; 
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (playType == PlayType.Click)
                ClickEvent();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (playType == PlayType.Hover)
                iconAnimator.Play("In");
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (playType == PlayType.Hover)
                iconAnimator.Play("Out");
        }
    }
}