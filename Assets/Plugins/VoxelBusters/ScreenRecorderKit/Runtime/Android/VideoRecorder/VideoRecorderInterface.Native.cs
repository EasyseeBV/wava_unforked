#if UNITY_ANDROID
using UnityEngine;
using VoxelBusters.ScreenRecorderKit.VideoRecorderCore.Internal;

namespace VoxelBusters.ScreenRecorderKit.VideoRecorderCore.Android
{
	public partial class VideoRecorderInterface
	{
		#region Platform Native Info

		private class Native
		{
			// Handler class name
			internal class Class
			{
				internal const string NAME			= "com.voxelbusters.screenrecorderkit.videorecorder.VideoRecorder";
			}

			// For holding method names
			internal class Methods
			{
				internal const string CAN_RECORD					= "canRecord";
				internal const string IS_RECORDING		            = "isRecording";
				internal const string IS_RECORDING_AVAILABLE		= "isRecordingAvailable";

				internal const string PREPARE_RECORDING				= "prepareRecording";
				internal const string START_RECORDING               = "startRecording";
				internal const string PAUSE_RECORDING				= "pauseRecording";
				internal const string RESUME_RECORDING				= "resumeRecording";
				internal const string STOP_RECORDING                = "stopRecording";

				internal const string OPEN_RECORDING				= "openRecording";
                internal const string DISCARD_RECORDING             = "discardRecording";
				internal const string SAVE_RECORDING             	= "saveRecording";
				internal const string SHARE_RECORDING             	= "shareRecording";
                internal const string SET_RECORDING_UI_VISIBILITY   = "setRecordingUIVisibility";
				internal const string SET_STATE_CHANGE_LISTENER		= "setStateChangeListener";
				internal const string SET_AVAILABILITY_LISTENER		= "setAvailabilityListener";
				internal const string FLUSH							= "flush";
            }
		}

		#endregion

		#region  Native Access Variables

		private AndroidJavaObject  	Plugin
		{
			get;
			set;
		}

        #endregion
    }
}
#endif