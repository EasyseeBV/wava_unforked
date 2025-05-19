using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using VoxelBusters.CoreLibrary;

namespace VoxelBusters.ScreenRecorderKit.GifRecorderCore
{
    public class GiphyApi
    {
        #region Properties

        public string ApiKey { get; private set; }

        private string DefaultUsername { get; set; }

        #endregion

        #region Constructors

        public GiphyApi(string apiKey, string defaultUsername)
        {
            this.ApiKey             = apiKey;
            this.DefaultUsername    = defaultUsername;
        }

        #endregion

        #region Public methods

        public void GetGifById(string id, SuccessCallback<GetGifResponseObject> onSuccess = null, ErrorCallback onError = null)
        {
            // Validate the data
            Assert.IsArgNotNull(id, nameof(id));

            // Make request
            string  path            = $"api.giphy.com/v1/gifs/{id}?api_key={ApiKey}";
            var     request         = UnityWebRequest.Get(RestClient.EscapeUrl(path));
            request.downloadHandler = new DownloadHandlerBuffer();
            RestClient.SharedInstance.StartWebRequest<GetGifResponseObject>(
                request,
                onSuccess: (result) => onSuccess?.Invoke(result),
                onError: (error) => onError?.Invoke(new Error(error)));
        }

        public void Upload(UploadRequestBody body, SuccessCallback<UploadResponseObject> onSuccess = null, ErrorCallback onError = null)
        {
            // Validate the data
            Assert.IsArgNotNull(body, nameof(body));

            // Create request
            var     formData    = new List<IMultipartFormSection>();

            // Required fields
            formData.Add(new MultipartFormDataSection("api_key", ApiKey));
            formData.Add(new MultipartFormFileSection("file", body.Asset.Data, body.Asset.Name, "application/octet-stream"));
            formData.Add(
                condition: () => !string.IsNullOrEmpty(body.Username),
                getItem: () => new MultipartFormDataSection("username", body.Username));

            // Optional fields (Data can't be empty -> https://github.com/Unity-Technologies/UnityCsReference/blob/3ff5b7b7ff1dac2a8c52b74b9fb7148a7f4e9a15/Modules/UnityWebRequest/Public/MultipartFormHelper.cs#L26)
            formData.Add(
                condition: () => (body.Tags != null && body.Tags.Length > 0),
                getItem: () => new MultipartFormDataSection("tags", string.Join(",", body.Tags)));
            formData.Add(
                condition: () => !string.IsNullOrEmpty(body.SourceUrl),
                getItem: () => new MultipartFormDataSection("source_post_url", body.SourceUrl));

            string  path            = $"https://upload.giphy.com/v1/gifs?api_key={ApiKey}";
            var     request         = UnityWebRequest.Post(RestClient.EscapeUrl(path), formData);
            request.downloadHandler = new DownloadHandlerBuffer();
            RestClient.SharedInstance.StartWebRequest<UploadResponseObject>(
                request,
                onSuccess: (result) => onSuccess?.Invoke(result),
                onError: (error) => onError?.Invoke(new Error(error)));
        }

        #endregion
    }
}