using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VoxelBusters.ScreenRecorderKit
{
    public class VideoRecorderRuntimeSettingsOption
    {
        #region Properties

        internal RuntimeSettingsOptionType OptionType { get; private set; }

        internal bool? BoolValue { get; private set; }

        #endregion

        #region Constructors

        private VideoRecorderRuntimeSettingsOption(RuntimeSettingsOptionType optionType, bool? boolValue = null)
        {
            // Set properties
            OptionType          = optionType;
            BoolValue           = boolValue;
        }

        #endregion

        #region Static methods

        public VideoRecorderRuntimeSettingsOption Microphone(bool enabled) => new VideoRecorderRuntimeSettingsOption(
            optionType: RuntimeSettingsOptionType.EnableMicrophone,
            boolValue: enabled);

        /*public VideoRecorderRuntimeSettingsOption Controls(bool show) => new VideoRecorderRuntimeSettingsOption(
            optionType: RuntimeSettingsOptionType.ShowControls,
            boolValue: show);*/

        #endregion

        #region Nested types

        public enum RuntimeSettingsOptionType
        {
            EnableMicrophone = 1,

            //ShowControls,
        }

        #endregion
    }
}