using UnityEngine;
using System.Collections;

namespace VoxelBusters.ScreenRecorderKit
{
	internal static class AssetConstants
	{
		#region Paths

		public static string ExtrasPath => "Assets/Plugins/VoxelBusters/ScreenRecorderKit/Essentials";

        public static string EditorExtrasPath => $"{ExtrasPath}/Editor";

		public static string AndroidPluginPath => $"Assets/Plugins/Android";

		public static string AndroidProjectFolderName => "com.voxelbusters.screenrecorderkit.androidlib";

		public static string AndroidProjectPath => $"{AndroidPluginPath}/{AndroidProjectFolderName}";

		public static string NativePluginsExporterName => "ScreenRecorderKit";

		#endregion
	}
}