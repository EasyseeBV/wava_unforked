//
//  VideoRecorder+VideoRecorder_OverlayControls.m
//  UnityFramework
//
//  Created by Ayyappa J on 03/10/22.
//

#import "VideoRecorder+OverlayControls.h"
#import "VideoRecorderUIOverlayWindow.h"
#import "VideoRecorderUIOverlayViewController.h"

@interface VideoRecorder ()
@property (nonatomic, retain) VideoRecorderUIOverlayWindow *overlayWindow;
@property (nonatomic, retain) UIButton* uiButton;
@end


@implementation VideoRecorder (OverlayControls)
#pragma mark - UI Overlay

-(void) showOverlay
{
    if(self.overlayWindow == nil)
    {
        VideoRecorderUIOverlayWindow* window = [[VideoRecorderUIOverlayWindow alloc] initWithFrame: [UIScreen mainScreen].bounds];
        [window setUserInteractionEnabled:YES];
        [window makeKeyAndVisible];

        // For hiding status bar
        UIViewController* vc = [[VideoRecorderUIOverlayViewController alloc] init];
        [vc.view setUserInteractionEnabled:FALSE];
        [window setRootViewController:vc];


        self.uiButton = [self addControl];
        [window addSubview:self.uiButton];
        
        [self setViewPosition:self.uiButton inParentFrame:[window frame]];
        
        window.windowLevel = UIWindowLevelAlert - 1;
                
        self.overlayWindow = window;

    }
    
    self.overlayWindow.hidden = FALSE;
    
    
    [[NSNotificationCenter defaultCenter] addObserver:self
                                             selector:@selector(orientationChanged:)
                                                 name:UIDeviceOrientationDidChangeNotification
                                               object:[UIDevice currentDevice]];
}

-(void) hideOverlay
{
    self.overlayWindow.hidden = TRUE;
    [[NSNotificationCenter defaultCenter] removeObserver:self
                                                    name:UIDeviceOrientationDidChangeNotification
                                                  object:[UIDevice currentDevice]];
}


-(UIButton*) addControl
{
    UIButton *button    = [UIButton buttonWithType:UIButtonTypeCustom];
    
    UIImage *startButtonImage   = [self getImage:@"start-recording" ofType:@"png"];
    UIImage *stopButtonImage    = [self getImage:@"stop-recording" ofType:@"png"];
    
    [button setImage:startButtonImage   forState:UIControlStateNormal];
    [button setImage:stopButtonImage    forState:UIControlStateSelected];
    
    CGRect size = CGRectMake(0, 0, startButtonImage.size.width, startButtonImage.size.height);
    
    
    [button setFrame:size];
    [button addTarget:self action:@selector(onButtonClick:) forControlEvents:UIControlEventTouchUpInside];
    return button;
}

-(void) setViewPosition:(UIView*) view inParentFrame:(CGRect) frame
{
    CALayer* buttonLayer    = [view layer];
    [buttonLayer setAnchorPoint:CGPointMake(0.5, 0.5)];
    [buttonLayer setPosition:CGPointMake(CGRectGetMaxX(frame)/2, CGRectGetMaxY(frame)-view.frame.size.height)];
}


-(void) onButtonClick:(UIButton*) sender
{
    if(sender.selected)
    {
        //Call stop recording
    }
    else
    {
        //Call start recording
    }
    
    sender.selected = !sender.selected;
}

-(UIImage*) getImage:(NSString*) name ofType:(NSString*) type
{
    NSString* filePath         = [[NSBundle mainBundle] pathForResource:name ofType:type inDirectory:@"Data/CrossPlatformReplayKit"];
    UIImage* image = [UIImage imageWithContentsOfFile:filePath];
    return image;
}

- (void)orientationChanged:(NSNotification *)notification
{
    [self setViewPosition:self.uiButton inParentFrame:[self.overlayWindow frame]];
}

@end
