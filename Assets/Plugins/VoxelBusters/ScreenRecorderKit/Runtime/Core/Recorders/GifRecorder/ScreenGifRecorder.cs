using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VoxelBusters.CoreLibrary;

namespace VoxelBusters.ScreenRecorderKit
{
    public class ScreenGifRecorder : GifRecorder
    {
        #region Fields

        private     RenderTexture   m_fullScreenRenderTexture;

        private     IEnumerator     m_recordCoroutine; 

        #endregion

        #region Static methods

        public static ScreenGifRecorder Create(GifRecorderSettings settings = null)
        {
            var     instance            = FindObjectOfType<ScreenGifRecorder>() ?? GameObjectUtility.CreateGameObjectWithComponent<ScreenGifRecorder>("");
            instance.gameObject.name    = "ScreenGifRecorder";
            instance.Settings           = settings;
            DontDestroyOnLoad(instance.gameObject);

            return instance;
        }

        #endregion

        #region Base class implementation

        protected override void Init()
        {
            base.Init();

            // Create render texture
            m_fullScreenRenderTexture   = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32)
            {
                wrapMode    = TextureWrapMode.Clamp,
                filterMode  = FilterMode.Bilinear,
                anisoLevel  = 0,
            }; 
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            // Release render texture
            DestroyTexture(m_fullScreenRenderTexture);
        }

        internal override void OnStateChange(ScreenRecorderState state)
        {
            base.OnStateChange(state);

            if (state == ScreenRecorderState.Record)
            {
                m_recordCoroutine   = RecordFramesCoroutine();
                StartCoroutine(m_recordCoroutine);
            }
            else
            {
                if (m_recordCoroutine != null)
                {
                    StopCoroutine(m_recordCoroutine);
                    m_recordCoroutine   = null;
                }
            }
        }

        protected override bool IsVirtualRecorder() => true;

        #endregion

        #region Private methods

        private IEnumerator RecordFramesCoroutine()
        {
            do
            {
                yield return new WaitForEndOfFrame();

                // Capture screen frame
                if (State == ScreenRecorderState.Record)
                {
			        UpdateFrameCaptureTimer(out bool canRecord);
			        if (canRecord)
                    {
                        ScreenCapture.CaptureScreenshotIntoRenderTexture(m_fullScreenRenderTexture);

                        var     newFrame    = GetOrCreateFrame();
                        Graphics.Blit(m_fullScreenRenderTexture, newFrame);
                        AddFrame(newFrame);
                    }
                }
            } while (true);
        }

        #endregion
    }
}