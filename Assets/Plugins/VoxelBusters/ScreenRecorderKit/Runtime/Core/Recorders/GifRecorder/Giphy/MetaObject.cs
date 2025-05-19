using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace VoxelBusters.ScreenRecorderKit.GifRecorderCore
{
    [System.Serializable]
    public class MetaObject
    {
        #region Properties

        [JsonProperty("msg")]
        public string Message { get; private set; }

        [JsonProperty("response_id")]
        public string ResponseId { get; private set; }

        #endregion

        #region Constructors

        public MetaObject(string message, string responseId)
        {
            // set properties
            Message     = message;
            ResponseId  = responseId;
        }

        #endregion
    }
}