using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace VoxelBusters.ScreenRecorderKit.GifRecorderCore
{
    public class GifObject
    {
        #region Properties

        [JsonProperty("id")]
        public string Id { get; private set; }

        [JsonProperty("url")]
        public string Url { get; private set; }

        [JsonProperty("bitly_gif_url")]
        public string BitlyGifUrl { get; private set; }

        [JsonProperty("bitly_url")]
        public string BitlyUrl { get; private set; }

        [JsonProperty("username")]
        public string Username { get; private set; }

        [JsonProperty("source")]
        public string Source { get; private set; }

        [JsonProperty("title")]
        public string Title { get; private set; }

        [JsonProperty("rating")]
        public string Rating { get; private set; }

        #endregion

        #region Constructors

        public GifObject(string id, string url = null,
            string bitlyUrl = null, string username = null,
            string source = null, string title = null,
            string rating = null)
        {
            // set properties
            Id          = id;
            Url         = url;
            BitlyUrl    = bitlyUrl;
            Username    = username;
            Source      = source;
            Title       = title;
            Rating      = rating;
        }

        #endregion
    }
}