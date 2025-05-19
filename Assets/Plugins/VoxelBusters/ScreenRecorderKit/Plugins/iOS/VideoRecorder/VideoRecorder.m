//
//  VideoRecorder.m
//  UnityFramework
//
//  Created by Ayyappa J on 26/09/22.
//

#import "VideoRecorder.h"
#import <ReplayKit/ReplayKit.h>
#import <NSError+ScreenRecorderKit.h>
#import <ActionCompleteListenerImpl.h>

#import <AVFoundation/AVFoundation.h>
#import <AVKit/AVKit.h>
#import <Photos/Photos.h>
#import "UnityAppController.h"

typedef void(^CaptureHandler)(CMSampleBufferRef  _Nonnull sampleBuffer, RPSampleBufferType bufferType, NSError * _Nullable error);
CaptureHandler captureHandler;
typedef void(^CallbackHandler)(NSError* _Nullable error);

@interface VideoRecorder ()
@property (nonatomic) RecordingState recordingState;
@property NSString* recordingPath;
@property (retain, nonnull) VideoRecorderSettings* settings;
@property (retain, nonnull) id<IRecordingAvailabilityListener> availabilityListener;
@property (retain, nonnull) id<IRecorderStateChangeListener> stateChangeListener;
@property NSMutableArray* prepareListeners;



@property BOOL initialisedWriter;

@property BOOL videoDataStarted;
@property BOOL audioDataStarted;
@property BOOL micDataStarted;

@property (atomic, retain) AVAssetWriter        *writer;
@property (atomic, retain) AVAssetWriterInput    *video;
@property (atomic, retain) AVAssetWriterInput    *audio;
@property (atomic, retain) AVAssetWriterInput    *mic;

@property (atomic, retain) AVCaptureSession *session;
@property (atomic, retain) AVCaptureAudioDataOutput *micCaptureOutput;
@property (atomic, retain) AVCaptureDeviceInput *micCaptureInput;

@property(nonatomic, retain)    AVPlayerViewController        *moviePlayerVC;
@property (nonatomic)         dispatch_queue_t              sessionQueue;

@property (nonatomic, retain) UIButton* uiButton;

@property (nonatomic, weak) UIWindow *cachedMainWindow;

@property CMTime elapsedTimeOffset;
@property CMTime previousVideoPts;
@property CMTime previousAudioPts;
@property CMTime previousMicrophonePts;
@property BOOL detectElapsedTime;

@end

@implementation VideoRecorder
@synthesize sessionQueue;
-(id) initWithSettings:(VideoRecorderSettings*) settings
{
    self = [super init];
    _settings = settings;
    
    _prepareListeners = [[NSMutableArray alloc] init];
    self.sessionQueue = dispatch_queue_create("Screen Recorder Kit Session Queue", DISPATCH_QUEUE_SERIAL);
    
    captureHandler = nil;
    
    return self;
}

- (BOOL)canRecord {
    
    if([self isRecordingOrPaused])
    {
        return FALSE;
    }
    
    return [self isRecordingApiAvailable];
}

- (BOOL) isRecordingApiAvailable {
    if (@available(iOS 11.0, *))
        return ([RPScreenRecorder class] != nil) && [RPScreenRecorder sharedRecorder].isAvailable;
    else
        return FALSE;
}


- (BOOL)isRecording {
    return _recordingState == Record;
}

- (BOOL) isRecordingOrPaused {
    return [self isRecording] || _recordingState == Pause;
}

- (BOOL)isRecordingAvailable {
    return _recordingPath != NULL;
}

- (void) setRecordingState:(RecordingState)recordingState
{
    if(_stateChangeListener != nil)
    {
        switch (recordingState) {
            case Invalid:
                [_stateChangeListener onInvalid];
                break;
            case Prepare:
                [_stateChangeListener onPrepare];
                break;
            case Record:
                [_stateChangeListener onRecord];
                break;
            case Pause:
                [_stateChangeListener onPause];
                break;
            case Stop:
                [_stateChangeListener onStop];
                break;
            default:
                break;
        }
    }
    
    _recordingState = recordingState;
}


- (void)setRecordingAvailabilityListener:(nonnull id<IRecordingAvailabilityListener>)listener {
    _availabilityListener = listener;
}


- (void)setRecorderStateChangeListener:(nonnull id<IRecorderStateChangeListener>)listener {
    _stateChangeListener = listener;
}


- (void)prepareRecording:(nonnull id<IActionCompleteListener>) listener {
    
    if(![self isRecordingApiAvailable])
    {
        [self reportError:[NSError apiUnavailable] forwardTo: listener];
        return;
    }
    
    if([self isRecordingOrPaused])
    {
        [self reportError:[NSError recorderBusyRecording] forwardTo: listener];
        return;
    }
    
    [self prepareRecordingInternal: listener];
}

- (void)startRecording:(nonnull id<IActionCompleteListener>)listener {
    
    if(_recordingState == Invalid)
    {
        [self prepareRecording:[[ActionCompleteListenerImpl alloc] initWithCallbacks:^{
            [self startRecordingInternal: listener];
        } withFailureCallback:^(NSError * _Nonnull error) {
            [self reportError:error forwardTo: listener];
        }]];
    }
    else if([self isRecordingOrPaused])
    {
        [self reportError:[NSError recorderBusyRecording] forwardTo: listener];
    }
    else
    {
        [self startRecordingInternal: listener];
    }
}

- (void)pauseRecording:(_Nullable id<IActionCompleteListener>)listener {
    if(listener != nil)
    {
        if(![self isRecordingOrPaused])
        {
            [self reportError:[NSError activeRecordingUnavailable] forwardTo: listener];
            return;
        }
    }
    
    [self setRecordingState:Pause];
        
    if(listener != NULL)
    {
        [listener onSuccess];
    }
}


- (void)resumeRecording:(nonnull id<IActionCompleteListener>)listener {
    if(listener != nil)
    {
        if(![self isRecordingOrPaused])
        {
            [self reportError:[NSError activeRecordingUnavailable] forwardTo: listener];
            return;
        }
    }
    
    [self setRecordingState:Record];
    _detectElapsedTime = TRUE;
    
    if(listener != NULL)
    {
        [listener onSuccess];
    }
}


- (void)stopRecording:(nonnull id<IActionCompleteListener>)listener {
    if(![self isRecordingOrPaused])
    {
        [self reportError:[NSError activeRecordingUnavailable] forwardTo: listener];
        return;
    }
    
    __weak NSString* weakRecordingPath = _recordingPath;

    [self.session stopRunning];
    [[RPScreenRecorder sharedRecorder] stopCaptureWithHandler:^(NSError * _Nullable error) {
        
        if(error != NULL)
        {
            NSLog(@"Failed to stop capture : %@ ", error);
            [self reportError:[NSError unknown:error.localizedDescription] forwardTo: listener];
        }
        else
        {
            [self setRecordingState:Stop];
            [listener onSuccess];
        }
        
        [self->_video markAsFinished];
        [self->_audio markAsFinished];
        
        if(self->_settings.enableMicrophone)
            [self->_mic markAsFinished];
        
        if(self->_writer != nil && (self->_writer.status == AVAssetWriterStatusWriting))
        {
            [self->_writer finishWritingWithCompletionHandler:^{
                NSLog(@"Finished stopping recording!");
                int status = (int)self->_writer.status;
                
                if (status == AVAssetWriterStatusFailed)
                {
                    NSLog(@"Error : Writer status =  AVAssetWriterStatusFailed : %@ %@", self->_writer.error.localizedFailureReason, self->_writer.error.localizedRecoverySuggestion);
                    NSError *unknownError = [NSError unknown:self->_writer.error.localizedFailureReason];
                    
                    [self cleanup: unknownError];
                    if(error == NULL)
                    {
                        [self reportError:unknownError forwardTo: listener];//"Failed stopping recording with status : AVAssetWriterStatusFailed"
                    }
                }
                else
                {
                    NSLog(@"File path weak : %@", weakRecordingPath);
                    long fileSize = [[[NSFileManager defaultManager] attributesOfItemAtPath:weakRecordingPath error:nil] fileSize];
                    if(fileSize > 0)
                    {
                        [self->_availabilityListener onAvailable:weakRecordingPath];
                    }
                    else
                    {
                        if(error == NULL)
                        {
                            NSLog(@"Recorded file size is empty!");
                            [self reportError:[NSError unknown:nil] forwardTo: listener];
                        }
                    }
                }
                self->_videoDataStarted   = FALSE;
            }];
        }
        [self cleanup: nil];
    }];
    
    [[NSNotificationCenter defaultCenter] removeObserver:self name:UIApplicationDidEnterBackgroundNotification object:nil];
    [[NSNotificationCenter defaultCenter] removeObserver:self name:AVCaptureSessionRuntimeErrorNotification object:nil];
}

- (void)discardRecording:(nonnull id<IActionCompleteListener>)listener {
    [self stopRecording:[[ActionCompleteListenerImpl alloc] initWithCallbacks:^{
        [self flush];
        [listener onSuccess];
    } withFailureCallback:^(NSError * _Nonnull error) {
        [self flush];
        [listener onFailure:error];
    }]];
}


- (void)openRecording:(nonnull id<IActionCompleteListener>)listener {
    NSString* filePath = _recordingPath;
    
    if(filePath == nil)
    {
        [listener onFailure:[NSError activeRecordingUnavailable]];
        return;
    }

    NSURL * url = [NSURL URLWithString:[@"file://" stringByAppendingString:filePath]];

    // Stop playing video
    if (_moviePlayerVC != nil) {
        [[_moviePlayerVC player] pause];
        _moviePlayerVC = nil;
    }

    _moviePlayerVC = [[AVPlayerViewController alloc] init];
    AVPlayer *player = [[AVPlayer alloc] initWithURL:url];

    _moviePlayerVC.player = player;
    [player play];

    UnityPause(TRUE);
    __weak id  _weakSelf = self;
    [UnityGetGLViewController() presentViewController:_moviePlayerVC animated:TRUE completion:^ {
        [_weakSelf checkVideoPlayerStatus];
        [[NSNotificationCenter defaultCenter] addObserver:_weakSelf selector:@selector(onVideoPlayerStopped:) name:AVPlayerItemDidPlayToEndTimeNotification object:nil];
        [[NSNotificationCenter defaultCenter] addObserver:_weakSelf selector:@selector(onVideoPlayerStopped:) name:AVPlayerItemFailedToPlayToEndTimeNotification object:nil];
    }];
    
    [listener onSuccess];
}


- (void)shareRecording:(NSString*) title withMessage:(NSString*) message withListener:(nonnull id<IActionCompleteListener>)listener {
    NSString* path = _recordingPath;
    
    if(path == nil)
    {
        [listener onFailure:[NSError activeRecordingUnavailable]];
        return;
    }
    
    NSURL   *url            = [NSURL fileURLWithPath:path];
    NSArray *activityItems  = [NSArray arrayWithObjects:url, message, nil, nil];
    
    UIActivityViewController *controller = [[UIActivityViewController alloc] initWithActivityItems:activityItems applicationActivities:nil];
    
    if(title != nil) { //gmail just copies the body and don't care about subject - This is something out of hands and issue of gmail on iOS
        [controller setValue:title forKey:@"Subject"];
    }

    __weak UIViewController *vc = GetAppController().rootViewController;
    
    dispatch_async(dispatch_get_main_queue(), ^{
        
        UnityPause(TRUE);
        
        [controller setCompletionWithItemsHandler:^(UIActivityType _Nullable activityType, BOOL completed,
                                              NSArray * _Nullable returnedItems,
                                              NSError * _Nullable activityError) {

            UnityPause(FALSE);
            [listener onSuccess];
        }];
        

        if (UI_USER_INTERFACE_IDIOM() != UIUserInterfaceIdiomPhone) {
            controller.modalPresentationStyle = UIModalPresentationPopover;
            controller.popoverPresentationController.sourceView = vc.view;
            CGRect  viewFrame   = [UnityGetGLView() frame];
            CGPoint position  = CGPointMake(CGRectGetMidX(viewFrame), CGRectGetMidY(viewFrame));
            controller.popoverPresentationController.sourceRect =  CGRectMake(position.x, position.y, 1, 1);
        }else {
            controller.modalPresentationStyle = UIModalPresentationFullScreen;
        }
        
        [vc presentViewController:controller animated:YES completion:nil];
        
    });
}

- (void)saveRecording:(nonnull id<ISaveRecordingListener>)listener {
    NSString* path = _recordingPath;
    
    if(path == nil)
    {
        [listener onFailure:[NSError activeRecordingUnavailable]];
        return;
    }
    
    dispatch_after(dispatch_time(DISPATCH_TIME_NOW, (int64_t)(1 * NSEC_PER_SEC)),
    dispatch_get_main_queue(), ^{
        __block VideoRecorder *instance = self;
        [PHPhotoLibrary requestAuthorizationForAccessLevel:PHAccessLevelAddOnly handler:^(PHAuthorizationStatus status) {
            switch (status) {
                case PHAuthorizationStatusAuthorized: {
                    //__block NSString* assetPath;
                    [[PHPhotoLibrary sharedPhotoLibrary] performChanges:^{
                        PHAssetChangeRequest *request = [PHAssetChangeRequest creationRequestForAssetFromVideoAtFileURL:[NSURL fileURLWithPath:path]];
                        //assetPath = [[request placeholderForCreatedAsset] localIdentifier];
                    } completionHandler:^(BOOL success, NSError * _Nullable error) {
                        if (error) {
                            NSLog(@"Error : %@",error);
                            // Lets try without photos library.
                            [instance trySavingPreviewWithOutPhotosLibrary:path withListener:listener];
                        }
                        else
                        {
                            //NSLog(@"Saved photo asset path : %@",assetPath);
                            NSLog(@"Saved video");
                            [listener onSuccess:path];
                        }
                    }];
                    break;
                }
                case PHAuthorizationStatusRestricted:
                case PHAuthorizationStatusDenied:
                {
                    [listener onFailure:[NSError storagePermissionUnavailable]];
                    break;
                }
                default:
                    break;
            }
        }];
    });
}

- (void)flush
{
    NSFileManager *fileManager = [NSFileManager defaultManager];
    NSString *filePath = _recordingPath;
    BOOL success  = TRUE;
    if(filePath != NULL)
    {
       NSError *error;
       success = [fileManager removeItemAtPath:filePath error:&error];
       if (!success)
       {
           NSLog(@"Failed deleting the file : %@ ",[error localizedDescription]);
       }
    }
    // Setting to null as we don't need this file anymore
    _recordingPath = NULL;
}


- (void) prepareRecordingInternal :(id<IActionCompleteListener>) listener
{
    [self setMainWindow];

    [_prepareListeners addObject:listener];

    if([_prepareListeners count] == 1) //Only request once
    {
        //1. Delete recording if any exists
        [self flush];
        
        CallbackHandler callback = ^(NSError *error) {
            [self notifyPrepareRecordingListeners:error];
        };
        
        //2. Check if microphone is required. If so request for it.
        if(_settings.enableMicrophone)
        {
            NSLog(@"Enable Microphone...");
            AVAudioSession *audioSession = [AVAudioSession sharedInstance];
            [audioSession requestRecordPermission:^(BOOL granted) {
                NSLog(@"Enable Microphone... %d", granted ? 1 : 0 );
                if(!granted)
                {
                    callback([NSError microphonePermissionUnavailable]);
                }
                else
                {
                    [self setupRecordingHandlersInternal: callback];
                }
            }];
        }
        else
        {
            [self setupRecordingHandlersInternal: callback];
        }
    }
}

- (void) startRecordingInternal:(id<IActionCompleteListener>) listener
{
    _elapsedTimeOffset = CMTimeMake(0, 1);
    
        // Register for pause events
    [[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(applicationDidEnterBackground:) name:UIApplicationDidEnterBackgroundNotification object:nil];
    [[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(captureError:) name:AVCaptureSessionRuntimeErrorNotification object:nil];
    [self setRecordingState:Record];
    
    NSLog(@"startRecordingInternal : %ld", _recordingState);
    
    if(listener != NULL)
    {
        [listener onSuccess];
    }
}


- (void) setupRecordingHandlersInternal : (CallbackHandler) callback
{
    RPScreenRecorder *recorder = [RPScreenRecorder sharedRecorder];
    
    // Due to bug in Replaykit, don't use replay kit's capture for recording mic data. So forcefully setting to FALSE.
    recorder.microphoneEnabled = FALSE;
    
    // Set flags
    _initialisedWriter  = FALSE;
        
    dispatch_async(self.sessionQueue, ^{
        
        // Add capture session if microphone is required
        if(self->_settings.enableMicrophone)
        {
            [self createCaptureSession];
            [self.session startRunning];
        }
        
        // Create and setup Asset Writer
        [self setupAssetWriter];
        NSLog(@"Setting up asset writer finished");
    });
        
    if (@available(iOS 11.0, *))
    {
        
        if(captureHandler == nil)
        {
            captureHandler = ^(CMSampleBufferRef  _Nonnull sampleBuffer, RPSampleBufferType bufferType, NSError * _Nullable error)
            {                
                if(self.writer == nil)
                    return;
                
                if(self->_recordingState == Pause)
                    return;
                
                if(!self->_initialisedWriter && self->_recordingState == Record)
                {
                    self->_initialisedWriter = TRUE;
                    dispatch_async(self.sessionQueue, ^{
                        [self.writer startWriting];
                    });
                }
                
                if (error != nil)
                {
                    NSLog(@"Sample Buffer Type : %d", (int)bufferType);
                    NSLog(@"Writer Status : %d", (int)self->_writer.status);
                    NSLog(@"Error  : %@", error);
                    return;
                }
                

                if (CMSampleBufferDataIsReady(sampleBuffer))
                {
                    if (self.writer.status != AVAssetWriterStatusWriting)
                        return;
                    
                    CMTime _currentPts = CMSampleBufferGetPresentationTimeStamp(sampleBuffer);
                    
                    if(self->_detectElapsedTime)
                    {
                        self->_elapsedTimeOffset = CMTimeAdd(self->_elapsedTimeOffset, CMTimeSubtract(_currentPts, self->_previousVideoPts));//Add previous time offset if exists
                        self->_detectElapsedTime = FALSE;
                        NSLog(@"Detected Elapsed Time : %lld", self->_elapsedTimeOffset.value/self->_elapsedTimeOffset.timescale);
                    }
                    
                    if (self->_writer.status == AVAssetWriterStatusFailed)
                    {
                        NSLog(@"Error : Writer status =  AVAssetWriterStatusFailed : %@ %@", self->_writer.error.localizedFailureReason, self->_writer.error.localizedRecoverySuggestion);
                        [self cleanup:[NSError unknown: self->_writer.error.localizedFailureReason]];
                        return;
                    }
                    
                    CMSampleBufferRef updatedSampleBuffer = [self getAdjustedSampleBuffer: sampleBuffer withOffset:self->_elapsedTimeOffset];
                    
                    _currentPts = CMSampleBufferGetPresentationTimeStamp(updatedSampleBuffer);
                    CMTime duration = CMSampleBufferGetDuration(updatedSampleBuffer);

                    switch (bufferType)
                    {
                        case RPSampleBufferTypeVideo:
                            
                            if(!self->_videoDataStarted && self->_initialisedWriter)
                            {
                                self->_videoDataStarted = TRUE;
                                [self->_writer startSessionAtSourceTime:CMSampleBufferGetPresentationTimeStamp(updatedSampleBuffer)];
                            }
                            
                            if(self->_video.isReadyForMoreMediaData)
                            {
                                _previousVideoPts = duration.value > 0 ? CMTimeAdd(_currentPts, duration) : _currentPts; //If dur is zero and we add it becomes invalid value after addition. May be duration has set timescale to zero for invalid values leading to invalid value after CMTimeAdd.
                                NSLog(@"Video Presentation time : %lld", self->_previousVideoPts.value/self->_previousVideoPts.timescale);
                                [self->_video appendSampleBuffer: updatedSampleBuffer];
                                if(sampleBuffer != updatedSampleBuffer)
                                    CFRelease(updatedSampleBuffer);
                            }
                            break;
                        case RPSampleBufferTypeAudioApp:
                            
                            if(self->_audio.isReadyForMoreMediaData && self->_videoDataStarted)
                            {
                                self->_previousAudioPts = duration.value > 0 ? CMTimeAdd(_currentPts, duration) : _currentPts;
                                [self->_audio appendSampleBuffer:updatedSampleBuffer];
                                if(sampleBuffer != updatedSampleBuffer)
                                    CFRelease(updatedSampleBuffer);
                            }
                            break;
                            
                        case RPSampleBufferTypeAudioMic:
                            
                            if(self->_mic.isReadyForMoreMediaData && self->_videoDataStarted)
                            {
                                self->_previousMicrophonePts = duration.value > 0 ? CMTimeAdd(_currentPts, duration) : _currentPts;
                                [self->_mic appendSampleBuffer:updatedSampleBuffer];
                                
                                if(sampleBuffer != updatedSampleBuffer)
                                    CFRelease(updatedSampleBuffer);
                            }
                            break;
                        default:
                            break;
                    }
                }
            };
        }
        
        
        [recorder startCaptureWithHandler:captureHandler completionHandler:^(NSError * _Nullable error)
         {
             if(error == nil)
             {
                 NSLog(@"Screen capturing successfully started!");
                 [self setRecordingState:ReadyForRecord];
                 callback(nil);
             }
             else
             {
                 NSLog(@"Screen capturing start failed with error code : %ld  Description  : %@   ", (long)error.code, error.localizedDescription);
                 
                 NSError *recordingError = (error.code == RPRecordingErrorUserDeclined) ? [NSError screenRecordingPermissionUnavailable] : [NSError unknown: error.localizedDescription];
                 
                 [self cleanup : recordingError];
                 callback(recordingError);
             }
             
         }];
    }
    else
    {
        // Fallback on earlier versions
        NSLog(@"[ReplayKit] : This plugin supports only from iOS 11 devices");
        callback([NSError apiUnavailable]);
    }
}

-(CMSampleBufferRef) getAdjustedSampleBuffer:(CMSampleBufferRef) sampleBuffer withOffset:(CMTime) offset
{
    /*if(offset.value > 0)
    {
        return [self adjustSampleBufferTimingInfo:sampleBuffer by:offset];
    }*/
    
    return  sampleBuffer;
}

- (void) notifyPrepareRecordingListeners: (NSError*) error
{
    for (id<IActionCompleteListener> eachListener in _prepareListeners) {
        if(eachListener != nil)
        {
            if(error == nil)
                [eachListener onSuccess];
            else
                [eachListener onFailure: error];
        }
    }
    
    [_prepareListeners removeAllObjects];
}


#pragma mark - Microphone Capture Setup

- (void) createCaptureSession
{
    // Set this to avoid stuttering
    [[AVAudioSession sharedInstance] setCategory:AVAudioSessionCategoryPlayAndRecord
                                     withOptions:/*AVAudioSessionCategoryOptionMixWithOthers | */AVAudioSessionCategoryOptionDefaultToSpeaker
                                           error:nil];
    
    self.session = [[AVCaptureSession alloc] init];
    
    // Get microphone capture device
    AVCaptureDevice *captureDevice = [self getCaptureDevice:AVCaptureDeviceTypeBuiltInMicrophone];
    
    self.micCaptureInput    = [self createCaptureDeviceInput: captureDevice];
    self.micCaptureOutput   = [self createCaptureDeviceOutput];
    
    // Set the delegate
    [self.micCaptureOutput setSampleBufferDelegate:self queue:dispatch_get_main_queue()];
    
    
    if ([self.session canAddInput:self.micCaptureInput])
    {
        [self.session addInput:self.micCaptureInput];
    }
    
    if ([self.session canAddOutput:self.micCaptureOutput])
    {
        [self.session addOutput:self.micCaptureOutput];
    }
}

- (AVCaptureDevice*) getCaptureDevice:(AVCaptureDeviceType)type
{
    AVCaptureDeviceDiscoverySession *discoverySession = [AVCaptureDeviceDiscoverySession discoverySessionWithDeviceTypes:@[type]
                                                                                                               mediaType:AVMediaTypeAudio
                                                                                                                position:AVCaptureDevicePositionUnspecified];
    NSArray *devices = discoverySession.devices;
    for (AVCaptureDevice *device in devices)
    {
        if (device.deviceType == type)
        {
            return device;
        }
    }
    
    return NULL;
}

- (AVCaptureDeviceInput*) createCaptureDeviceInput:(AVCaptureDevice*) device
{
    NSError *error = nil;
    AVCaptureDeviceInput *input = [[AVCaptureDeviceInput alloc] initWithDevice:device error:&error];
    if(error != nil)
    {
        NSLog(@"Error creating AVCaptureDeviceInput : %@", error);
        return NULL;
    }
    else
    {
        return input;
    }
}

- (AVCaptureAudioDataOutput*) createCaptureDeviceOutput
{
    AVCaptureAudioDataOutput *output = [[AVCaptureAudioDataOutput alloc] init];
    return output;
}

#pragma mark - AVCaptureAudioDataOutputSampleBufferDelegate Implementation

- (void)captureOutput:(AVCaptureOutput *)captureOutput didOutputSampleBuffer:(CMSampleBufferRef)sampleBuffer fromConnection:(AVCaptureConnection *)connection {
    
    if (self.writer.status != AVAssetWriterStatusWriting || !_videoDataStarted || _recordingState == Pause)
    {
        return;
    }

    if(_initialisedWriter && _mic.isReadyForMoreMediaData)
    {
        [_mic appendSampleBuffer:[self getAdjustedSampleBuffer: sampleBuffer withOffset:_elapsedTimeOffset]];
    }
}



- (void) setupAssetWriter
{
    NSError *writerError;
    NSURL *url = [self recordingURL];
    
    CGFloat contentScaleFactor = [[UIScreen mainScreen] scale];

    int width   = floor([UIScreen mainScreen].bounds.size.width/16) * 16;
    int height  = floor([UIScreen mainScreen].bounds.size.height/16) * 16;
    
    NSLog(@"Width : %d Height : %d %f", width, height, contentScaleFactor);
    
    NSDictionary <NSString *, id> *videoSettings = @{
                                                     AVVideoCodecKey: AVVideoCodecTypeH264,
                                                     AVVideoWidthKey: @(width * contentScaleFactor),
                                                     AVVideoHeightKey: @(height * contentScaleFactor)
//                                                     AVVideoScalingModeKey: AVVideoScalingModeResizeAspectFill
                                                     };
    
    NSDictionary <NSString *, id> *appAudioSettings = @{
                                                        AVFormatIDKey: @(kAudioFormatMPEG4AAC),
                                                        AVNumberOfChannelsKey: @(2),
                                                        AVSampleRateKey: @(44100.0),
                                                        AVEncoderBitRateKey: @(128000)
                                                        };
    
    NSDictionary <NSString *, id> *microphoneSettings = @{
                                                          AVFormatIDKey: @(kAudioFormatMPEG4AAC),
                                                          AVNumberOfChannelsKey: @(2),
                                                          AVSampleRateKey: @(44100.0),
                                                          AVEncoderBitRateKey: @(128000)
                                                          };
    
    
    _recordingPath = [url path];
    
    
    self.writer = [AVAssetWriter assetWriterWithURL:url fileType:AVFileTypeMPEG4 error:&writerError];
    
    NSLog(@"%@",videoSettings);
    self.video  = [AVAssetWriterInput assetWriterInputWithMediaType:AVMediaTypeVideo outputSettings:videoSettings];
    self.audio  = [AVAssetWriterInput assetWriterInputWithMediaType:AVMediaTypeAudio outputSettings:appAudioSettings];
    
    if(_settings.enableMicrophone)
        self.mic    = [AVAssetWriterInput assetWriterInputWithMediaType:AVMediaTypeAudio outputSettings:microphoneSettings];
    
    
    self.video.expectsMediaDataInRealTime   = YES;
    self.audio.expectsMediaDataInRealTime   = YES;

    self.video.transform = CGAffineTransformIdentity;
    NSLog(@"naturalSize %@", NSStringFromCGSize(self.video.naturalSize));
    
    if(_settings.enableMicrophone)
        self.mic.expectsMediaDataInRealTime     = YES;
    
    [self.writer addInput:self.video];
    
    if(_settings.enableMicrophone) //Order Imp
        [self.writer addInput:self.mic];
    
    [self.writer addInput:self.audio];
}

- (CGAffineTransform)assetWriterVideoTransform:(UIInterfaceOrientation) orientation
{
    CGAffineTransform transform;

    switch (orientation) {
        case UIInterfaceOrientationLandscapeRight:
            transform = CGAffineTransformMakeRotation(-M_PI_2);
            break;
        case UIInterfaceOrientationLandscapeLeft:
            transform = CGAffineTransformMakeRotation(M_PI_2);
            break;
        case UIInterfaceOrientationPortraitUpsideDown:
            transform = CGAffineTransformMakeRotation(M_PI);
            break;
        default:
            transform = CGAffineTransformIdentity;
    }
    return transform;
}


- (NSURL *)recordingURL
{
    NSString *basePath = NSSearchPathForDirectoriesInDomains(NSDocumentDirectory, NSUserDomainMask, YES).firstObject;
    NSString *path = [basePath stringByAppendingPathComponent:@"Recordings"];
    
    if (![[NSFileManager defaultManager] fileExistsAtPath:path]) {
        [[NSFileManager defaultManager] createDirectoryAtPath:path withIntermediateDirectories:YES attributes:nil error:nil];
    }
    
    NSString *filename = [NSString stringWithFormat:@"Recording_%.0f.mp4", [NSDate date].timeIntervalSince1970];
    
    NSURL *url = [NSURL fileURLWithPath:[NSString pathWithComponents:@[path, filename]]];
    
//#ifdef DEBUG
    NSLog(@"Recording Output URL: %@", url);
//#endif
    
    return url;
}


- (void) cleanup :(NSError*) writeFailureError
{
    [self setRecordingState:Invalid];
    self.writer = NULL;
    
    if(writeFailureError != nil)
    {
        _recordingPath = NULL;
    }
    
    dispatch_async(dispatch_get_main_queue(), ^{
        [self resetMainWindow];
    });
}


- (void) setMainWindow
{
    UIWindow* window = UnityGetMainWindow();
    _cachedMainWindow = [UIApplication sharedApplication].delegate.window;
    [[[UIApplication sharedApplication] delegate] setWindow:window];
}

- (void) resetMainWindow
{
    if(_cachedMainWindow != nil)
        [[[UIApplication sharedApplication] delegate] setWindow:_cachedMainWindow];
}


- (void) reportError:(NSError*) error forwardTo:(_Nullable id<IActionCompleteListener>) forwardTo
{
    NSLog(@"Error : %@", error);
    if(forwardTo != nil)
    {
        [forwardTo onFailure:error];
    }
    
    //Fire error to common error listener
}

- (void)applicationDidEnterBackground:(id)sender
{
    [self pauseRecording:nil];
}

- (void)captureError:(id)sender
{
    NSLog(@"Info : %@ : ", sender);
}

- (void) checkVideoPlayerStatus
{
    if ((_moviePlayerVC.player.rate != 0) && (_moviePlayerVC.player.error == nil)) {
        [self performSelector:@selector(checkVideoPlayerStatus) withObject:nil afterDelay:0.5];
    } else {
        [self onVideoPlayerStopped : nil];
    }
}

- (void) onVideoPlayerStopped :(NSNotification*) notification
{
    [[NSNotificationCenter defaultCenter] removeObserver:self name:AVPlayerItemDidPlayToEndTimeNotification object:nil];
    [[NSNotificationCenter defaultCenter] removeObserver:self name:AVPlayerItemFailedToPlayToEndTimeNotification object:nil];
    
    [NSObject cancelPreviousPerformRequestsWithTarget:self];

    UnityPause(FALSE);
}

-(void) trySavingPreviewWithOutPhotosLibrary :(NSString*) path withListener:(nonnull id<ISaveRecordingListener>) listener
{
    BOOL compatible = UIVideoAtPathIsCompatibleWithSavedPhotosAlbum(path);
    if (compatible) {
        UISaveVideoAtPathToSavedPhotosAlbum(path, self, @selector(savePreviewFinished:didFinishSavingWithError:contextInfo:), nil);
        [listener onSuccess:path];//TODO
    }
    else
    {
        long fileSize = [[[NSFileManager defaultManager] attributesOfItemAtPath:path error:nil] fileSize];
        NSLog(@"Unable to save video to camera roll! : %ld", fileSize);
        [listener onFailure:[NSError unknown:@"Unable to save video to camera roll"]];
    }
}

- (void)savePreviewFinished:(NSString *)videoPath didFinishSavingWithError:(NSError *)error contextInfo:(void *)contextInfo
{
    NSLog(@"Finished Saving to Gallery! [Error : %@]", error);
    if(error != nil)
    {
        [self reportError:[NSError unknown:error.localizedDescription] forwardTo:nil];
    }
    else
    {
        //TODO : Here update the success callback.
    }
}


- (CMSampleBufferRef) adjustSampleBufferTimingInfo:(CMSampleBufferRef) sampleBuffer by:(CMTime) offset
{
    CFAllocatorRef allocator = kCFAllocatorDefault;
    CMSampleTimingInfo timingInfo = kCMTimingInfoInvalid;
    OSStatus status;

    CMTime currentPts = CMSampleBufferGetPresentationTimeStamp(sampleBuffer);
    CMTime currentDts = CMSampleBufferGetDecodeTimeStamp(sampleBuffer);

    timingInfo.duration = CMSampleBufferGetDuration(sampleBuffer);
    timingInfo.presentationTimeStamp = CMTimeSubtract(currentPts, offset);
    timingInfo.decodeTimeStamp = CMTimeSubtract(currentDts, offset);

    // Create a copy of the sample buffer with updated timing information
    CMSampleBufferRef updatedSampleBuffer;
    status = CMSampleBufferCreateCopyWithNewTiming(allocator, sampleBuffer, 1, &timingInfo, &updatedSampleBuffer);

    if (status == noErr) {
        return updatedSampleBuffer;
    }
    
    //fallback
    return sampleBuffer;
}

@end
