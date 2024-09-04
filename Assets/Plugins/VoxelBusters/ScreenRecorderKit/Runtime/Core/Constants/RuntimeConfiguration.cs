using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VoxelBusters.CoreLibrary.NativePlugins;

namespace VoxelBusters.ScreenRecorderKit
{
    internal static class RuntimeConfiguration
    {
        #region Constants

        private     const   string      kMainAssembly                   = "VoxelBusters.ScreenRecorderKit";

        private     const   string      kRootNamespaceVideoRecorder     = "VoxelBusters.ScreenRecorderKit.VideoRecorderCore";

        #endregion

        #region Static properties

        public static NativeFeatureRuntimeConfiguration VideoRecorder { get; private set; } = new NativeFeatureRuntimeConfiguration(
            packages: new NativeFeatureRuntimePackage[]
            {
                new NativeFeatureRuntimePackage(platform: RuntimePlatform.IPhonePlayer, assembly: $"{kMainAssembly}.iOSModule",     ns: $"{kRootNamespaceVideoRecorder}.iOS",        nativeInterfaceType: "VideoRecorderInterface"),
                new NativeFeatureRuntimePackage(platform: RuntimePlatform.tvOS,         assembly: $"{kMainAssembly}.iOSModule",     ns: $"{kRootNamespaceVideoRecorder}.iOS",        nativeInterfaceType: "VideoRecorderInterface"),
                new NativeFeatureRuntimePackage(platform: RuntimePlatform.Android,      assembly: $"{kMainAssembly}.AndroidModule", ns: $"{kRootNamespaceVideoRecorder}.Android",    nativeInterfaceType: "VideoRecorderInterface"),
            },
            simulatorPackage: new NativeFeatureRuntimePackage(assembly: $"{kMainAssembly}.SimulatorModule", ns: $"{kRootNamespaceVideoRecorder}.Simulator", nativeInterfaceType: "VideoRecorderInterface"),
            fallbackPackage: new NativeFeatureRuntimePackage(assembly: kMainAssembly, ns: kRootNamespaceVideoRecorder, nativeInterfaceType: "NullVideoRecorderInterface"));

        #endregion
    }
}