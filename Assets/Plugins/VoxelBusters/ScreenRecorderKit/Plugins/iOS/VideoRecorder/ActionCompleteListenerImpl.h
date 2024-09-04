//
//  ActionCompleteListenerImpl.h
//  UnityFramework
//
//  Created by Ayyappa J on 30/09/22.
//

#import <Foundation/Foundation.h>
#import <IActionCompleteListener.h>

NS_ASSUME_NONNULL_BEGIN
typedef void (^ SuccessCallback)();
typedef void (^ FailureCallback)(NSError*);

@interface ActionCompleteListenerImpl : NSObject<IActionCompleteListener>
-(id) initWithCallbacks:(SuccessCallback) onSuccess withFailureCallback:(FailureCallback) onFailure;
@end

NS_ASSUME_NONNULL_END
