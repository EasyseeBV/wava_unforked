//
//  ActionCompleteListenerWrapper.h
//  Unity-iPhone
//
//  Created by Ayyappa J on 06/10/22.
//

#import <Foundation/Foundation.h>
#import "IActionCompleteListener.h"
#import "NPListenerWrapperBase.h"
#import "NPKit.h"

NS_ASSUME_NONNULL_BEGIN

typedef void (*ActionCompleteListenerOnSuccessCallback)(void* listener);
typedef void (*ActionCompleteListenerOnFailureCallback)(void* listener, NPError error);

@interface ActionCompleteListenerWrapper : NPListenerWrapperBase<IActionCompleteListener>
@property (nonatomic, assign) ActionCompleteListenerOnSuccessCallback onSuccessCallback;
@property (nonatomic, assign) ActionCompleteListenerOnFailureCallback onFailureCallback;

@end

NS_ASSUME_NONNULL_END
