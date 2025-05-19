using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace VoxelBusters.ScreenRecorderKit.GifRecorderCore
{
    public class UploadResponseObject
    {
        #region Properties

        [JsonProperty("data")]
        public UploadResponseData Data { get; private set; }

        #endregion

        #region Constructors

        public UploadResponseObject(UploadResponseData data)
        {
            // set properties
            Data    = data;
        }

        #endregion
    }
}