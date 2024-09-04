#if UNITY_IOS
using System;
using System.Runtime.InteropServices;

namespace VoxelBusters.ScreenRecorderKit.VideoRecorderCore.iOS
{
    public class NativeVideoRecorderSettings
    {
        #region Native Bindings

        [DllImport("__Internal")]
        public static extern IntPtr ScreenRecorderKit_VideoRecorderSettings_Create();

        [DllImport("__Internal")]
        public static extern void ScreenRecorderKit_VideoRecorderSettings_SetEnableMicrophone(IntPtr settings, bool value);

        [DllImport("__Internal")]
        public static extern bool ScreenRecorderKit_VideoRecorderSettings_GetEnableMicrophone(IntPtr settings);


        public IntPtr NativeHandle
        {
            get;
            private set;
        }

        public bool EnableMicrophone
        {
            get
            {
                return ScreenRecorderKit_VideoRecorderSettings_GetEnableMicrophone(NativeHandle);
            }

            private set
            {
                ScreenRecorderKit_VideoRecorderSettings_SetEnableMicrophone(NativeHandle, value);
            }
        }

        public NativeVideoRecorderSettings(VideoRecorderSettings settings, VideoRecorderRuntimeSettings runtimeSettings)
        {
            NativeHandle = ScreenRecorderKit_VideoRecorderSettings_Create();
            EnableMicrophone = runtimeSettings.EnableMicrophone ?? false;
        }

        #endregion
    }
}
#endif
