#if UNITY_ANDROID
using System;
using UnityEditor;
using System.Collections.Generic;
using System.Xml;
using VoxelBusters.CoreLibrary;

namespace VoxelBusters.ScreenRecorderKit.Editor.Build.Android
{
    /// <summary>
    /// Replay Kit Dependencies for Cross Platform Replay Kit.
    /// </summary>
    [InitializeOnLoad]
    public class AndroidDependenciesGenerator
    {
		#region Constants

		/// <summary>
		/// The name of your plugin.  This is used to create a settings file
		/// which contains the dependencies specific to your plugin.
		/// </summary>
		private		const	string kDependencyName 		        = "ScreenRecorderKitDependencies.xml";

        private		const	string kAndroidXLibsVersionString	= "1.3.0+";

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes static members of the <see cref="AndroidDependenciesGenerator"/> class.
		/// </summary>
		static AndroidDependenciesGenerator()
		{
			EditorApplication.update -= Update;
			EditorApplication.update += Update;
			Create();
		}

		#endregion

		#region Static methods

        public static void Create()
		{
			string	path	= IOServices.CombinePath($"{AssetConstants.EditorExtrasPath}",$"{kDependencyName}");

			CreateInternal(path);
		}

		#endregion

		#region Private static methods

        private static void Update()
		{
			if (!ScreenRecorderKitSettingsEditorUtility.SettingsExists) return;

			ScreenRecorderKitSettings.Instance.OnSettingsUpdated	-= Create;
			ScreenRecorderKitSettings.Instance.OnSettingsUpdated	+= Create;

			//Unregister
			EditorApplication.update -= Update;
		}

		private static void CreateInternal(string path)
		{
            // Settings
            var		settings		= new XmlWriterSettings
            {
                Encoding			= new System.Text.UTF8Encoding(true),
                ConformanceLevel	= ConformanceLevel.Document,
                Indent				= true,
                NewLineOnAttributes = true,
                IndentChars			= "\t"
            };

            // Generate and write dependecies
            using (var xmlWriter = XmlWriter.Create(path, settings))
			{
				xmlWriter.WriteStartDocument();
				{
                    xmlWriter.WriteComment("DONT MODIFY HERE. AUTO GENERATED DEPENDENCIES FROM ScreenRecorderKit's AndroidDependenciesGenerator.cs.");

					xmlWriter.WriteStartElement("dependencies");
					{
						xmlWriter.WriteStartElement ("androidPackages");
						{
                            xmlWriter.WriteComment("Dependency added for using AndroidX Libraries");
                            var		androidxCoreDependency	= new AndroidDependency("androidx.core", "core", kAndroidXLibsVersionString);
                            WritePackageDependency(xmlWriter, androidxCoreDependency);

                        }
						xmlWriter.WriteEndElement();
					}
					xmlWriter.WriteEndElement();
				}
				xmlWriter.WriteEndDocument();
			}
		}

		private static void WritePackageDependency(XmlWriter xmlWriter, AndroidDependency dependency)
		{
			xmlWriter.WriteStartElement ("androidPackage");
			{
				xmlWriter.WriteAttributeString ("spec", String.Format("{0}:{1}:{2}", dependency.Group, dependency.Artifact, dependency.Version));

				var		packageIDs		= dependency.PackageIDs;
				if (packageIDs != null) 
				{
					xmlWriter.WriteStartElement ("androidSdkPackageIds");
					{
						foreach(string _each in packageIDs)
						{
							xmlWriter.WriteStartElement ("androidSdkPackageId");
							{
								xmlWriter.WriteString (_each);
							}
							xmlWriter.WriteEndElement ();
						}
					}
					xmlWriter.WriteEndElement ();
				}

                var		repositories	= dependency.Repositories;
                if (repositories != null) 
                {
                    xmlWriter.WriteStartElement ("repositories");
                    {
                        foreach(string _each in repositories)
                        {
                            xmlWriter.WriteStartElement ("repository");
                            {
                                xmlWriter.WriteString (_each);
                            }
                            xmlWriter.WriteEndElement ();
                        }
                    }
                    xmlWriter.WriteEndElement ();
                }

			}
			xmlWriter.WriteEndElement ();
		}

		#endregion
    }

    public class AndroidDependency
	{
		private string 	m_group;
		private string 	m_artifact;
		private string	m_version;

		private	List<string>	m_packageIDs;
        private List<string>    m_respositories;

        public string Group => m_group;

        public string Artifact => m_artifact;

        public string Version => m_version;

        public List<string> PackageIDs => m_packageIDs;

        public List<string> Repositories => m_respositories;

        public AndroidDependency(string _group, string _artifact, string _version)
		{
			m_group = _group;
			m_artifact = _artifact;
			m_version = _version;
		}

		public void AddPackageID(string _packageID)
		{
			if (m_packageIDs == null)
				m_packageIDs = new List<string>();


			m_packageIDs.Add(_packageID);
		}

        public void AddRepository(string _repository)
        {
            if (m_respositories == null)
                m_respositories = new List<string>();


            m_respositories.Add(_repository);
        }
	}
}
#endif