using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VoxelBusters.CoreLibrary;

namespace VoxelBusters.ScreenRecorderKit
{
    /// <summary>
    /// Builder for creating an instance of IScreenRecorder for Video or Gif.
    /// </summary>
    [IncludeInDocs]
    public partial class ScreenRecorderBuilder
    {
        #region Fields

        private     readonly    IScreenRecorder     m_instance;

        #endregion

        #region Constructors

        private ScreenRecorderBuilder(IScreenRecorder instance)
        {
            // Set properties
            m_instance  = instance;
        }

        #endregion

        #region Setter methods

        public ScreenRecorderBuilder SetOnRecordingAvailable(SuccessCallback<ScreenRecorderRecordingAvailableResult> callback = null)
        {
            m_instance.SetOnRecordingAvailable(callback);

            return this;
        }

        #endregion

        #region Public methods

        public IScreenRecorder Build()
        {
            return m_instance;
        }

        #endregion
    }
}