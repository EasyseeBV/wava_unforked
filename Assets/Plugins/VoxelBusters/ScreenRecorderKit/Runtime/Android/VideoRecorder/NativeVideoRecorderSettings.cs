#if UNITY_ANDROID
using UnityEngine;

namespace VoxelBusters.ScreenRecorderKit.VideoRecorderCore.Android
{
    public class NativeVideoRecorderSettings
    {
        #region Properties

        public AndroidJavaObject NativeObject
        {
            get;
            private set;
        }

        public bool EnableMicrophone
        {
            get
            {
                return NativeObject.Get<bool>(Native.Fields.EnableMicrophone);
            }

            private set
            {
                NativeObject.Set(Native.Fields.EnableMicrophone, value);
            }
        }

        public float CustomBitrateFactor
        {
            get
            {
                return NativeObject.Get<float>(Native.Fields.CustomBitrateFactor);
            }

            private set
            {
                NativeObject.Set(Native.Fields.CustomBitrateFactor, value);
            }
        }

        public bool PrioritiseAppAudioWhenUsingMicrophone
        {
            get
            {
                return NativeObject.Get<bool>(Native.Fields.PrioritiseAppAudioWhenUsingMicrophone);
            }

            private set
            {
                NativeObject.Set(Native.Fields.PrioritiseAppAudioWhenUsingMicrophone, value);
            }
        }

        public int VideoQuality
        {
            get
            {
                return NativeObject.Get<int>(Native.Fields.VideoQuality);
            }

            private set
            {
                NativeObject.Set(Native.Fields.VideoQuality, value);
            }
        }

        #endregion

        public NativeVideoRecorderSettings(VideoRecorderSettings settings, VideoRecorderRuntimeSettings runtimeSettings)
        {
            NativeObject = new AndroidJavaObject(Native.Class.Name);
            EnableMicrophone = runtimeSettings.EnableMicrophone ?? false;
            CustomBitrateFactor = settings.AndroidProperties.CustomBitrateSetting.AllowCustomBitrates ? settings.AndroidProperties.CustomBitrateSetting.BitrateFactor : -1;
            PrioritiseAppAudioWhenUsingMicrophone = settings.AndroidProperties.PrioritiseAppAudioWhenUsingMicrophone;
            VideoQuality = (int)GetNativeVideoQualityType(settings.AndroidProperties.VideoQuality);
        }

        private NativeVideoQualityType GetNativeVideoQualityType(VideoRecorderQuality videoQuality)
        {
            switch (videoQuality)
            {
                case VideoRecorderQuality.QUALITY_MATCH_SCREEN_SIZE:
                    return NativeVideoQualityType.QUALITY_MATCH_SCREEN_SIZE;

                case VideoRecorderQuality.QUALITY_1080P:
                    return NativeVideoQualityType.QUALITY_1080P;

                case VideoRecorderQuality.QUALITY_720P:
                    return NativeVideoQualityType.QUALITY_720P;

                case VideoRecorderQuality.QUALITY_480P:
                    return NativeVideoQualityType.QUALITY_480P;

                default:
                    throw new System.Exception("Not implemented on native : " + videoQuality);

            }
        }

        private class Native
        {
            public class Class
            {
                public const string Name = "com.voxelbusters.screenrecorderkit.videorecorder.datatypes.VideoRecorderSettings";
            }

            public class Fields
            {
                public const string EnableMicrophone                        = "enableMicrophone";
                public const string CustomBitrateFactor                     = "customBitrateFactor";
                public const string PrioritiseAppAudioWhenUsingMicrophone   = "prioritiseAppAudio";
                public const string VideoQuality                            = "videoQuality";
            }
        }
    }
}
#endif
