//
//  ActionCompleteListenerImpl.m
//  UnityFramework
//
//  Created by Ayyappa J on 30/09/22.
//

#import "ActionCompleteListenerImpl.h"


@interface ActionCompleteListenerImpl ()
@property SuccessCallback onSuccessCallback;
@property FailureCallback onFailureCallback;
@end

@implementation ActionCompleteListenerImpl
-(id) initWithCallbacks:(SuccessCallback) onSuccess withFailureCallback:(FailureCallback) onFailure
{
    self = [super init];
    _onSuccessCallback = onSuccess;
    _onFailureCallback = onFailure;
    return self;
}

- (void)onFailure:(nonnull NSError *)error {
    if(_onFailureCallback != nil) {
        _onFailureCallback(error);
    }
}

- (void)onSuccess {
    if(_onSuccessCallback != nil) {
        _onSuccessCallback();
    }
}

@end
