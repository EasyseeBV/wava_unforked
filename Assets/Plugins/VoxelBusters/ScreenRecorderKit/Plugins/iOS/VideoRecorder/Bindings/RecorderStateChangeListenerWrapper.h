//
//  StateChangeListenerWrapper.h
//  UnityFramework
//
//  Created by Ayyappa J on 10/10/22.
//

#import <Foundation/Foundation.h>
#import "NPListenerWrapperBase.h"
#import "IRecorderStateChangeListener.h"
#import "NPKit.h"

NS_ASSUME_NONNULL_BEGIN

typedef void (*RecorderStateChangeListenerOnNewStateCallback)(void* listener);

@interface RecorderStateChangeListenerWrapper : NPListenerWrapperBase<IRecorderStateChangeListener>

@property (nonatomic, assign) RecorderStateChangeListenerOnNewStateCallback onInvalidCallback;
@property (nonatomic, assign) RecorderStateChangeListenerOnNewStateCallback onPrepareCallback;
@property (nonatomic, assign) RecorderStateChangeListenerOnNewStateCallback onRecordCallback;
@property (nonatomic, assign) RecorderStateChangeListenerOnNewStateCallback onPauseCallback;
@property (nonatomic, assign) RecorderStateChangeListenerOnNewStateCallback onStopCallback;

@end

NS_ASSUME_NONNULL_END

