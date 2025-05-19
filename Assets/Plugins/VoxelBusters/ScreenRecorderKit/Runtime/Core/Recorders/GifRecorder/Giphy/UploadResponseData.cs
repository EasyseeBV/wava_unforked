using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace VoxelBusters.ScreenRecorderKit.GifRecorderCore
{
    [System.Serializable]
    public class UploadResponseData
    {
        #region Properties

        [JsonProperty("id")]
        public string Id { get; private set; }

        #endregion

        #region Constructors

        public UploadResponseData(string id)
        {
            // set properties
            Id  = id;
        }

        #endregion
    }
}