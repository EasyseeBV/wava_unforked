using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VoxelBusters.ScreenRecorderKit
{
    public partial class VideoRecorderSettings
    {
        [System.Serializable]
        public class CustomBitRateSetting
        {
            #region Fields

            [SerializeField]
            private     bool    m_allowCustomBitrates   = false;

            [SerializeField]
            [Range(0.0f, 1.0f)]
            private     float   m_bitrateFactor         = 0.5f;

            #endregion

            #region Properties

            public bool AllowCustomBitrates => m_allowCustomBitrates;

            public float BitrateFactor => m_bitrateFactor;

            #endregion

            #region Constructors

            public CustomBitRateSetting(bool allowCustomBitrates = false, float bitrateFactor = 0.5f)
            {
                // set properties
                m_allowCustomBitrates   = allowCustomBitrates;
                m_bitrateFactor         = bitrateFactor;
            }

            #endregion
        }
    }
}