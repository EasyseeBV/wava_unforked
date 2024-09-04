//
//  VideoRecorder.h
//  UnityFramework
//
//  Created by Ayyappa J on 26/09/22.
//

#import <Foundation/Foundation.h>
#import <IActionCompleteListener.h>
#import <ISaveRecordingListener.h>
#import <IRecordingAvailabilityListener.h>
#import <IRecorderStateChangeListener.h>
#import <AVFoundation/AVFoundation.h>
#import <VideoRecorderSettings.h>

NS_ASSUME_NONNULL_BEGIN

@protocol IVideoRecorder<NSObject>

-(BOOL) canRecord;
-(BOOL) isRecording;
-(BOOL) isRecordingAvailable;

-(void) prepareRecording:(id<IActionCompleteListener>) listener;
-(void) startRecording:(id<IActionCompleteListener>) listener;
-(void) pauseRecording:(_Nullable id<IActionCompleteListener>) listener;
-(void) resumeRecording:(id<IActionCompleteListener>) listener;
-(void) stopRecording:(id<IActionCompleteListener>) listener;
-(void) discardRecording:(id<IActionCompleteListener>) listener;
-(void) openRecording:(id<IActionCompleteListener>) listener;
-(void) shareRecording:(id<IActionCompleteListener>) listener;
-(void) saveRecording:(id<ISaveRecordingListener>) listener;
-(void) setRecordingAvailabilityListener:(id<IRecordingAvailabilityListener>) listener;
-(void) setRecorderStateChangeListener:(id<IRecorderStateChangeListener>) listener;
-(void) flush;
@end


typedef NS_ENUM(NSInteger, RecordingState)
{
    Invalid,
    Prepare,
    ReadyForRecord,
    Record,
    Pause,
    Stop
};

@interface VideoRecorder : NSObject <IVideoRecorder, AVCaptureAudioDataOutputSampleBufferDelegate>
-(id) initWithSettings:(VideoRecorderSettings*) settings;
@end

NS_ASSUME_NONNULL_END
