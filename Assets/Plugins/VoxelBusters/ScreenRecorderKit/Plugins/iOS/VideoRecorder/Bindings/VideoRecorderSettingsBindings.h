//
//  VideoRecorderSettingsBindings.h
//  UnityFramework
//
//  Created by Ayyappa J on 07/10/22.
//

#import <Foundation/Foundation.h>
#import "VideoRecorderBindingsCommon.h"

#define METHOD(ReturnType, Name, ...) METHOD_BASE(ReturnType, VideoRecorderSettings, Name, __VA_ARGS__)

METHOD(void*,   Create);
METHOD(bool,   GetEnableMicrophone, void* owner);
METHOD(void,   SetEnableMicrophone, void* owner, bool val);
