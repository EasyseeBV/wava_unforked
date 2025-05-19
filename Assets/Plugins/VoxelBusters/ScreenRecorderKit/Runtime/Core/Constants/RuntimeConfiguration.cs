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

        public static NativeFeatureRuntimeConfiguration VideoRecorder { get; private set; }

        #endregion

        #region Constructors

        static RuntimeConfiguration()
        {
            VideoRecorder   = new NativeFeatureRuntimeConfiguration(packages: new NativeFeatureRuntimePackage[]
                                                                    {
                                                                        NativeFeatureRuntimePackage.iOS(assembly: $"{kMainAssembly}.iOSModule",
                                                                                                        ns: $"{kRootNamespaceVideoRecorder}.iOS",
                                                                                                        nativeInterfaceType: "VideoRecorderInterface"),
                                                                        NativeFeatureRuntimePackage.Android(assembly: $"{kMainAssembly}.AndroidModule",
                                                                                                            ns: $"{kRootNamespaceVideoRecorder}.Android",
                                                                                                            nativeInterfaceType: "VideoRecorderInterface"),
                                                                    },
                                                                    simulatorPackage: NativeFeatureRuntimePackage.Generic(assembly: $"{kMainAssembly}.SimulatorModule",
                                                                                                                          ns: $"{kRootNamespaceVideoRecorder}.Simulator",
                                                                                                                          nativeInterfaceType: "VideoRecorderInterface"),
                                                                    fallbackPackage: NativeFeatureRuntimePackage.Generic(assembly: kMainAssembly,
                                                                                                                         ns: kRootNamespaceVideoRecorder,
                                                                                                                         nativeInterfaceType: "NullVideoRecorderInterface"));
        }

        #endregion
    }
}