/*
 * Copyright (c) 2015 Thomas Hourdel
 *
 * This software is provided 'as-is', without any express or implied
 * warranty. In no event will the authors be held liable for any damages
 * arising from the use of this software.
 *
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 * 
 *    1. The origin of this software must not be misrepresented; you must not
 *    claim that you wrote the original software. If you use this software
 *    in a product, an acknowledgment in the product documentation would be
 *    appreciated but is not required.
 * 
 *    2. Altered source versions must be plainly marked as such, and must not be
 *    misrepresented as being the original software.
 * 
 *    3. This notice may not be removed or altered from any source
 *    distribution.
 */

using UnityEngine;
using System;
using System.Collections.Generic;
using System.Threading;
using VoxelBusters.CoreLibrary;

using ThreadPriority = System.Threading.ThreadPriority;

namespace VoxelBusters.ScreenRecorderKit.GifRecorderCore
{
	internal sealed class GifExportOperation
	{
        #region Static Properties

		private static int WorkerId { get; set; } = 1;

        #endregion

        #region Fields

		private		int				m_id;

		private		GifTexture		m_texture;

		private		GifEncoder		m_encoder;

		private		string			m_savePath;

        private		Thread			m_thread;

        #endregion

        #region Properties

		public float Progress { get; private set; }

        #endregion

        #region Delegates

        public delegate void ProgressDelegate(float progress);

        #endregion

        #region Events

        public event CompletionCallback OnExportComplete;

		public event ProgressDelegate OnExportProgress;

        #endregion

        #region Constructors

        public GifExportOperation(GifEncoder encoder, GifTexture texture, string savePath, ThreadPriority priority)
		{
			// Set properties
			m_id			= WorkerId++;
			m_encoder		= encoder;
			m_texture		= texture;
			m_savePath		= savePath;
            m_thread		= new Thread(Run)
            {
                Priority	= priority
            };
            Progress		= 0f;
		}

        #endregion

        #region Public methods

        public void Start()
		{
			m_thread.Start();
		}

        #endregion

        #region Private methods

        private void Run()
		{
			// Prepare encoder
			m_encoder.Start(m_savePath);

			// Serialize frames using encoder
			int		width		= m_texture.Width;
			int		height		= m_texture.Height;
			int		frameCount	= m_texture.TotalFrames;
			var		gifFrames	= m_texture.ToGifFrames();
			for (int iter = 0; iter < frameCount; iter++)
			{
				var		frame	= gifFrames[iter];
				m_encoder.AddFrame(frame);

				// Send progres event data
				Progress		= (float)iter / (float)frameCount;
				OnExportProgress?.Invoke(Progress);
			}
			m_encoder.Finish();

			// Send completion event
			OnExportComplete?.Invoke(true, null);
		}

        #endregion
    }
}
