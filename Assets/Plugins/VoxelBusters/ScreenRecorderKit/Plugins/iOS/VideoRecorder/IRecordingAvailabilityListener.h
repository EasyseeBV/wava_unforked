//
//  IRecordingAvailabilityListener.h
//  UnityFramework
//
//  Created by Ayyappa J on 27/09/22.
//

#import <Foundation/Foundation.h>

NS_ASSUME_NONNULL_BEGIN

@protocol IRecordingAvailabilityListener <NSObject>

-(void) onAvailable:(NSString*) path;

@end


NS_ASSUME_NONNULL_END
