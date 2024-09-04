using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VoxelBusters.CoreLibrary;

namespace VoxelBusters.ScreenRecorderKit
{
    [RequireComponent(typeof(Camera))]
    public class CameraGifRecorder : GifRecorder
    {
        #region Fields

        private     Camera      m_camera;

        #endregion

        #region Static methods

        public static CameraGifRecorder Create(Camera camera, GifRecorderSettings settings = null)
        {
            Assert.IsArgNotNull(camera, nameof(camera));

            var     instance    = camera.gameObject.AddComponentIfNotFound<CameraGifRecorder>();
            instance.Settings   = settings;

            return instance;
        }

        #endregion

        #region Base class implementation

        protected override void Init()
        {
            base.Init();

            // cache components
            m_camera    = GetComponent<Camera>();
        }

        public override bool CanRecord(out Error error)
        {
            if (!enabled)
            {
                error   = new Error("Camera is disabled.");
                return false;
            }
            return base.CanRecord(out error);
        }

        protected override float AspectRatio()
        {
            return m_camera.aspect;
        }

        #endregion

        #region Unity callback methods

        private void OnRenderImage(RenderTexture source, RenderTexture destination)
		{
            // In record mode, we need to capture the frame
			if (State == ScreenRecorderState.Record)
			{
				// Copy current frame data and add it to cache
			    UpdateFrameCaptureTimer(out bool canRecord);
			    if (canRecord)
			    {
				    var     rt  = GetOrCreateFrame();
				    Graphics.Blit(source, rt);
				    AddFrame(rt);
			    }
			}

			Graphics.Blit(source, destination);
		}

        #endregion
    }
}