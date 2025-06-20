//
//  UIViewController+Presentation.h
//  Native Plugins
//
//  Created by Ashwin kumar on 22/01/19.
//  Copyright (c) 2019 Voxel Busters Interactive LLP. All rights reserved.
//

#import <Foundation/Foundation.h>


#if !TARGET_OS_TV
@protocol NPUIPopoverPresentationControllerDelegate <UIPopoverPresentationControllerDelegate>
#else
@protocol NPUIPopoverPresentationControllerDelegate <NSObject>
#endif
@end

@interface UIViewController (Presentation)

- (void)presentViewControllerInPopoverStyleIfRequired:(UIViewController*)viewControllerToPresent
                                         withDelegate:(id<NPUIPopoverPresentationControllerDelegate>)delegate
                                         fromPosition:(CGPoint)position
                                             animated:(BOOL)flag
                                           completion:(void (^)())completion;

- (void)presentViewControllerInPopoverStyleIfRequired:(UIViewController*)viewControllerToPresent
                                         withDelegate:(id<NPUIPopoverPresentationControllerDelegate>)delegate
                                         fromPosition:(CGPoint)position
                             permittedArrowDirections:(UIPopoverArrowDirection)direction
                                             animated:(BOOL)flag
                                           completion:(void (^)())completion;

- (void)presentViewControllerInFullScreen: (UIViewController*)viewControllerToPresent
                                 animated: (BOOL)flag
                                completion: (void (^)())completion;
@end
