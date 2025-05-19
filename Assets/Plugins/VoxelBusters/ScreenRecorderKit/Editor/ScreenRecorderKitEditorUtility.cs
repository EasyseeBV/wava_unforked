using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using VoxelBusters.CoreLibrary;
using VoxelBusters.CoreLibrary.NativePlugins;
using VoxelBusters.CoreLibrary.Editor;

namespace VoxelBusters.ScreenRecorderKit.Editor
{
    public static class ScreenRecorderKitEditorUtility
    {
        #region Constants

		// URL
        private     const   string      kProductUrl                     = "https://link.voxelbusters.com/screen-recorder-kit";

        private     const   string      kDocumentationUrl               = "https://voxelbusters.com/products/cross-platform-screen-recorder-kit/documentation";

        private const   string          kSupportUrl			            = "https://discord.gg/voxel-busters-672868273779507223";

		private		const   string      kTutorialUrl		            = "https://assetstore.screenrecorderkit.voxelbusters.com";		

		private		const   string	    kSubscribePageUrl	            = "http://bit.ly/2ESQfAg";

        #endregion

        #region Public methods

        #endregion

        #region Resource methods

        public static void OpenTutorialsPage()
        {
            Application.OpenURL(kTutorialUrl);
        }

        public static void OpenDocumentationPage()
        {
            Application.OpenURL(kDocumentationUrl);
        }

        public static void OpenSupportPage()
        {
            Application.OpenURL(kSupportUrl);
        }

        public static void OpenSubscribePage()
        {
            Application.OpenURL(kSubscribePageUrl);
        }

        public static void OpenProductPage()
        {
            Application.OpenURL(kProductUrl);
        }

        #endregion
    }
}