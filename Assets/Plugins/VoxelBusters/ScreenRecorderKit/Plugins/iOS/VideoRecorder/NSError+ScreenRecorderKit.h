//
//  NSError+ScreenRecorderKit.h
//  UnityFramework
//
//  Created by Ayyappa J on 29/09/22.
//

#import <Foundation/Foundation.h>

NS_ASSUME_NONNULL_BEGIN

typedef NS_ENUM(NSInteger, RecordingError)
{
    Unknown,
    ApiUnavailable,
    RecordingInProgress,
    ActiveRecordingUnavailable,
    PermissionUnavailable,
    FeatureUnavailable,
    PrepareNotCalled
};


@interface NSError (ScreenRecorderKit)
+(NSError*) apiUnavailable;
+(NSError*) microphonePermissionUnavailable;
+(NSError*) storagePermissionUnavailable;
+(NSError*) recorderBusyRecording;
+(NSError*) activeRecordingUnavailable;
+(NSError*) screenRecordingPermissionUnavailable;
+(NSError*) unknown:(nullable NSString*) description;

@end

NS_ASSUME_NONNULL_END
