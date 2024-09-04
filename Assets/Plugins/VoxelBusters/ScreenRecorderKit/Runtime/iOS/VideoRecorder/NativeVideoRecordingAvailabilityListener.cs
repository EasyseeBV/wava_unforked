#if UNITY_IOS
using System;
using System.Runtime.InteropServices;
using AOT;
using VoxelBusters.CoreLibrary.NativePlugins;

namespace VoxelBusters.ScreenRecorderKit.VideoRecorderCore.iOS
{
    public class NativeRecordingAvailabilityListener
    {
        #region Delegates

        internal delegate void RecordingAvailabilityOnAvailableNativeCallback(IntPtr tagPtr, IntPtr path);
        
        #endregion

        #region Public callbacks

        public Action<string> onAvailableCallback;

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

        public NativeRecordingAvailabilityListener()
        {
            m_nativeHandle = MarshalUtility.GetIntPtr(this);
        }

        ~NativeRecordingAvailabilityListener()
        {
            var tagHandle = GCHandle.FromIntPtr(m_nativeHandle);
            tagHandle.Free();
        }

        #endregion

        [MonoPInvokeCallback(typeof(RecordingAvailabilityOnAvailableNativeCallback))]
        public static void RecordingOnAvailableCallback(IntPtr tag, IntPtr path)
        {
            var tagHandle = GCHandle.FromIntPtr(tag);

            NativeRecordingAvailabilityListener listener = (NativeRecordingAvailabilityListener)tagHandle.Target;
            listener.onAvailableCallback(path.AsString());
        }
    }
}
#endif