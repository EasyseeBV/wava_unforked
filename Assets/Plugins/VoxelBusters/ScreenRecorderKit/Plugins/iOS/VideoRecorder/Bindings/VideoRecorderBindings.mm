//
//  VideoRecorderBindings.m
//  Unity-iPhone
//
//  Created by Ayyappa J on 05/10/22.
//

#import "VideoRecorder.h"
#import "VideoRecorderBindings.h"

METHOD(void*, CreateWithSettings, void* settings)
{
    return (__bridge_retained void*)[[VideoRecorder alloc] initWithSettings:(__bridge VideoRecorderSettings*) settings];
}

METHOD(bool, CanRecord, void* owner)
{
    VideoRecorder*     recorder  = CAST_TO_OBJC(VideoRecorder, owner);
    return [recorder canRecord];
}

METHOD(bool, IsRecordingAvailable, void* owner)
{
    VideoRecorder*     recorder  = CAST_TO_OBJC(VideoRecorder, owner);
    return [recorder canRecord];
}
METHOD(bool, IsRecording, void* owner)
{
    VideoRecorder*     recorder  = CAST_TO_OBJC(VideoRecorder, owner);
    return [recorder isRecording];
}

METHOD(void, SetRecordingAvailabilityListener,   void* owner, void* listener, RecordingAvailabilityListenerOnAvailableCallback onAvailable)
{
    VideoRecorder*     recorder  = CAST_TO_OBJC(VideoRecorder, owner);
    RecordingAvailabilityListenerWrapper *listenerWrapper = [[RecordingAvailabilityListenerWrapper alloc] initWithTag:listener];
    listenerWrapper.onAvailableCallback = onAvailable;
    [recorder setRecordingAvailabilityListener:listenerWrapper];
}

METHOD(void,    SetRecorderStateChangeListener,             void* owner, void* listener, RecorderStateChangeListenerOnNewStateCallback onInvalid, RecorderStateChangeListenerOnNewStateCallback onRecord, RecorderStateChangeListenerOnNewStateCallback onPrepare, RecorderStateChangeListenerOnNewStateCallback onPause, RecorderStateChangeListenerOnNewStateCallback onStop)
{
    VideoRecorder*     recorder  = CAST_TO_OBJC(VideoRecorder, owner);
    RecorderStateChangeListenerWrapper *listenerWrapper = [[RecorderStateChangeListenerWrapper alloc] initWithTag:listener];
    listenerWrapper.onInvalidCallback = onInvalid;
    listenerWrapper.onPrepareCallback = onPrepare;
    listenerWrapper.onRecordCallback = onRecord;
    listenerWrapper.onPauseCallback = onPause;
    listenerWrapper.onStopCallback = onStop;
    [recorder setRecorderStateChangeListener:listenerWrapper];
}

METHOD(void, PrepareRecording, void* owner, void* listener, ActionCompleteListenerOnSuccessCallback onSuccess, ActionCompleteListenerOnFailureCallback onFailure)
{
    VideoRecorder*     recorder  = CAST_TO_OBJC(VideoRecorder, owner);
    ActionCompleteListenerWrapper *listenerWrapper = [[ActionCompleteListenerWrapper alloc] initWithTag:listener];
    listenerWrapper.onSuccessCallback = onSuccess;
    listenerWrapper.onFailureCallback = onFailure;
    [recorder prepareRecording:listenerWrapper];
}
METHOD(void,    StartRecording, void* owner, void* listener,  ActionCompleteListenerOnSuccessCallback onSuccess, ActionCompleteListenerOnFailureCallback onFailure)
{
    VideoRecorder*     recorder  = CAST_TO_OBJC(VideoRecorder, owner);
    ActionCompleteListenerWrapper *listenerWrapper = [[ActionCompleteListenerWrapper alloc] initWithTag:listener];
    listenerWrapper.onSuccessCallback = onSuccess;
    listenerWrapper.onFailureCallback = onFailure;
    [recorder startRecording:listenerWrapper];
}
METHOD(void, PauseRecording, void* owner, void* listener,  ActionCompleteListenerOnSuccessCallback onSuccess, ActionCompleteListenerOnFailureCallback onFailure)
{
    VideoRecorder*     recorder  = CAST_TO_OBJC(VideoRecorder, owner);
    ActionCompleteListenerWrapper *listenerWrapper = [[ActionCompleteListenerWrapper alloc] initWithTag:listener];
    listenerWrapper.onSuccessCallback = onSuccess;
    listenerWrapper.onFailureCallback = onFailure;
    [recorder startRecording:listenerWrapper];
}
METHOD(void, ResumeRecording, void* owner, void* listener, ActionCompleteListenerOnSuccessCallback onSuccess, ActionCompleteListenerOnFailureCallback onFailure)
{
    VideoRecorder*     recorder  = CAST_TO_OBJC(VideoRecorder, owner);
    ActionCompleteListenerWrapper *listenerWrapper = [[ActionCompleteListenerWrapper alloc] initWithTag:listener];
    listenerWrapper.onSuccessCallback = onSuccess;
    listenerWrapper.onFailureCallback = onFailure;
    [recorder resumeRecording:listenerWrapper];
}
METHOD(void,    StopRecording, void* owner, void* listener, ActionCompleteListenerOnSuccessCallback onSuccess, ActionCompleteListenerOnFailureCallback onFailure)
{
    VideoRecorder*     recorder  = CAST_TO_OBJC(VideoRecorder, owner);
    ActionCompleteListenerWrapper *listenerWrapper = [[ActionCompleteListenerWrapper alloc] initWithTag:listener];
    listenerWrapper.onSuccessCallback = onSuccess;
    listenerWrapper.onFailureCallback = onFailure;
    [recorder stopRecording:listenerWrapper];
}
METHOD(void, DiscardRecording, void* owner, void* listener, ActionCompleteListenerOnSuccessCallback onSuccess, ActionCompleteListenerOnFailureCallback onFailure)
{
    VideoRecorder*     recorder  = CAST_TO_OBJC(VideoRecorder, owner);
    ActionCompleteListenerWrapper *listenerWrapper = [[ActionCompleteListenerWrapper alloc] initWithTag:listener];
    listenerWrapper.onSuccessCallback = onSuccess;
    listenerWrapper.onFailureCallback = onFailure;
    [recorder discardRecording:listenerWrapper];
}
METHOD(void, OpenRecording, void* owner, void* listener, ActionCompleteListenerOnSuccessCallback onSuccess, ActionCompleteListenerOnFailureCallback onFailure)
{
    VideoRecorder*     recorder  = CAST_TO_OBJC(VideoRecorder, owner);
    ActionCompleteListenerWrapper *listenerWrapper = [[ActionCompleteListenerWrapper alloc] initWithTag:listener];
    listenerWrapper.onSuccessCallback = onSuccess;
    listenerWrapper.onFailureCallback = onFailure;
    [recorder openRecording:listenerWrapper];
}
METHOD(void, ShareRecording, void* owner, void* listener, ActionCompleteListenerOnSuccessCallback onSuccess, ActionCompleteListenerOnFailureCallback onFailure)
{
    VideoRecorder*     recorder  = CAST_TO_OBJC(VideoRecorder, owner);
    ActionCompleteListenerWrapper *listenerWrapper = [[ActionCompleteListenerWrapper alloc] initWithTag:listener];
    listenerWrapper.onSuccessCallback = onSuccess;
    listenerWrapper.onFailureCallback = onFailure;
    [recorder shareRecording:listenerWrapper];
}
METHOD(void, SaveRecording, void* owner, void* fileName, void* listener, SaveRecordingListenerOnSuccessCallback onSuccess, SaveRecordingListenerOnFailureCallback onFailure)
{
    VideoRecorder*     recorder  = CAST_TO_OBJC(VideoRecorder, owner);
    SaveRecordingListenerWrapper *listenerWrapper = [[SaveRecordingListenerWrapper alloc] initWithTag:listener];
    listenerWrapper.onSuccessCallback = onSuccess;
    listenerWrapper.onFailureCallback = onFailure;
    [recorder saveRecording:listenerWrapper];
}
METHOD(void, Flush, void* owner)
{
    VideoRecorder*     recorder  = CAST_TO_OBJC(VideoRecorder, owner);
    [recorder flush];
}

