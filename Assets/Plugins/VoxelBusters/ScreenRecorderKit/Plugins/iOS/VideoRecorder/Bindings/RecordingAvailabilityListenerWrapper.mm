//
//  RecordingAvailabilityListenerWrapper.m
//  Unity-iPhone
//
//  Created by Ayyappa J on 06/10/22.
//

#import "RecordingAvailabilityListenerWrapper.h"

@implementation RecordingAvailabilityListenerWrapper

- (void)onAvailable:(nonnull NSString *)path {
    if(_onAvailableCallback != NULL)
    {
        _onAvailableCallback(self.tag, NPCreateCStringFromNSString(path));
    }
}

@end
