/*****************************************************
// File: TMP Container Size Fitter
// Author: The Messy Coder
// Date: March 2022
// Please support by following on Twitch & social media
// Twitch:   www.Twitch.tv/TheMessyCoder
// YouTube:  www.YouTube.com/TheMessyCoder
// Facebook: www.Facebook.com/TheMessyCoder
// Twitter:  www.Twitter.com/TheMessyCoder
*****************************************************/
 
using UnityEngine;
 
namespace Messy
{
    [ExecuteAlways]
    [AddComponentMenu("Layout/TMP Container Size Fitter")]
    public class TextContainerFitter : MonoBehaviour
    {
        public bool FixHeight;
        public bool FixWidth;
        public Vector2 Offset;

        [SerializeField]
        private TMPro.TextMeshProUGUI m_TMProUGUI;
        public TMPro.TextMeshProUGUI TextMeshPro
        {
            get
            {
                if (m_TMProUGUI == null && transform.GetComponentInChildren<TMPro.TextMeshProUGUI>())
                {
                    m_TMProUGUI = transform.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                    m_TMPRectTransform = m_TMProUGUI.rectTransform;
                }
                return m_TMProUGUI;
            }
        }
 
        protected RectTransform m_RectTransform;
        public RectTransform rectTransform
        {
            get
            {
                if (m_RectTransform == null)
                {
                    m_RectTransform = GetComponent<RectTransform>();
                }
                return m_RectTransform;
            }
        }
 
        protected RectTransform m_TMPRectTransform;
        public RectTransform TMPRectTransform { get { return m_TMPRectTransform; } }
 
        protected float m_PreferredHeight;
        public float PreferredHeight { get { return m_PreferredHeight; } }

        protected float m_PreferredWidth;
        public float PreferredWidth { get { return m_PreferredWidth; } }

        protected virtual void SetHeight()
        {
            if (TextMeshPro == null)
                return;
 
            m_PreferredHeight = TextMeshPro.preferredHeight;
            rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, PreferredHeight + Offset.y);
 
            SetDirty();
        }

        protected virtual void SetWidth() {
            if (TextMeshPro == null)
                return;

            m_PreferredWidth = TextMeshPro.preferredWidth;
            rectTransform.sizeDelta = new Vector2(PreferredWidth + Offset.x, rectTransform.sizeDelta.y);

            SetDirty();
        }

        protected virtual void OnEnable() {
            if (FixHeight)
                SetHeight();
            if (FixWidth)
                SetWidth();
        }
 
        protected virtual void Start()
        {
            if (FixHeight)
                SetHeight();
            if (FixWidth)
                SetWidth();
        }
 
        protected virtual void Update()
        {
            if (PreferredHeight != TextMeshPro.preferredHeight && FixHeight)
            {
                SetHeight();
            }
            if (PreferredWidth != TextMeshPro.preferredWidth && FixWidth) {
                SetWidth();
            }
        }
 
        #region MarkLayoutForRebuild
        public virtual bool IsActive()
        {
            return isActiveAndEnabled;
        }
 
        protected void SetDirty()
        {
            if (!IsActive())
                return;
 
            UnityEngine.UI.LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
        }
 
 
        protected virtual void OnRectTransformDimensionsChange()
        {
            SetDirty();
        }
 
 
        #if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            SetDirty();
        }
        #endif
 
        #endregion
    }
}