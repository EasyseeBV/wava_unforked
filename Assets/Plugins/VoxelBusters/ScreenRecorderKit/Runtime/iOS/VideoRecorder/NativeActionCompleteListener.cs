#if UNITY_IOS
using System;
using System.Runtime.InteropServices;
using AOT;
using VoxelBusters.CoreLibrary.NativePlugins;

namespace VoxelBusters.ScreenRecorderKit.VideoRecorderCore.iOS
{
    public class NativeActionCompleteListener
    {
        #region Delegates

        internal delegate void ActionCompleteOnSuccessNativeCallback(IntPtr tagPtr);
        internal delegate void ActionCompleteOnFailureNativeCallback(IntPtr tagPtr, NativeError error);

        #endregion

        #region Public callbacks

        public Action onSuccessCallback;
        public Action<int, string> onFailureCallback;

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

        public NativeActionCompleteListener()
        {
            m_nativeHandle = MarshalUtility.GetIntPtr(this);
        }

        ~NativeActionCompleteListener()
        {
            var tagHandle = GCHandle.FromIntPtr(m_nativeHandle);
            tagHandle.Free();
        }

        #endregion

        [MonoPInvokeCallback(typeof(ActionCompleteOnSuccessNativeCallback))]
        internal static void OnSuccessCallback(IntPtr tag)
        {
            var tagHandle = GCHandle.FromIntPtr(tag);

            NativeActionCompleteListener listener = (NativeActionCompleteListener)tagHandle.Target;
            listener.onSuccessCallback();
        }

        [MonoPInvokeCallback(typeof(ActionCompleteOnFailureNativeCallback))]
        internal static void OnFailureCallback(IntPtr tag, NativeError error)
        {
            var tagHandle = GCHandle.FromIntPtr(tag);

            NativeActionCompleteListener listener = (NativeActionCompleteListener)tagHandle.Target;
            listener.onFailureCallback(error.Code, error.Description);
        }
    }
}
#endif
