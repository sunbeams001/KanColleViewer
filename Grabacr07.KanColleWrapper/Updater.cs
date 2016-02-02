using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Xml.Linq;
using System.Net;
using System.Diagnostics;
using Grabacr07.KanColleWrapper.Models;

namespace Grabacr07.KanColleWrapper
{
	public class Updater
	{
		private static readonly string directory = Path.Combine(KanColleClient.Directory, "Translations");
		private static readonly string tmpDirectory = Path.Combine(directory, "tmp");

		private XDocument VersionXML;

		/// <summary>
		/// Loads the version XML file from a remote URL. This houses all current online version info.
		/// </summary>
		/// <param name="UpdateURL">String URL to the version XML file.</param>
		/// <returns>True: Successful, False: Failed</returns>
		public bool LoadVersion(string UpdateURL)
		{
			return LoadVersion(UpdateURL, UpdateURL);
		}

		public bool LoadVersion(string UpdateURL, string UpdateTransURL)
		{
			try
			{
				VersionXML = XDocument.Load(UpdateURL);
				if (VersionXML == null) return false;
				if (UpdateURL != UpdateTransURL)
				{
					XDocument TransVersionXML = XDocument.Load(UpdateTransURL);
					if (TransVersionXML != null)
					{
						IEnumerable<XElement> Versions = VersionXML.Root.Descendants("Item");
						foreach (XElement node in TransVersionXML.Root.Elements("Item"))
						{
							var oldNode = Versions.Where(x => x.Element("Name").Value.Equals(node.Element("Name").Value)).FirstOrDefault();
							if (oldNode != null)
							{
								oldNode.ReplaceWith(node);
							}
							else
							{
								VersionXML.Root.Add(node);
                            }
						}
					}
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex);
				return false;
			}
			return true;
		}

		private int UpdateFile(bool checkVersion, WebClient client, string culture, string path, TranslationType translationType, string version)
		{
			if (checkVersion && !this.IsOnlineVersionGreater(translationType, version)) return 0;
			var file = Path.Combine(directory, culture, path);
			var tmpFile = Path.Combine(tmpDirectory, path);
			var url = this.GetOnlineVersion(translationType, true);
			if (culture.Length > 0) url = new Uri(new Uri(url), "./" + culture + "/" + path).ToString();
			client.DownloadFile(url, tmpFile);
			try
			{
				if (File.Exists(file)) File.Delete(file);
				File.Move(tmpFile, file);
				return 1;
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex);
				return -1;
			}
		}

		/// <summary>
		/// Updates any translation files that differ from that found online.
		/// </summary>
		/// <param name="translationsRef">Link to the translation engine to obtain current translation versions.</param>
		/// <param name="culture">Language version to download</param>
		/// <param name="checkVersion"></param>
		/// <returns>Returns a state code depending on how it ran. [-1: Error, 0: Nothing to update, 1: Update Successful]</returns>
		public int UpdateTranslations(Translations translationsRef, string culture, bool checkVersion = true)
		{
			culture = culture == null || culture == "en-US" || culture == "en" || culture == "ja-JP" || culture == "ja" ? "" : culture;

			using (var client = new WebClient())
			{
				var updated = false;
				var error = false;

				try
				{
					var directoryCulture = Path.Combine(directory, culture);
					if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
					if (!Directory.Exists(directoryCulture)) Directory.CreateDirectory(directoryCulture);
					if (!Directory.Exists(tmpDirectory)) Directory.CreateDirectory(tmpDirectory);

					var r = this.UpdateFile(checkVersion, client, culture, "Equipment.xml", TranslationType.Equipment, translationsRef.EquipmentVersion);
					if (r == 1) updated = true;
					if (r == -1) error = true;
					r = this.UpdateFile(checkVersion, client, culture, "Operations.xml", TranslationType.Operations, translationsRef.OperationsVersion);
					if (r == 1) updated = true;
					if (r == -1) error = true;
					r = this.UpdateFile(checkVersion, client, culture, "Quests.xml", TranslationType.Quests, translationsRef.QuestsVersion);
					if (r == 1) updated = true;
					if (r == -1) error = true;
					r = this.UpdateFile(checkVersion, client, culture, "Ships.xml", TranslationType.Ships, translationsRef.ShipsVersion);
					if (r == 1) updated = true;
					if (r == -1) error = true;
					r = this.UpdateFile(checkVersion, client, culture, "ShipTypes.xml", TranslationType.ShipTypes, translationsRef.ShipTypesVersion);
					if (r == 1) updated = true;
					if (r == -1) error = true;
					r = this.UpdateFile(checkVersion, client, culture, "Expeditions.xml", TranslationType.Expeditions, translationsRef.ExpeditionsVersion);
					if (r == 1) updated = true;
					if (r == -1) error = true;
					r = this.UpdateFile(checkVersion, client, "", "Data.xml", TranslationType.Data, translationsRef.DataVersion);
					if (r == 1) updated = true;
					if (r == -1) error = true;
				}
				catch (Exception ex)
				{
					Debug.WriteLine(ex);
					error = true;
				}

				if (Directory.Exists(tmpDirectory)) Directory.Delete(tmpDirectory);

				return updated ? 1 : error ? -1 : 0;
			}
		}

		/// <summary>
		/// Uses the downloaded Version XML document to return a specific version number as a string.
		/// </summary>
		/// <param name="Type">Translation file type. Can also be for the App itself.</param>
		/// <param name="bGetURL">If true, returns the URL of the online file instead of the version.</param>
		/// <returns>String value of either the Version or URL to the file.</returns>
		public string GetOnlineVersion(TranslationType Type, bool bGetURL = false)
		{
			if (VersionXML == null)
				return "";

			IEnumerable<XElement> Versions = VersionXML.Root.Descendants("Item");
			string ElementName = !bGetURL ? "Version" : "URL";

			try
			{
				switch (Type)
				{
					case TranslationType.App:
						return Versions.Where(x => x.Element("Name").Value.Equals("App")).FirstOrDefault().Element(ElementName).Value;
					case TranslationType.Equipment:
						return Versions.Where(x => x.Element("Name").Value.Equals("Equipment")).FirstOrDefault().Element(ElementName).Value;
					case TranslationType.Operations:
					case TranslationType.OperationSortie:
					case TranslationType.OperationMaps:
						return Versions.Where(x => x.Element("Name").Value.Equals("Operations")).FirstOrDefault().Element(ElementName).Value;
					case TranslationType.Quests:
					case TranslationType.QuestDetail:
					case TranslationType.QuestTitle:
						return Versions.Where(x => x.Element("Name").Value.Equals("Quests")).FirstOrDefault().Element(ElementName).Value;
					case TranslationType.Ships:
						return Versions.Where(x => x.Element("Name").Value.Equals("Ships")).FirstOrDefault().Element(ElementName).Value;
					case TranslationType.ShipTypes:
						return Versions.Where(x => x.Element("Name").Value.Equals("ShipTypes")).FirstOrDefault().Element(ElementName).Value;
					case TranslationType.Expeditions:
					case TranslationType.ExpeditionDetail:
					case TranslationType.ExpeditionTitle:
						return Versions.Where(x => x.Element("Name").Value.Equals("Expeditions")).FirstOrDefault().Element(ElementName).Value;
					case TranslationType.Data:
						return Versions.Where(x => x.Element("Name").Value.Equals("Data")).FirstOrDefault().Element(ElementName).Value;
				}
			}
			catch
			{
				return "";
			}
			return "";
		}

		/// <summary>
		/// Conditional function to determine whether the supplied version is greater than the one found online.
		/// </summary>
		/// <param name="Type">Translation file type. Can also be for the App itself.</param>
		/// <param name="LocalVersionString">Version string of the local file to check against</param>
		/// <returns></returns>
		public bool IsOnlineVersionGreater(TranslationType Type, string LocalVersionString)
		{
			if (VersionXML == null)
				return false;

			IEnumerable<XElement> Versions = VersionXML.Root.Descendants("Item");
			string ElementName = "Version";
			Version LocalVersion = new Version(LocalVersionString);

			try
			{
				switch (Type)
				{
					case TranslationType.App:
						return LocalVersion.CompareTo(new Version(Versions.Where(x => x.Element("Name").Value.Equals("App")).FirstOrDefault().Element(ElementName).Value)) < 0;
					case TranslationType.Equipment:
						return LocalVersion.CompareTo(new Version(Versions.Where(x => x.Element("Name").Value.Equals("Equipment")).FirstOrDefault().Element(ElementName).Value)) < 0;
					case TranslationType.Operations:
					case TranslationType.OperationSortie:
					case TranslationType.OperationMaps:
						return LocalVersion.CompareTo(new Version(Versions.Where(x => x.Element("Name").Value.Equals("Operations")).FirstOrDefault().Element(ElementName).Value)) < 0;
					case TranslationType.Quests:
					case TranslationType.QuestDetail:
					case TranslationType.QuestTitle:
						return LocalVersion.CompareTo(new Version(Versions.Where(x => x.Element("Name").Value.Equals("Quests")).FirstOrDefault().Element(ElementName).Value)) < 0;
					case TranslationType.Ships:
						return LocalVersion.CompareTo(new Version(Versions.Where(x => x.Element("Name").Value.Equals("Ships")).FirstOrDefault().Element(ElementName).Value)) < 0;
					case TranslationType.ShipTypes:
						return LocalVersion.CompareTo(new Version(Versions.Where(x => x.Element("Name").Value.Equals("ShipTypes")).FirstOrDefault().Element(ElementName).Value)) < 0;
					case TranslationType.Expeditions:
					case TranslationType.ExpeditionDetail:
					case TranslationType.ExpeditionTitle:
						return LocalVersion.CompareTo(new Version(Versions.Where(x => x.Element("Name").Value.Equals("Expeditions")).FirstOrDefault().Element(ElementName).Value)) < 0;
					case TranslationType.Data:
						return LocalVersion.CompareTo(new Version(Versions.Where(x => x.Element("Name").Value.Equals("Data")).FirstOrDefault().Element(ElementName).Value)) < 0; ;
				}
			}
			catch
			{
				return false;
			}

			return false;
		}

	}
}
