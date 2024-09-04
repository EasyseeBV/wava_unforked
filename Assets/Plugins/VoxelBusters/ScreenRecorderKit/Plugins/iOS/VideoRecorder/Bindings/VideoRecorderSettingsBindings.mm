//
//  VideoRecorderSettingsBindings.m
//  UnityFramework
//
//  Created by Ayyappa J on 07/10/22.
//

#import "VideoRecorderSettingsBindings.h"
#import "VideoRecorderSettings.h"

METHOD(void*, Create)
{
    return (__bridge_retained void*)[[VideoRecorderSettings alloc] init];
}

METHOD(bool, GetEnableMicrophone, void* owner)
{
    VideoRecorderSettings*  settings  = CAST_TO_OBJC(VideoRecorderSettings, owner);
    return [settings enableMicrophone];
}

METHOD(void, SetEnableMicrophone, void* owner, bool val)
{
    VideoRecorderSettings*  settings  = CAST_TO_OBJC(VideoRecorderSettings, owner);
    [settings setEnableMicrophone:val];
}

