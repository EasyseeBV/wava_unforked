using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VoxelBusters.CoreLibrary;

namespace VoxelBusters.ScreenRecorderKit.GifRecorderCore
{
    public class UploadRequestBody
    {
        #region Properties

        public Asset Asset { get; private set; }

        public string Username { get; private set; }

        public string[] Tags { get; private set; }

        public string SourceUrl { get; private set; }

        #endregion

        #region Constructors

        public UploadRequestBody(Asset asset, string username = null,
            string[] tags = null, string sourceUrl = null)
        {
            Assert.IsArgNotNull(asset, nameof(asset));

            // Set properties
            Asset       = asset;
            Username    = username;
            Tags        = tags;
            SourceUrl   = sourceUrl;
        }

        #endregion
    }
}