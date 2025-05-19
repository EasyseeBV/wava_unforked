using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VoxelBusters.CoreLibrary;

namespace VoxelBusters.ScreenRecorderKit
{
    public interface IExternalServiceProvider
    {
        #region Methods

        void ShareGif(string filePath, string title = null, string description = null, CompletionCallback callback = null);

        void ShareVideo(string filePath, string title = null, string description = null, CompletionCallback callback = null);

        #endregion
    }
}