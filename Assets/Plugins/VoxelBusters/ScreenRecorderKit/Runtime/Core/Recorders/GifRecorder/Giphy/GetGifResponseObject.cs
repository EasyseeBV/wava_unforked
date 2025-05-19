using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace VoxelBusters.ScreenRecorderKit.GifRecorderCore
{
    public class GetGifResponseObject
    {
        #region Properties

        [JsonProperty("data")]
        public GifObject Data { get; private set; }

        #endregion

        #region Constructors

        public GetGifResponseObject(GifObject data)
        {
            // set properties
            Data    = data;
        }

        #endregion
    }
}