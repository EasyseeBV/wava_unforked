using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VoxelBusters.ScreenRecorderKit
{
    public sealed class GifRecordingAvailableResult : ScreenRecorderRecordingAvailableResult
    {
        #region Properties

        public GifTexture Texture { get; private set; }

        #endregion

        #region Constructors

        public GifRecordingAvailableResult(GifTexture texture = null)
        {
            // set properties
            Texture     = texture;
        }

        #endregion

        #region Base class methods

        protected override object GetDataInternal() => Texture;

        #endregion
    }
}