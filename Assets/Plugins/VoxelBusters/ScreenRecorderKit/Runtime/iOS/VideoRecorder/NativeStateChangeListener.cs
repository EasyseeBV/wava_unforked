#if UNITY_IOS
using System;
using System.Runtime.InteropServices;
using AOT;
using VoxelBusters.CoreLibrary.NativePlugins;

namespace VoxelBusters.ScreenRecorderKit.VideoRecorderCore.iOS
{
    public class NativeStateChangeListener
    {
        #region Delegates

        internal delegate void StateChangeOnNewStateNativeCallback(IntPtr tagPtr);

        #endregion

        #region Public callbacks

        public Action onInvalid;
        public Action onPrepare;
        public Action onRecord;
        public Action onPause;
        public Action onStop;

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

        public NativeStateChangeListener()
        {
            m_nativeHandle = MarshalUtility.GetIntPtr(this);
        }

        ~NativeStateChangeListener()
        {
            var tagHandle = GCHandle.FromIntPtr(m_nativeHandle);
            tagHandle.Free();
        }

        #endregion

        [MonoPInvokeCallback(typeof(StateChangeOnNewStateNativeCallback))]
        internal static void OnInvalid(IntPtr tag)
        {
            var tagHandle = GCHandle.FromIntPtr(tag);

            NativeStateChangeListener listener = (NativeStateChangeListener)tagHandle.Target;
            listener.onInvalid();
        }


        [MonoPInvokeCallback(typeof(StateChangeOnNewStateNativeCallback))]
        internal static void OnPrepare(IntPtr tag)
        {
            var tagHandle = GCHandle.FromIntPtr(tag);

            NativeStateChangeListener listener = (NativeStateChangeListener)tagHandle.Target;
            listener.onPrepare();
        }


        [MonoPInvokeCallback(typeof(StateChangeOnNewStateNativeCallback))]
        internal static void OnRecord(IntPtr tag)
        {
            var tagHandle = GCHandle.FromIntPtr(tag);

            NativeStateChangeListener listener = (NativeStateChangeListener)tagHandle.Target;
            listener.onRecord();
        }


        [MonoPInvokeCallback(typeof(StateChangeOnNewStateNativeCallback))]
        internal static void OnPause(IntPtr tag)
        {
            var tagHandle = GCHandle.FromIntPtr(tag);

            NativeStateChangeListener listener = (NativeStateChangeListener)tagHandle.Target;
            listener.onPause();
        }


        [MonoPInvokeCallback(typeof(StateChangeOnNewStateNativeCallback))]
        internal static void OnStop(IntPtr tag)
        {
            var tagHandle = GCHandle.FromIntPtr(tag);

            NativeStateChangeListener listener = (NativeStateChangeListener)tagHandle.Target;
            listener.onStop();
        }

    }
}
#endif
