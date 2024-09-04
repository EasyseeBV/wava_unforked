using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VoxelBusters.CoreLibrary;

namespace VoxelBusters.ScreenRecorderKit
{
    internal static class ScreenRecorderError
    {
        #region Private fields

        private static readonly string kRecordingInProgress                 = "The recorder is currently busy recording.";
        private static readonly string kActiveRecordingUnavailable          = "No active recording available.";
        private static readonly string kPermissionUnavailable               = "Permission unavailable";
        private static readonly string kFeatureUnsupported                  = "Feature not supported.";
        private static readonly string kSharingServiceUnavailable           = "No service available for sharing.";
        private static readonly string kUnknown                             = "Unknown error";

        #endregion

        #region Public

        public static Error RecorderIsBusy(string domain, string description = null) => CreateError(domain, ScreenRecorderErrorCode.kRecordingInProgress, description ?? kRecordingInProgress);
        public static Error ActiveRecordingUnavailable(string domain, string description = null) => CreateError(domain, ScreenRecorderErrorCode.kActiveRecordingUnavailable, description ?? kActiveRecordingUnavailable);
        public static Error PermissionUnavailable(string domain, string description = null) => CreateError(domain, ScreenRecorderErrorCode.kPermissionUnavailable, description ?? kPermissionUnavailable);
        public static Error FeatureUnsupported(string domain, string description = null) => CreateError(domain, ScreenRecorderErrorCode.kFeatureUnsupported, description ?? kFeatureUnsupported);
        public static Error ShareServiceUnavailable(string domain, string description = null) => CreateError(domain, ScreenRecorderErrorCode.kShareServiceUnavailable, description ?? kSharingServiceUnavailable);

        public static Error Unknown(string domain, string description = null) => CreateError(domain, ScreenRecorderErrorCode.kUnknown, description ?? kUnknown);

        #endregion


        #region Private

        private static Error CreateError(string domain, int code, string description)
        {
            return new Error(domain, code, description);
        }

        internal static Error RecorderIsBusy(string errorDomain)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
