using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using VoxelBusters.CoreLibrary;

namespace VoxelBusters.ScreenRecorderKit.GifRecorderCore
{
    public class GifConverter
    {
        public static void ConvertFramesToGifAsync(RenderTexture[] frameTextures, string name,
            int width, int height,
            int fps, int repeat,
            int quality, EventCallback<GifTexture> callback)
        {
            var     coroutine   = ConvertRenderTexturesToGifFrames(
                frameTextures: frameTextures,
                width: width,
                height: height,
                format: TextureFormat.RGBA32,
                callback: (gifFrames, error) =>
                {
                    if (error == null)
                    {
                        // Create texture object with specified properties
                        var     gifTexture  = new GifTexture(
                            width: width,
                            height: height,
                            fps: fps,
                            repeat: repeat,
                            quality: quality,
                            format: TextureFormat.RGBA32,
                            frames: gifFrames);
                        callback?.Invoke(gifTexture, null);
                    }
                    else
                    {
                        callback?.Invoke(null, error);
                    }
                });
            SurrogateCoroutine.StartCoroutine(coroutine);
        }

        private static IEnumerator ConvertRenderTexturesToGifFrames(RenderTexture[] frameTextures, int width,
            int height, TextureFormat format,
            EventCallback<GifFrame[]> callback)
        {
            // Get a temporary texture to read RenderTexture data
			var     tempTex = CreateTexture(width, height, format);

            // Convert render textures to frames
            int     frameCount      = frameTextures.Length;
            var     gifFrames       = new GifFrame[frameCount];
            for (int fIter = 0; fIter < frameCount; fIter++)
            {
                gifFrames[fIter]    = ToGifFrame(
                    renderTexture: frameTextures[fIter],
                    width: width,
                    height: height,
                    tempTex: tempTex);
                yield return null;
            }

            // Send result to handler function
            callback(gifFrames, null);
        }

        private static GifFrame ToGifFrame(RenderTexture renderTexture, int width,
            int height, Texture2D tempTex)
        {
            // Copy texture data
            RenderTexture.active        = renderTexture;
			tempTex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
			tempTex.Apply();
			RenderTexture.active        = null;

            // Create frame object
            return new GifFrame(width, height, tempTex.GetPixels32());
        }

        private static Texture2D CreateTexture(int width, int height, TextureFormat format)
        {
            return new Texture2D(width, height, format, false)
            {
                wrapMode    = TextureWrapMode.Clamp,
			    filterMode  = FilterMode.Bilinear,
                anisoLevel  = 0,
                hideFlags   = HideFlags.HideAndDontSave,
            };
        }
    }
}