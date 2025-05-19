using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VoxelBusters.ScreenRecorderKit
{
    [System.Serializable]
    public class GiphyServiceSettings
    {
        #region Fields
            
		[SerializeField]
		private		string		m_apiKey;
		[SerializeField]
		private		string		m_debugApiKey;
		[SerializeField]
		private		string		m_username;

        #endregion

        #region Properties

		public string ApiKey
		{
			get => m_apiKey;
			private set => m_apiKey = value;
		}

		public string DebugApiKey
		{
			get => m_debugApiKey;
			private set => m_debugApiKey = value;
		}

		public string Username
		{
			get => m_username;
			private set => m_username = value;
		}

        #endregion

		#region Constructors

		public GiphyServiceSettings(string apiKey = null, string debugApiKey = null,
			string username = null)
		{
			// Set properties
			ApiKey			= apiKey;
			DebugApiKey		= debugApiKey;
			Username		= username;
		}

		#endregion
    }
}