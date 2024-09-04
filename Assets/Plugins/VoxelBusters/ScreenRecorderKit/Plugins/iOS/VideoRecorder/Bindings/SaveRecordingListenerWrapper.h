//
//  SaveRecordingListenerWrapper.h
//  Unity-iPhone
//
//  Created by Ayyappa J on 07/10/22.
//


#import <Foundation/Foundation.h>
#import "ISaveRecordingListener.h"
#import "NPListenerWrapperBase.h"
#import "NPKit.h"

NS_ASSUME_NONNULL_BEGIN

typedef void (*SaveRecordingListenerOnSuccessCallback)(void* listener, void* path);
typedef void (*SaveRecordingListenerOnFailureCallback)(void* listener, NPError error);


@interface SaveRecordingListenerWrapper : NPListenerWrapperBase<ISaveRecordingListener>
@property (nonatomic, assign) SaveRecordingListenerOnSuccessCallback onSuccessCallback;
@property (nonatomic, assign) SaveRecordingListenerOnFailureCallback onFailureCallback;
@end

NS_ASSUME_NONNULL_END



