using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VoxelBusters.CoreLibrary;

namespace VoxelBusters.ScreenRecorderKit
{
    [IncludeInDocs]
    public static class ScreenRecorderErrorCode
    {
        /// <summary> Error code indicating that an unknown or unexpected error occurred. </summary>
        public  const   int     kUnknown                = 0;

        /// <summary> Error code indicating that api is unavailable. </summary>
        public  const   int     kApiUnavailable         = kUnknown + 1;

        /// <summary> Error code indicating that recorder is currently busy recording as recording is in progress. </summary>
        public  const   int     kRecordingInProgress    = kApiUnavailable + 1;

        /// <summary> Error code indicating that required permission is unavailable. </summary>
        public  const   int     kPermissionUnavailable  = kRecordingInProgress + 1;

        /// <summary> Error code indicating that there is no active recording to operate on. </summary>
        public  const   int     kActiveRecordingUnavailable = kPermissionUnavailable + 1;

        /// <summary> Error code indicating that feature is not supported. </summary>
        public  const   int     kFeatureUnsupported     = kActiveRecordingUnavailable + 1;

        /// <summary> Error code indicating that there is no service available for sharing. </summary>
        public  const   int     kShareServiceUnavailable = kFeatureUnsupported + 1;
    }
}