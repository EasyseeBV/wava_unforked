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
    return [NSError errorWithDomain:@"VideoRecorder" code:ApiUnavailable userInfo:@{@"Error reason": @"Api unavailable error"}];
}
+(NSError*) microphonePermissionUnavailable
{
    return [NSError errorWithDomain:@"VideoRecorder" code:PermissionUnavailable userInfo:@{@"Error reason": @"Microphone permission unavailable"}];
}

+(NSError*) screenRecordingPermissionUnavailable
{
    return [NSError errorWithDomain:@"VideoRecorder" code:PermissionUnavailable userInfo:@{@"Error reason": @"Screen recording permission unavailable"}];
}

+(NSError*) recorderBusyRecording
{
    return [NSError errorWithDomain:@"VideoRecorder" code:RecordingInProgress userInfo:@{@"Error reason": @"Recorder busy recording"}];
}

+(NSError*) activeRecordingUnavailable
{
    return [NSError errorWithDomain:@"VideoRecorder" code:ActiveRecordingUnavailable userInfo:@{@"Error reason": @"No active recording available"}];
}

+(NSError*) storagePermissionUnavailable
{
    return [NSError errorWithDomain:@"VideoRecorder" code:PermissionUnavailable userInfo:@{@"Error reason": @"Storage permission unavailable"}];
}

+(NSError*) unknown:(nullable NSString*) description
{
    return [NSError errorWithDomain:@"VideoRecorder" code:Unknown userInfo:(description != nil) ? @{@"Error reason": description} : @{@"Error reason": @"Unknown error"}];
}

@end
