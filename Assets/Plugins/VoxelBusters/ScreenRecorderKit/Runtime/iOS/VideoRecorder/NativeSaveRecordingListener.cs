#if UNITY_IOS
using System;
using System.Runtime.InteropServices;
using AOT;
using VoxelBusters.CoreLibrary.NativePlugins;

namespace VoxelBusters.ScreenRecorderKit.VideoRecorderCore.iOS
{
    public class NativeSaveRecordingListener
    {
        #region Delegates

        internal delegate void SaveRecordingOnSuccessNativeCallback(IntPtr tagPtr, IntPtr path);
        internal delegate void SaveRecordingOnFailureNativeCallback(IntPtr tagPtr, NativeError error);

        #endregion

        #region Public callbacks

        public Action<string>       onSuccessCallback;
        public Action<int, string>  onFailureCallback;

        #endregion

        #region Fields

        private IntPtr m_nativeHandle;

        #endregion

        #region Properties

        public IntPtr NativeHandle
        {
            get
            {
                return m_nativeHandle;
            }
            private set
            {
                m_nativeHandle = value;
            }
        }

        #endregion

        #region Life cycle

        public NativeSaveRecordingListener()
        {
            m_nativeHandle = MarshalUtility.GetIntPtr(this);
        }

        ~NativeSaveRecordingListener()
        {
            var tagHandle = GCHandle.FromIntPtr(m_nativeHandle);
            tagHandle.Free();
        }

        #endregion

        [MonoPInvokeCallback(typeof(SaveRecordingOnSuccessNativeCallback))]
        internal static void OnSuccessCallback(IntPtr tag, IntPtr path)
        {
            var tagHandle = GCHandle.FromIntPtr(tag);

            NativeSaveRecordingListener listener = (NativeSaveRecordingListener)tagHandle.Target;
            listener.onSuccessCallback(path.AsString());
        }

        [MonoPInvokeCallback(typeof(SaveRecordingOnFailureNativeCallback))]
        internal static void OnFailureCallback(IntPtr tag, NativeError error)
        {
            var tagHandle = GCHandle.FromIntPtr(tag);

            NativeActionCompleteListener listener = (NativeActionCompleteListener)tagHandle.Target;
            listener.onFailureCallback(error.Code, error.Description);
        }
    }
}
#endif
