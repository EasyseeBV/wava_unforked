using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VoxelBusters.ScreenRecorderKit
{
    internal sealed class GifRecorderPreviewStateChangeResult : ScreenRecorderPreviewStateChangeResult
    {
        #region Properties

        public GifTexture Texture { get; private set; }

        #endregion

        #region Constructors

        public GifRecorderPreviewStateChangeResult(ScreenRecorderPreviewState state, string path = null,
            GifTexture texture = null)
            : base(state, path)
        {
            // set properties
            Texture     = texture;
        }

        #endregion
    }
}