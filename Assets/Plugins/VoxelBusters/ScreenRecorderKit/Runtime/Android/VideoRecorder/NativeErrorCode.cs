#if UNITY_ANDROID
namespace VoxelBusters.ScreenRecorderKit.VideoRecorderCore.Android
{
    public class NativeErrorCode
    {
        public const string UNKNOWN                         = "UNKNOWN";
        public const string API_UNAVAILABLE                 = "API_UNAVAILABLE";
        public const string RECORDING_IN_PROGRESS           = "RECORDING_IN_PROGRESS";
        public const string PERMISSION_UNAVAILABLE          = "PERMISSION_UNAVAILABLE";
        public const string ACTIVE_RECORDING_UNAVAILABLE    = "ACTIVE_RECORDING_UNAVAILABLE";
        public const string FEATURE_UNAVAILABLE             = "FEATURE_UNAVAILABLE";
    }
}
#endif
