//
//  VideoRecorderBindings.h
//  Unity-iPhone
//
//  Created by Ayyappa J on 05/10/22.
//

#import <Foundation/Foundation.h>
#import "NPKit.h"
#import "RecordingAvailabilityListenerWrapper.h"
#import "ActionCompleteListenerWrapper.h"
#import "SaveRecordingListenerWrapper.h"
#import "VideoRecorderBindingsCommon.h"
#import "RecorderStateChangeListenerWrapper.h"

#define METHOD(ReturnType, Name, ...) METHOD_BASE(ReturnType, VideoRecorder, Name, __VA_ARGS__)

NS_ASSUME_NONNULL_BEGIN

METHOD(void*,   CreateWithSettings,                 void* settings);
METHOD(bool,    CanRecord,                          void* owner);
METHOD(bool,    IsRecordingAvailable,               void* owner);
METHOD(bool,    IsRecording,                        void* owner);
METHOD(bool,    IsRecordingOrPaused,                void* owner);
METHOD(void,    SetRecordingAvailabilityListener,   void* owner, void* listener, RecordingAvailabilityListenerOnAvailableCallback onAvailable);
METHOD(void,    SetRecorderStateChangeListener,     void* owner, void* listener, RecorderStateChangeListenerOnNewStateCallback onInvalid, RecorderStateChangeListenerOnNewStateCallback onRecord, RecorderStateChangeListenerOnNewStateCallback onPrepare, RecorderStateChangeListenerOnNewStateCallback onPause, RecorderStateChangeListenerOnNewStateCallback onStop);
METHOD(void,    PrepareRecording,                   void* owner, void* listener, ActionCompleteListenerOnSuccessCallback onSuccess, ActionCompleteListenerOnFailureCallback onFailure);
METHOD(void,    StartRecording,                     void* owner, void* listener, ActionCompleteListenerOnSuccessCallback onSuccess, ActionCompleteListenerOnFailureCallback onFailure);
METHOD(void,    PauseRecording,                     void* owner, void* listener, ActionCompleteListenerOnSuccessCallback onSuccess, ActionCompleteListenerOnFailureCallback onFailure);
METHOD(void,    ResumeRecording,                    void* owner, void* listener, ActionCompleteListenerOnSuccessCallback onSuccess, ActionCompleteListenerOnFailureCallback onFailure);
METHOD(void,    StopRecording,                      void* owner, void* listener, ActionCompleteListenerOnSuccessCallback onSuccess, ActionCompleteListenerOnFailureCallback onFailure);
METHOD(void,    DiscardRecording,                   void* owner, void* listener, ActionCompleteListenerOnSuccessCallback onSuccess, ActionCompleteListenerOnFailureCallback onFailure);
METHOD(void,    OpenRecording,                      void* owner, void* listener, ActionCompleteListenerOnSuccessCallback onSuccess, ActionCompleteListenerOnFailureCallback onFailure);
METHOD(void,    ShareRecording,                     void* owner, void* listener, ActionCompleteListenerOnSuccessCallback onSuccess, ActionCompleteListenerOnFailureCallback onFailure);
METHOD(void,    SaveRecording,                      void* owner, void* fileName, void* listener, SaveRecordingListenerOnSuccessCallback onSuccess, SaveRecordingListenerOnFailureCallback onFailure);
METHOD(void,    Flush,                              void* owner);


NS_ASSUME_NONNULL_END
