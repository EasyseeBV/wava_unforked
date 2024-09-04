#if UNITY_ANDROID
using UnityEngine;
using System;
using VoxelBusters.CoreLibrary;
using VoxelBusters.CoreLibrary.NativePlugins;
using VoxelBusters.ScreenRecorderKit.VideoRecorderCore.Internal;

namespace VoxelBusters.ScreenRecorderKit.VideoRecorderCore.Android
{
    internal class Native
    {
        internal const string ROOT_NAMESPACE                    = "com.voxelbusters.screenrecorderkit.videorecorder.interfaces";
        internal const string ACTION_COMPLETE_LISTENER          = ROOT_NAMESPACE + ".IActionCompleteListener";
        internal const string SAVE_RECORDING_LISTENER           = ROOT_NAMESPACE + ".ISaveRecordingListener";
        internal const string RECORDING_AVAILABILITY_LISTENER   = ROOT_NAMESPACE + ".IRecordingAvailabilityListener";
        internal const string RECORDER_STATE_CHANGE_LISTENER    = ROOT_NAMESPACE + ".IRecorderStateChangeListener";
    }

    internal interface IActionCompleteListener
    {
        void OnSuccess();
        void OnFailure(string code, string description);
    }

    interface ISaveRecordingListener
    {
        void OnSaveSuccess(string path);
        void OnSaveFailure(string errorCode, string errorDescription);
    }

    internal interface IRecordingAvailabilityListener
    {
        void OnAvailable(string path);
    }

    internal class NativeActionCompleteListener : AndroidJavaProxy, IActionCompleteListener
    {
        #region Callbacks

        private readonly Action               onSuccessCallback;
        private readonly Action<Error>        onFailureCallback;

        #endregion


        #region Constructors

        public NativeActionCompleteListener(CompletionCallback callback) : base(Native.ACTION_COMPLETE_LISTENER)
        {
            onSuccessCallback = () => CallbackDispatcher.InvokeOnMainThread(callback, true, null);
            onFailureCallback = (error) => CallbackDispatcher.InvokeOnMainThread(callback, false, error);
        }

        #endregion

        public void OnSuccess()
        {
            onSuccessCallback?.Invoke();
        }

        public void OnFailure(string code, string description)
        {
            onFailureCallback?.Invoke(new Error(VideoRecorder.ErrorDomain, NativeUtility.GetErrorCode(code), description));
        }
    }


    internal class NativeSaveRecordingListener : AndroidJavaProxy, ISaveRecordingListener
    {
        #region Callbacks

        private readonly Action<string> onSuccessCallback;
        private readonly Action<Error> onFailureCallback;

        #endregion


        #region Constructors

        public NativeSaveRecordingListener(CompletionCallback<ScreenRecorderSaveRecordingResult> callback) : base(Native.SAVE_RECORDING_LISTENER)
        {
            onSuccessCallback = (path) => CallbackDispatcher.InvokeOnMainThread(callback, new ScreenRecorderSaveRecordingResult(path), null);
            onFailureCallback = (error) => CallbackDispatcher.InvokeOnMainThread(callback, null, error);
        }

        #endregion

        public void OnSaveSuccess(string path)
        {
            onSuccessCallback?.Invoke(path);
        }

        public void OnSaveFailure(string code, string description)
        {
            onFailureCallback?.Invoke(new Error(VideoRecorder.ErrorDomain, NativeUtility.GetErrorCode(code), description));
        }
    }

    internal class NativeRecordingAvailabilityListener : AndroidJavaProxy, IRecordingAvailabilityListener
    {
        #region Delegates

        internal delegate void OnAvailableDelegate(string path);

        #endregion

        #region Public callbacks

        public OnAvailableDelegate onAvailable;

        #endregion

        #region Constructors

        public NativeRecordingAvailabilityListener() : base(Native.RECORDING_AVAILABILITY_LISTENER)
        {
        }

        #endregion

        public void OnAvailable(string path)
        {
            onAvailable?.Invoke(path);
        }
    }

    internal class NativeRecorderStateChangeListener : AndroidJavaProxy, IRecorderStateChangeListener
    {
        #region Delegates

        internal delegate void OnNewStateDelegate();

        #endregion

        #region Public callbacks

        public OnNewStateDelegate onPrepare;
        public OnNewStateDelegate onRecord;
        public OnNewStateDelegate onPause;
        public OnNewStateDelegate onStop;
        public OnNewStateDelegate onInvalid;

        #endregion

        #region Constructors

        public NativeRecorderStateChangeListener() : base(Native.RECORDER_STATE_CHANGE_LISTENER)
        {
        }

        public void OnPrepare()
        {
            onPrepare?.Invoke();
        }

        public void OnRecord()
        {
            onRecord?.Invoke();
        }

        public void OnPause()
        {
            onPause?.Invoke();
        }

        public void OnStop()
        {
            onStop?.Invoke();
        }

        public void OnInvalid()
        {
            onInvalid?.Invoke();
        }

        #endregion
    }
}
#endif