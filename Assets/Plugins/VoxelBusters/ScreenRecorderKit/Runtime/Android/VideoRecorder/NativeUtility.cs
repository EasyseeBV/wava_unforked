#if UNITY_ANDROID
namespace VoxelBusters.ScreenRecorderKit.VideoRecorderCore.Android
{
    public class NativeUtility
    {
        public static int GetErrorCode(string nativeErrorCode)
        {

            switch (nativeErrorCode)
            {
                case NativeErrorCode.UNKNOWN:
                    return ScreenRecorderErrorCode.kUnknown;
                case NativeErrorCode.API_UNAVAILABLE:
                    return ScreenRecorderErrorCode.kApiUnavailable;
                case NativeErrorCode.RECORDING_IN_PROGRESS:
                    return ScreenRecorderErrorCode.kRecordingInProgress;
                case NativeErrorCode.PERMISSION_UNAVAILABLE:
                    return ScreenRecorderErrorCode.kPermissionUnavailable;
                case NativeErrorCode.ACTIVE_RECORDING_UNAVAILABLE:
                    return ScreenRecorderErrorCode.kActiveRecordingUnavailable;
                case NativeErrorCode.FEATURE_UNAVAILABLE:
                    return ScreenRecorderErrorCode.kFeatureUnsupported;
                default:
                    return ScreenRecorderErrorCode.kUnknown;
            }
        }
    }
}
#endif
