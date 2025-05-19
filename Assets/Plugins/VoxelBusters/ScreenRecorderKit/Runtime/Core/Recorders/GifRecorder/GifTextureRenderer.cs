using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using VoxelBusters.CoreLibrary;

namespace VoxelBusters.ScreenRecorderKit
{
    [RequireComponent(typeof(RawImage))]
    public class GifTextureRenderer : MonoBehaviour
    {
        #region Fields

        [SerializeField]
        private     AspectRatioFitter.AspectMode    m_aspectMode        = AspectRatioFitter.AspectMode.WidthControlsHeight;
        private     RawImage                        m_rawImage;
        private     AspectRatioFitter               m_rawImageAspect;

        private     GifTexture m_gif;

        #endregion

        #region Properties

        public AspectRatioFitter.AspectMode AspectMode
        {
            get => m_aspectMode;
            set
            {
                m_aspectMode    = value;
                SetDirty();
            }
        }

        #endregion

        #region Static methods

        public static GifTextureRenderer CreateFullScreenRenderer(Canvas canvas = null, GameObject closeButtonPrefab = null,
            Vector2? closeButtonOffset = null, UnityAction<GifTextureRenderer> closeButtonAction = null)
        {
            // Find scene canvas reference
            if (canvas == null)
            {
                var     sceneCanvases   = new List<Canvas>(FindObjectsOfType<Canvas>());
                if (sceneCanvases.Count == 0)
                {
                    sceneCanvases.Add(CreateCanvas());
                }
                canvas                  = FindTopmostCanvas(sceneCanvases);
            }

            // Create renderer object
            var     rendererParentGO          = GameObjectUtility.CreateChild("GifRenderer(Fullscreen)", canvas.transform);
            var     rendererGO              = GameObjectUtility.CreateChild("GifRenderer", rendererParentGO.transform);
            var     renderer            = rendererGO.AddComponent<GifTextureRenderer>();
            renderer.AspectMode         = AspectRatioFitter.AspectMode.FitInParent;
            var     parentTransform   = (RectTransform)rendererParentGO.transform;
            parentTransform.anchorMin = Vector2.zero;
            parentTransform.anchorMax = Vector2.one;
            parentTransform.sizeDelta = Vector2.zero;

            if (SystemInfo.graphicsUVStartsAtTop)
            {
                rendererGO.transform.localScale = new Vector3(1f, -1f, 1f);
            }

            // Create close button
            if (closeButtonPrefab == null)
            {
                closeButtonPrefab       = ResourcesUtility.LoadBuiltinAsset<GameObject>("CloseButton");
            }
            var     closeButtonGO       = GameObjectUtility.Instantiate(closeButtonPrefab, rendererParentGO.transform);
            var     closeButtonTrans    = (RectTransform)closeButtonGO.transform;
            closeButtonTrans.pivot      = Vector2.one;
            closeButtonTrans.anchorMin  = Vector2.one;
            closeButtonTrans.anchorMax  = Vector2.one;
            closeButtonTrans.anchoredPosition   = closeButtonOffset ?? new Vector2(-32f, -32f);
            
            var     closeButton         = closeButtonGO.AddComponentIfNotFound<Button>();
            if (closeButtonAction != null)
            {
                closeButton.onClick.AddListener(() =>
                {
                    closeButtonAction(renderer);
                });
            }

            return renderer;
        }

        private static Canvas CreateCanvas()
        {
            var     canvasGO            = new GameObject("UICanvas", typeof(RectTransform));
            var     canvas              = canvasGO.AddComponent<Canvas>();

            // Set scaler properties
            var     canvasScaler                = canvasGO.AddComponentIfNotFound<CanvasScaler>();
            canvasScaler.uiScaleMode            = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.matchWidthOrHeight     = 0.5f;
            canvasScaler.referenceResolution    = new Vector2(1080f, 1920f);

            return canvas;
        }

        private static Canvas FindTopmostCanvas(IList<Canvas> array)
        {
            int     topmostSortOrder    = int.MinValue;
            Canvas  topCanvas           = null;
            foreach (var item in array)
            {
                if (item.sortingOrder > topmostSortOrder)
                {
                    topCanvas           = item;
                    topmostSortOrder    = item.sortingOrder;
                }
            }
            return topCanvas;
        }

        #endregion

        #region Properties

        public GifTexture Texture
        {
            get => m_gif;
            set
            {
                m_gif   = value;
                SetDirty();
            }
        }

        private bool IsVisible { get; set; }

        #endregion

        #region Unity methods

        private void Awake()
        {
            // Cache references
            m_rawImage          = GetComponent<RawImage>();
            m_rawImageAspect    = gameObject.AddComponentIfNotFound<AspectRatioFitter>();
        }

        private void OnEnable()
        {
            // Update visiblity status
            IsVisible   = true;

            SetDirty();
        }

        private void OnDisable()
        {
            // Update visiblity status
            IsVisible   = false;

            StopAllCoroutines();
        }

        #endregion

        #region Private methods

        private void SetDirty()
        {
            StopAllCoroutines();

            if (m_gif == null) return;

            // Update image component properties
            m_rawImageAspect.aspectMode     = m_aspectMode;
            m_rawImageAspect.aspectRatio    = ((float)m_gif.Width) / m_gif.Height;

            // Start animating
            StartCoroutine(AnimateCoroutine());
        }

        private IEnumerator AnimateCoroutine()
        {
            float   dt                  = 1f / m_gif.FramesPerSecond;
            int     currentFrameIndex   = 0;
            int     totalFrames         = m_gif.TotalFrames;
            while (true)
            {
                if (IsVisible && (m_rawImage != null))
                {
                    m_rawImage.texture  = m_gif.GetFrameAt(currentFrameIndex);
                }
                currentFrameIndex       = (currentFrameIndex + 1) % totalFrames;
                yield return new WaitForSecondsRealtime(dt);
            }
        }

        #endregion
    }
}