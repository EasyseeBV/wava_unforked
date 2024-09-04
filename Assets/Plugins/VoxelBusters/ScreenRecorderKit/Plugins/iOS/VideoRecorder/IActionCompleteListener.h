//
//  IActionCompleteListener.h
//  UnityFramework
//
//  Created by Ayyappa J on 27/09/22.
//

#import <Foundation/Foundation.h>

NS_ASSUME_NONNULL_BEGIN

@protocol IActionCompleteListener <NSObject>

-(void) onSuccess;
-(void) onFailure:(NSError*) error;

@end

NS_ASSUME_NONNULL_END
