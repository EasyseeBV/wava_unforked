//
//  VideoRecorderBindingsCommon.h
//  UnityFramework
//
//  Created by Ayyappa J on 07/10/22.
//

#import <Foundation/Foundation.h>
#import "NPKit.h"

#define CONCAT(A,B,C) A##_##B##_##C
#define METHOD_BASE(ReturnType, Class, Name, ...) NPBINDING DONTSTRIP ReturnType CONCAT(ScreenRecorderKit,Class,Name)(__VA_ARGS__)
#define CAST_TO_OBJC(type, owner) (__bridge type*)owner

