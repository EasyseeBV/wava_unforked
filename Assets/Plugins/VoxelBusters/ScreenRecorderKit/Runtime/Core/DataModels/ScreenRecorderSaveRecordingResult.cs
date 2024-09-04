using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VoxelBusters.ScreenRecorderKit
{
    public class ScreenRecorderSaveRecordingResult
    {
        #region Properties

        public string Path { get; private set; }

        #endregion

        #region Constructors

        public ScreenRecorderSaveRecordingResult(string path)
        {
            // Set properties
            Path    = path;
        }

        #endregion
    }
}