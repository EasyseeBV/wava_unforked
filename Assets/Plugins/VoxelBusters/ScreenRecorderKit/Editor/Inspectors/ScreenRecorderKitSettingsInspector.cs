using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor;
using VoxelBusters.CoreLibrary;
using VoxelBusters.CoreLibrary.Editor;

namespace VoxelBusters.ScreenRecorderKit.Editor
{
	[CustomEditor(typeof(ScreenRecorderKitSettings))]
	public class ScreenRecorderKitSettingsInspector : SettingsObjectInspector
	{
        #region Base class methods

        protected override UnityPackageDefinition GetOwner()
        {
            return ScreenRecorderKitSettings.Package;
        }

		protected override InspectorDrawStyle GetDrawStyle()
        {
            return InspectorDrawStyle.Group;
        }

        protected override ButtonInfo[] GetTopBarButtons()
        {
            return new ButtonInfo[]
            {
                new ButtonInfo(label: "Tutorials",      onClick: ScreenRecorderKitEditorUtility.OpenTutorialsPage),
                new ButtonInfo(label: "Documentation",  onClick: ScreenRecorderKitEditorUtility.OpenDocumentationPage),
                new ButtonInfo(label: "Discord",        onClick: ScreenRecorderKitEditorUtility.OpenSupportPage),
                new ButtonInfo(label: "Write Review",	onClick: ScreenRecorderKitEditorUtility.OpenReviewPage),
                new ButtonInfo(label: "Subscribe",		onClick: ScreenRecorderKitEditorUtility.OpenSubscribePage),
            };
        }

        protected override PropertyGroupInfo[] GetPropertyGroups()
        {
            return new PropertyGroupInfo[]
                {
                    new PropertyGroupInfo(reference: serializedObject.FindProperty("m_videoRecorderSettings"), displayName: "Video Recorder Settings"),
                    new PropertyGroupInfo(reference: serializedObject.FindProperty("m_gifRecorderSettings"), displayName: "GIF Recorder Settings"),
                };
        }

        protected override void DrawFooter()
        {
            base.DrawFooter();
#if ENABLE_VOXELBUSTERS_SCREEN_RECORDER_KIT_UPM_SUPPORT
            GUILayout.Space(5f);
            ShowMigrateToUpmOption();
#endif
        }

#endregion
	}
}