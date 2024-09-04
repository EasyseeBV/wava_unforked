//
//  SaveRecordingListenerWrapper.m
//  Unity-iPhone
//
//  Created by Ayyappa J on 07/10/22.
//

#import "SaveRecordingListenerWrapper.h"

@implementation SaveRecordingListenerWrapper

- (void)onSuccess:(NSString*) path {
    if(_onSuccessCallback != nil)
    {
        _onSuccessCallback(self.tag, NPCreateCStringFromNSString(path));
    }
}

- (void)onFailure:(nonnull NSError *)error {
    if(_onFailureCallback != nil)
    {
        _onFailureCallback(self.tag, NPCreateError([error code], [error localizedDescription]));
    }
}
@end
