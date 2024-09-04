using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VoxelBusters.CoreLibrary;

namespace VoxelBusters.ScreenRecorderKit
{
    public class GifTexture : IEnumerable<Texture2D>
    {
        #region Properties

        public string Name { get; internal set; }

        public int Width { get; private set; }

        public int Height { get; private set; }

        public int FramesPerSecond { get; private set; }

        public int Repeat { get; private set; }

        public int Quality { get; private set; }

        public TextureFormat Format { get; private set; }

        public int TotalFrames => RawFrames.Length;

        public string Path { get; internal set; }

        private GifFrame[] RawFrames { get; set; }

        private Texture2D[] Frames { get; set; }

        #endregion

        #region Constructors

        internal GifTexture(int width, int height,
            int fps, int repeat,
            int quality, TextureFormat format,
            GifFrame[] frames)
        {
            Assert.IsNotNullOrEmpty(frames, nameof(frames));

            // Set properties
            Name            = null;
            Width           = width;
            Height          = height;
            FramesPerSecond = fps;
            Repeat          = repeat;
            Quality         = quality;
            Format          = format;
            RawFrames       = frames;
            Path            = null;
        }

        #endregion

        #region Public methods

        public Texture2D GetThumbnail()
        {
            EnsureFramesAreCached();

            return Frames[0];
        }

        public Texture2D GetFrameAt(int index)
        {
            EnsureFramesAreCached();

            return Frames[index];
        }

        internal GifFrame[] ToGifFrames() => RawFrames;

        #endregion

        #region Private methods

        private void EnsureFramesAreCached()
        {
            // Check whether cache is available
            if (Frames != null) return;

            // Create textures
            int     frameCount      = TotalFrames;
            var     frameTextures   = new Texture2D[frameCount];
            for (int iter = 0; iter < frameCount; iter++)
            {
                // Create new texture with corresponding data
                var     newTexture  = new Texture2D(Width, Height, Format, false)
                {
                    wrapMode        = TextureWrapMode.Clamp,
			        filterMode      = FilterMode.Bilinear,
                    anisoLevel      = 0,
                    hideFlags       = HideFlags.HideAndDontSave,
                };
                newTexture.SetPixels32(RawFrames[iter].Data);

                // Add to array
                frameTextures[iter] = newTexture;
            }
            Frames  = frameTextures;
        }

        #endregion

        #region IEnumerable implementation

        public IEnumerator<Texture2D> GetEnumerator()
        {
            EnsureFramesAreCached();

            return ((IEnumerable<Texture2D>)Frames).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion
    }
}