using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VoxelBusters.CoreLibrary;

namespace VoxelBusters.ScreenRecorderKit
{
    public abstract class ScreenRecorder : MonoBehaviourZ, IScreenRecorder
    {
        #region Static fields

        private     static  IExternalServiceProvider        s_serviceProvider;

        #endregion

        #region Static properties

        public static IExternalServiceProvider ServiceProvider
        {
            get => ObjectHelper.CreateInstanceIfNull(
                ref s_serviceProvider,
                () =>
                {
                    var     defaultType = ReflectionUtility.GetTypeFromAssemblyCSharp(
                        typeName: "VoxelBusters.ScreenRecorderKit.Addons.DefaultServiceProvider",
                        includeFirstPass: true);
                    return ReflectionUtility.CreateInstance(defaultType) as IExternalServiceProvider;
                });
            set
            {
                Assert.IsPropertyNotNull(value, nameof(value));

                // Set property
                s_serviceProvider   = value;
            }
        }


        #endregion

        #region IScreenRecorder implementation

        public abstract bool CanRecord();

        public abstract bool IsRecording();

        public abstract bool IsPausedOrRecording();

        public abstract void PrepareRecording(CompletionCallback callback = null);

        public abstract void StartRecording(CompletionCallback callback = null);

        public abstract void PauseRecording(CompletionCallback callback = null);

        public abstract void StopRecording(CompletionCallback callback = null);

        public abstract void StopRecording(bool flushMemory, CompletionCallback callback = null);

        public abstract void DiscardRecording(CompletionCallback callback = null);

        public abstract void SaveRecording(CompletionCallback<ScreenRecorderSaveRecordingResult> callback = null);

        public abstract void SaveRecording(string fileName, CompletionCallback<ScreenRecorderSaveRecordingResult> callback = null);

        public abstract void SetOnRecordingAvailable(SuccessCallback<ScreenRecorderRecordingAvailableResult> callback = null);

        public abstract void OpenRecording(CompletionCallback callback = null);

        public abstract void ShareRecording(string title = null, string message = null, CompletionCallback callback = null);

        public abstract void Flush();

        #endregion
    }
}