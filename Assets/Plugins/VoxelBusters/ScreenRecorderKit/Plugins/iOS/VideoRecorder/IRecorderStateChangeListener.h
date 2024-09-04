//
//  IRecordingStateChangeListener.h
//  UnityFramework
//
//  Created by Ayyappa J on 27/09/22.
//

#import <Foundation/Foundation.h>

NS_ASSUME_NONNULL_BEGIN

@protocol IRecorderStateChangeListener <NSObject>

-(void) onInvalid;
-(void) onPrepare;
-(void) onRecord;
-(void) onPause;
-(void) onStop;

@end
NS_ASSUME_NONNULL_END
