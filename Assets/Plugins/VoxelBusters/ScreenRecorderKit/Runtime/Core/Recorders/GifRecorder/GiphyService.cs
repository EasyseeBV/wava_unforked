using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VoxelBusters.CoreLibrary;
using VoxelBusters.ScreenRecorderKit.GifRecorderCore;

namespace VoxelBusters.ScreenRecorderKit
{
    public static class GiphyService
    {
        #region Static fields

        private     static  GiphyApi        s_giphyApi;

        #endregion

        #region Properties

        private static string ApiKey { get; set; }

        private static string Username { get; set; }

        public static GiphyApi GiphyApi => ObjectHelper.CreateInstanceIfNull(
            ref s_giphyApi,
            () =>
            {
                EnsureInitialized();
                return new GiphyApi(ApiKey, Username);
            });

        #endregion

        #region Static methods

        public static void Init(string apiKey, string username)
        {
            // Set new values
            ApiKey      = apiKey;
            Username    = username;

            // Reset properties
            s_giphyApi  = null;
        }

        #endregion

        #region Api operations

        public static void Upload(Asset asset, string username = null,
            string[] tags = null, string sourceUrl = null,
            SuccessCallback<string> onSuccess = null, ErrorCallback onError = null)
        {
            GiphyApi.Upload(
                body: new UploadRequestBody(asset, username, tags, sourceUrl),
                onSuccess: (uploadResponse) =>
                {
                    GiphyApi.GetGifById(
                        id: uploadResponse.Data.Id,
                        onSuccess: (getGifResponse) =>
                        {
                            onSuccess?.Invoke(getGifResponse.Data.Url);
                        },
                        onError: onError);
                },
                onError : onError);
        }

        #endregion

        #region Private static methods

        private static void EnsureInitialized()
        {
            if (ApiKey != null) return;

            // Use properties specified in default Settings object
            var     recorderSettigns    = ScreenRecorderKitSettings.Instance.GifRecorderSettings;
            ApiKey      = recorderSettigns.GiphyService.ApiKey;
            Username    = recorderSettigns.GiphyService.Username;
        }

        #endregion
    }
}