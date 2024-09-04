#if UNITY_ANDROID
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using VoxelBusters.CoreLibrary;
using VoxelBusters.CoreLibrary.NativePlugins.Android;
using VoxelBusters.ScreenRecorderKit.VideoRecorderCore.Internal;
using System;

namespace VoxelBusters.ScreenRecorderKit.VideoRecorderCore.Android
{
	public partial class VideoRecorderInterface : NativeVideoRecorderInterfaceBase
	{
        #region Fields

        private bool                            m_allowControllingAudio;

        private Dictionary<AudioSource, float> collection = new Dictionary<AudioSource, float>();
        
        #endregion

        #region Constructors

        public VideoRecorderInterface(VideoRecorderSettings settings, VideoRecorderRuntimeSettings runtimeSettings)
		{
            try
            {
                m_allowControllingAudio = settings.AndroidProperties.AllowControllingAudio;
                var classObj = new AndroidJavaClass(Native.Class.NAME);
                Plugin = classObj.CallStatic<AndroidJavaObject>("getInstance", NativeUnityPluginUtility.GetContext().NativeObject, new NativeVideoRecorderSettings(settings, runtimeSettings).NativeObject);
            } catch(Exception e)
            {
                Debug.LogError("VideoRecorderInterface Exception Message : " + e.Message);
                Debug.LogError("VideoRecorderInterface Exception : " + e.StackTrace);
            }

        }

        #endregion

        #region Base class implementation

        protected override bool GetIsAvailableInternal() => true;

        public override bool CanRecord()
        {
            return Plugin.Call<bool>(Native.Methods.CAN_RECORD);
        }

        public override bool IsRecording()
        {
            return Plugin.Call<bool>(Native.Methods.IS_RECORDING);
        }

        public override bool IsRecordingAvailable()
        {
            return Plugin.Call<bool>(Native.Methods.IS_RECORDING_AVAILABLE);
        }

        public override void PrepareRecording(CompletionCallback callback)
        {
            Plugin.Call(Native.Methods.PREPARE_RECORDING, new NativeActionCompleteListener(callback));
        }

        public override void StartRecording(CompletionCallback callback)
        {
            SurrogateCoroutine.StartCoroutine(StartRecordingInternal(callback));
        }

        public override void PauseRecording(CompletionCallback callback)
        {
            Plugin.Call(Native.Methods.PAUSE_RECORDING, new NativeActionCompleteListener(callback));
        }


        public override void ResumeRecording(CompletionCallback callback)
        {
            Plugin.Call(Native.Methods.RESUME_RECORDING, new NativeActionCompleteListener(callback));
        }

        private IEnumerator StartRecordingInternal(CompletionCallback callback)
        {
            if (!IsRecording())
            {
                if (m_allowControllingAudio)
                    PauseAudio();
            }

            yield return new WaitForEndOfFrame();

            Plugin.Call(Native.Methods.START_RECORDING, new NativeActionCompleteListener(callback));
        }

        public override void StopRecording(CompletionCallback callback)
        {
            SurrogateCoroutine.StartCoroutine(StopRecordingInternal(callback));
        }

        private IEnumerator StopRecordingInternal(CompletionCallback callback)
        {
            if (IsRecording())
            {
                if (m_allowControllingAudio)
                    PauseAudio();
            }

            yield return new WaitForEndOfFrame();

            Plugin.Call(Native.Methods.STOP_RECORDING, new NativeActionCompleteListener(callback));
        }

        public override void DiscardRecording(CompletionCallback callback)
        {
            Plugin.Call(Native.Methods.DISCARD_RECORDING, new NativeActionCompleteListener(callback));
        }

        public override void OpenRecording(CompletionCallback callback)
        {
            Plugin.Call(Native.Methods.OPEN_RECORDING, new NativeActionCompleteListener(callback));
        }

        public override void SaveRecording(string filename, CompletionCallback<ScreenRecorderSaveRecordingResult> callback)
        {
            Plugin.Call(Native.Methods.SAVE_RECORDING, filename, new NativeSaveRecordingListener(callback));
        }

        public override void ShareRecording(string text, string subject, CompletionCallback callback)
        {
            Plugin.Call(Native.Methods.SHARE_RECORDING, text, subject, new NativeActionCompleteListener(callback));
        }

        public override void SetRecordingStateChangeListener(IRecorderStateChangeListener callback)
        {
            Plugin.Call(Native.Methods.SET_STATE_CHANGE_LISTENER, new NativeRecorderStateChangeListener()
            {
                onPrepare = () =>
                {
                    CallbackDispatcher.InvokeOnMainThread(() => callback.OnPrepare()); 
                },
                onRecord = () =>
                {
                    CallbackDispatcher.InvokeOnMainThread(() =>
                    {
                        if (m_allowControllingAudio)
                        {
                            ResumeAudio();
                        }

                        callback.OnRecord();
                    });
                    
                },
                onPause = () =>
                {
                    CallbackDispatcher.InvokeOnMainThread(() => callback.OnPause());
                },
                onStop = () =>
                {
                    CallbackDispatcher.InvokeOnMainThread(() =>
                    {
                        if (m_allowControllingAudio)
                        {
                            ResumeAudio();
                        }

                        callback.OnStop();
                    });
                    
                },
                onInvalid = () =>
                {
                    CallbackDispatcher.InvokeOnMainThread(() =>
                    {
                        callback.OnInvalid();
                    });
                }
            });
        }

        public override void SetOnRecordingAvailable(SuccessCallback<ScreenRecorderRecordingAvailableResult> callback)
        {
            Plugin.Call(Native.Methods.SET_AVAILABILITY_LISTENER, new NativeRecordingAvailabilityListener()
            {
                onAvailable = (path) =>
                {
                    var result = new VideoRecorderRecordingAvailableResult(path);
                    CallbackDispatcher.InvokeOnMainThread(callback, result);
                }
            });
        }

        public override void Flush()
        {
            Plugin.Call(Native.Methods.FLUSH);
        }

        #endregion

        #region Helpers

        private void PauseAudio()
        {
            AudioSource[] audioArray = GameObject.FindObjectsOfType<AudioSource>();
            collection.Clear();
            foreach (AudioSource each in audioArray)
            {
                if (each.isPlaying)
                {
                    //Debug.Log("Stop audio : " + each.name + " Time : " + each.time);
                    collection.Add(each, each.time);
                    each.Stop();
                }
            }
        }

        private void ResumeAudio()
        {
            SurrogateCoroutine.StartCoroutine(ResumeAudioInternal());
        }

        private IEnumerator ResumeAudioInternal()
        {
            yield return new WaitForSeconds(0.1f);

            foreach (AudioSource each in collection.Keys)
            {
                each.time = collection[each];
                each.Play();
                //Debug.Log("Resume audio : " + each.name + " Time : " + each.time + " Mute ? : " + each.mute + " Is Playing : " + each.isPlaying);
            }
            collection.Clear();
        }

        #endregion
    }
}
#endif
