using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VoxelBusters.CoreLibrary;
#if ENABLE_VOXELBUSTERS_ESSENTIAL_KIT
using VoxelBusters.EssentialKit;
#endif

namespace VoxelBusters.ScreenRecorderKit.Addons
{
    public class DefaultServiceProvider : IExternalServiceProvider
    {
        #region IExternalServiceProvider implementation

        public void ShareGif(string filePath, string title = null,
            string description = null, CompletionCallback callback = null)
        {
#if ENABLE_VOXELBUSTERS_ESSENTIAL_KIT
            var     items   = new List<ShareItem>();
            items.Add(() => !string.IsNullOrEmpty(title), () => ShareItem.Text(title));
            items.Add(ShareItem.URL(URLString.URLWithPath(filePath)));
            SharingServices.ShowShareSheet(
                callback: (result, error) =>
                {
                    callback?.Invoke((result.ResultCode == ShareSheetResultCode.Done), error);
                },
                shareItems: items.ToArray());
#else
            callback?.Invoke(false, new Error(GifRecorder.ErrorDomain, ScreenRecorderErrorCode.kShareServiceUnavailable, "Service not available."));
#endif
        }

        public void ShareVideo(string filePath, string title = null,
            string description = null, CompletionCallback callback = null)
        {
#if ENABLE_VOXELBUSTERS_ESSENTIAL_KIT
            var     items   = new List<ShareItem>();
            items.Add(() => !string.IsNullOrEmpty(title), () => ShareItem.Text(title));
            items.Add(ShareItem.URL(URLString.FileURLWithPath(filePath)));
            SharingServices.ShowShareSheet(
                callback: (result, error) =>
                {
                    callback?.Invoke((result.ResultCode == ShareSheetResultCode.Done), error);
                },
                shareItems: items.ToArray());
#else
            callback?.Invoke(false, new Error(VideoRecorder.ErrorDomain, ScreenRecorderErrorCode.kShareServiceUnavailable, "Service not available."));
#endif
        }

        #endregion
    }
}