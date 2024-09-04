//
//  RecordingAvailabilityListenerWrapper.h
//  Unity-iPhone
//
//  Created by Ayyappa J on 06/10/22.
//

#import <Foundation/Foundation.h>
#import "NPKit.h"
#import "IRecordingAvailabilityListener.h"
#import "NPListenerWrapperBase.h"

NS_ASSUME_NONNULL_BEGIN

typedef void (*RecordingAvailabilityListenerOnAvailableCallback)(void* listener, void* path);


@interface RecordingAvailabilityListenerWrapper : NPListenerWrapperBase<IRecordingAvailabilityListener>
@property (nonatomic, assign) RecordingAvailabilityListenerOnAvailableCallback onAvailableCallback;
@end

NS_ASSUME_NONNULL_END
