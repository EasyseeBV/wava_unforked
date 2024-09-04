//
//  ISaveRecordingListener.h
//  UnityFramework
//
//  Created by Ayyappa J on 27/09/22.
//

#import <Foundation/Foundation.h>

NS_ASSUME_NONNULL_BEGIN

@protocol ISaveRecordingListener <NSObject>

-(void) onSuccess:(NSString*) path;
-(void) onFailure:(NSError*) error;

@end

NS_ASSUME_NONNULL_END
