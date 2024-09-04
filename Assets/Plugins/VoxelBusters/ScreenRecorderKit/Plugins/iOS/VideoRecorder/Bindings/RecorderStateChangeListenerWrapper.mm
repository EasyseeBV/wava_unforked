//
//  StateChangeListenerWrapper.m
//  UnityFramework
//
//  Created by Ayyappa J on 10/10/22.
//

#import "RecorderStateChangeListenerWrapper.h"

@implementation RecorderStateChangeListenerWrapper

- (void)onInvalid {
    if(_onInvalidCallback != nil)
    {
        _onInvalidCallback(self.tag);
    }
}

- (void)onPrepare {
    if(_onPrepareCallback != nil)
    {
        _onPrepareCallback(self.tag);
    }
}

- (void)onRecord {
    if(_onRecordCallback != nil)
    {
        _onRecordCallback(self.tag);
    }
}

- (void)onPause {
    if(_onPauseCallback != nil)
    {
        _onPauseCallback(self.tag);
    }
}

- (void)onStop {
    if(_onStopCallback != nil)
    {
        _onStopCallback(self.tag);
    }
}

@end
