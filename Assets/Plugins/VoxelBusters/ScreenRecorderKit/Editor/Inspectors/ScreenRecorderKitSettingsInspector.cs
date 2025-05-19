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
        #region Fields

        private     EditorSectionInfo           m_videoRecorderSection;

        private     EditorSectionInfo           m_gifRecorderSection;

        private     ButtonMeta[]                m_resourceButtons;

        #endregion

        #region Base class methods

        protected override void OnEnable()
        {
            base.OnEnable();

            m_videoRecorderSection  = new EditorSectionInfo(displayName: "Video Recorder Settings",
                                                            property: serializedObject.FindProperty("m_videoRecorderSettings"),
                                                            drawStyle: EditorSectionDrawStyle.Expand);
            m_gifRecorderSection    = new EditorSectionInfo(displayName: "GIF Recorder Settings",
                                                            property: serializedObject.FindProperty("m_gifRecorderSettings"),
                                                            drawStyle: EditorSectionDrawStyle.Expand);
            m_resourceButtons       = new ButtonMeta[]
            {
                new ButtonMeta(label: "Documentation",  onClick: ScreenRecorderKitEditorUtility.OpenDocumentationPage),
                new ButtonMeta(label: "Tutorials",      onClick: ScreenRecorderKitEditorUtility.OpenTutorialsPage),
                new ButtonMeta(label: "Support",        onClick: ScreenRecorderKitEditorUtility.OpenSupportPage),
                new ButtonMeta(label: "Write Review",	onClick: ScreenRecorderKitEditorUtility.OpenProductPage),
                new ButtonMeta(label: "Subscribe",	    onClick: ScreenRecorderKitEditorUtility.OpenSubscribePage),
            };
        }

        protected override UnityPackageDefinition GetOwner()
        {
            return ScreenRecorderKitSettings.Package;
        }

        protected override string[] GetTabNames()
        {
            return new string[]
            {
                DefaultTabs.kGeneral,
                DefaultTabs.kMisc,
            };
        }

        protected override EditorSectionInfo[] GetSectionsForTab(string tab)
        {
            return null;
        }

        protected override bool DrawTabView(string tab)
        {
            switch (tab)
            {
                case DefaultTabs.kGeneral:
                    DrawGeneralTabView();
                    return true;

                case DefaultTabs.kMisc:
                    DrawMiscTabView();
                    return true;

                default:
                    return false;
            }
        }

        #endregion

        #region Private methods

        private void DrawGeneralTabView()
        {
            LayoutBuilder.DrawSection(m_videoRecorderSection,
                                      showDetails: true,
                                      selectable: false);
            LayoutBuilder.DrawSection(m_gifRecorderSection,
                                      showDetails: true,
                                      selectable: false);
        }

        private void DrawMiscTabView()
        {
            DrawButtonList(m_resourceButtons);
        }

        #endregion
    }
}