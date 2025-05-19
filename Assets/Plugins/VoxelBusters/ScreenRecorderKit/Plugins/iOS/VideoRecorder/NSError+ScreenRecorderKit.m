//
//  NSError+ScreenRecorderKit.m
//  UnityFramework
//
//  Created by Ayyappa J on 29/09/22.
//

#import "NSError+ScreenRecorderKit.h"

@implementation NSError (ScreenRecorderKit)

+(NSError*) apiUnavailable
{
    return [NSError errorWithDomain:@"VideoRecorder" code:ApiUnavailable userInfo:@{NSLocalizedDescriptionKey : @"Api unavailable error"}];
}
+(NSError*) microphonePermissionUnavailable
{
    return [NSError errorWithDomain:@"VideoRecorder" code:PermissionUnavailable userInfo:@{NSLocalizedDescriptionKey : @"Microphone permission unavailable"}];
}

+(NSError*) screenRecordingPermissionUnavailable
{
    return [NSError errorWithDomain:@"VideoRecorder" code:PermissionUnavailable userInfo:@{NSLocalizedDescriptionKey: @"Screen recording permission unavailable"}];
}

+(NSError*) recorderBusyRecording
{
    return [NSError errorWithDomain:@"VideoRecorder" code:RecordingInProgress userInfo:@{NSLocalizedDescriptionKey : @"Recorder busy recording"}];
}

+(NSError*) activeRecordingUnavailable
{
    return [NSError errorWithDomain:@"VideoRecorder" code:ActiveRecordingUnavailable userInfo:@{NSLocalizedDescriptionKey : @"No active recording available"}];
}

+(NSError*) storagePermissionUnavailable
{
    return [NSError errorWithDomain:@"VideoRecorder" code:PermissionUnavailable userInfo:@{NSLocalizedDescriptionKey : @"Storage permission unavailable"}];
}

+(NSError*) unknown:(nullable NSString*) description
{
    return [NSError errorWithDomain:@"VideoRecorder" code:Unknown userInfo:(description != nil) ? @{NSLocalizedDescriptionKey : description} : @{NSLocalizedDescriptionKey : @"Unknown error"}];
}

@end
