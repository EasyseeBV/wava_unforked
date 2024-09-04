//
//  ActionCompleteListenerWrapper.m
//  Unity-iPhone
//
//  Created by Ayyappa J on 06/10/22.
//

#import "ActionCompleteListenerWrapper.h"

@implementation ActionCompleteListenerWrapper


- (void)onSuccess {
    if(_onSuccessCallback != nil)
    {
        _onSuccessCallback(self.tag);
    }
}

- (void)onFailure:(nonnull NSError *)error {
    if(_onFailureCallback != nil)
    {
        _onFailureCallback(self.tag, NPCreateError((int)[error code], [error localizedDescription]));
    }
}


@end
