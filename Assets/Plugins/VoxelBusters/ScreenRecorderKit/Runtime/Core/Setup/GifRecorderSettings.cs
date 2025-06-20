﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VoxelBusters.CoreLibrary;

using ThreadPriority = System.Threading.ThreadPriority;

namespace VoxelBusters.ScreenRecorderKit
{
    [System.Serializable]
    public class GifRecorderSettings : SettingsPropertyGroup
    {
		#region Fields

        [SerializeField, Min(8)]
		private		int						m_width;
		[SerializeField, Min(8)]
		private		int						m_height;
		[SerializeField]
		private		bool					m_autoAspect;
		[SerializeField, Range(1, 30)]
		private		int						m_framePerSecond;
		[SerializeField, Min(-1)]
		private		int						m_repeat;
		[SerializeField, Range(1, 100)]
		private		int						m_quality;
		[SerializeField, Min(0.1f)]
		private		float					m_bufferSize;
		[SerializeField]
		private		ThreadPriority			m_threadPriority;

		[SerializeField]
		private		GiphyServiceSettings	m_giphyService;

        #endregion

        #region Properties

		public int Width
		{
			get => m_width;
			private set => m_width = value;
		}

		public int Height
		{
			get => m_height;
			private set => m_height = value;
		}

		public bool AutoAspect
		{
			get => m_autoAspect;
			private set => m_autoAspect	= value;
		}

		public int FramePerSecond
		{
			get => m_framePerSecond;
			private set => m_framePerSecond	= value;
		}

		public int Repeat
		{
			get => m_repeat;
			private set => m_repeat	= value;
		}

		public int Quality
		{
			get => m_quality;
			private set => m_quality	= value;
		}

		public float BufferSize
		{
			get => m_bufferSize;
			private set => m_bufferSize	= value;
		}

		public int MaxFrameCount => Mathf.RoundToInt(FramePerSecond * BufferSize);

		public float TimePerFrame => 1f / FramePerSecond;

		public ThreadPriority ThreadPriority
		{
			get => m_threadPriority;
			private set => m_threadPriority = value;
		}

		public GiphyServiceSettings GiphyService
		{
			get => m_giphyService;
			private set => m_giphyService = value;
		}

		#endregion

		#region Constructors

		public GifRecorderSettings(bool isEnabled = true, int width = 320,
			int height = 200, bool autoAspect = true,
			int fps = 15, int repeat = 0,
			int quality = 15, float bufferSize = 3f,
			ThreadPriority threadPriority = ThreadPriority.BelowNormal, GiphyServiceSettings giphyService = null)
			: base(name: "Gif Recorder Settings", isEnabled: isEnabled)
		{
			// Set properties
			Width					= width;
			Height					= height;
			AutoAspect				= autoAspect;
			FramePerSecond			= fps;
			Repeat					= repeat;
			Quality					= quality;
			BufferSize				= bufferSize;
			ThreadPriority			= threadPriority;
			GiphyService			= giphyService;
		}

		#endregion
    }
}