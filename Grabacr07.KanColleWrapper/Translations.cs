using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Grabacr07.KanColleWrapper.Models;
using Grabacr07.KanColleWrapper.Models.Raw;
using Livet;

namespace Grabacr07.KanColleWrapper
{
	public class Translations : NotificationObject
	{
		private static readonly string directory = Path.Combine(KanColleClient.Directory, "Translations");

		private string currentCulture = "";

		private string shipsFile;
		private string shipTypesFile;
		private string equipmentFile;
		private string operationsFile;
		private string dataFile;
		private string questsFile;
		private string expeditionsFile;

		private XDocument shipsXml;
		private XDocument shipTypesXml;
		private XDocument equipmentXml;
		private XDocument operationsXml;
		private XDocument dataXml;
		private XDocument questsXml;
		private XDocument expeditionsXml;

		public bool EnableTranslations { get; set; }
		
		#region EquipmentVersion 変更通知プロパティ

		private string _EquipmentVersion;

		public string EquipmentVersion
		{
			get { return this._EquipmentVersion; }
			set
			{
				if (this._EquipmentVersion != value)
				{
					this._EquipmentVersion = value;
					this.RaisePropertyChanged();
				}
			}
		}

		#endregion

		#region OperationsVersion 変更通知プロパティ

		private string _OperationsVersion;

		public string OperationsVersion
		{
			get { return this._OperationsVersion; }
			set
			{
				if (this._OperationsVersion != value)
				{
					this._OperationsVersion = value;
					this.RaisePropertyChanged();
				}
			}
		}

		private string _DataVersion;
		public string DataVersion
		{
			get { return this._DataVersion; }
			set
			{
				if (this._DataVersion != value)
				{
					this._DataVersion = value;
					this.RaisePropertyChanged();
				}
			}
		}

		#endregion

		#region QuestsVersion 変更通知プロパティ

		private string _QuestsVersion;

		public string QuestsVersion
		{
			get { return this._QuestsVersion; }
			set
			{
				if (this._QuestsVersion != value)
				{
					this._QuestsVersion = value;
					this.RaisePropertyChanged();
				}
			}
		}

		#endregion

		#region ShipsVersion 変更通知プロパティ

		private string _ShipsVersion;

		public string ShipsVersion
		{
			get { return this._ShipsVersion; }
			set
			{
				if (this._ShipsVersion != value)
				{
					this._ShipsVersion = value;
					this.RaisePropertyChanged();
				}
			}
		}

		#endregion

		#region ShipTypesVersion 変更通知プロパティ

		private string _ShipTypesVersion;

		public string ShipTypesVersion
		{
			get { return this._ShipTypesVersion; }
			set
			{
				if (this._ShipTypesVersion != value)
				{
					this._ShipTypesVersion = value;
					this.RaisePropertyChanged();
				}
			}
		}

		#endregion

        #region ExpeditionsVersion 変更通知プロパティ

        private string _ExpeditionsVersion;

        public string ExpeditionsVersion
        {
            get { return this._ExpeditionsVersion; }
            set
            {
                if (this._ExpeditionsVersion != value)
                {
	                this._ExpeditionsVersion = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        #endregion

		private void LoadTranslations()
		{
			try
			{
				this.shipsFile = Path.Combine(directory, this.currentCulture, "Ships.xml");
				this.shipTypesFile = Path.Combine(directory, this.currentCulture, "ShipTypes.xml");
				this.equipmentFile = Path.Combine(directory, this.currentCulture, "Equipment.xml");
				this.operationsFile = Path.Combine(directory, this.currentCulture, "Operations.xml");
				this.questsFile = Path.Combine(directory, this.currentCulture, "Quests.xml");
				this.expeditionsFile = Path.Combine(directory, this.currentCulture, "Expeditions.xml");
				this.dataFile = Path.Combine(directory, "Data.xml");

				if (File.Exists(this.shipsFile)) this.shipsXml = XDocument.Load(this.shipsFile);
				if (File.Exists(this.shipTypesFile)) this.shipTypesXml = XDocument.Load(this.shipTypesFile);
				if (File.Exists(this.equipmentFile)) this.equipmentXml = XDocument.Load(this.equipmentFile);
				if (File.Exists(this.operationsFile)) this.operationsXml = XDocument.Load(this.operationsFile);
				if (File.Exists(this.questsFile)) this.questsXml = XDocument.Load(this.questsFile);
				if (File.Exists(this.expeditionsFile)) this.expeditionsXml = XDocument.Load(this.expeditionsFile);
				if (File.Exists(this.dataFile)) this.dataXml = XDocument.Load(this.dataFile);

				this.GetVersions();

				CompileData();
				dataXml = null;
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex);
			}
		}

		internal Translations()
		{
			LoadTranslations();
		}

		public void ChangeCulture(string culture)
		{
			this.currentCulture = culture == "en-US" || culture == "en" ? "" : culture == "ja-JP" || culture == "ja" ? "ja-JP" : culture;

			this.shipsXml = null;
			this.shipTypesXml = null;
			this.equipmentXml = null;
			this.operationsXml = null;
			this.questsXml = null;
			this.expeditionsXml = null;

			if (!this.EnableTranslations || this.currentCulture == "ja-JP")
			{
				this.RaisePropertyChanged("CurrentCulture");
				return;
			}

			this.LoadTranslations();

			this.RaisePropertyChanged("CurrentCulture");
		}

		private void GetVersions()
		{
			// ReSharper disable PossibleNullReferenceException
			try
			{
				this.ShipsVersion = this.shipsXml.Root.Attribute("Version").Value;
			}
			catch (NullReferenceException)
			{
				this.ShipsVersion = "0.0.0";
			}

			try
			{
				this.ShipTypesVersion = this.shipTypesXml.Root.Attribute("Version").Value;
			}
			catch (NullReferenceException)
			{
				this.ShipTypesVersion = "0.0.0";
			}

			try
			{
				this.EquipmentVersion = this.equipmentXml.Root.Attribute("Version").Value;
			}
			catch (NullReferenceException)
			{
				this.EquipmentVersion = "0.0.0";
			}

			try
			{
				this.OperationsVersion = this.operationsXml.Root.Attribute("Version").Value;
			}
			catch (NullReferenceException)
			{
				this.OperationsVersion = "0.0.0";
			}

			try
			{
				this.DataVersion = this.dataXml.Root.Attribute("Version").Value;
			}
			catch (NullReferenceException)
			{
				this.DataVersion = "0.0.0";
			}

			try
			{
				this.QuestsVersion = this.questsXml.Root.Attribute("Version").Value;
			}
			catch (NullReferenceException)
			{
				this.QuestsVersion = "0.0.0";
			}

			try
			{
				this.ExpeditionsVersion = this.expeditionsXml.Root.Attribute("Version").Value;
			}
			catch (NullReferenceException)
			{
				this.ExpeditionsVersion = "0.0.0";
			}
			// ReSharper restore PossibleNullReferenceException
		}

		private IEnumerable<XElement> GetTranslationList(TranslationType type)
		{
			switch(type)
			{
				case TranslationType.Ships:
					if (this.shipsXml != null) 
						return this.shipsXml.Descendants("Ship");
					break;
				case TranslationType.ShipTypes:
					if (this.shipTypesXml != null) 
						return this.shipTypesXml.Descendants("Type");
					break;
				case TranslationType.Equipment:
					if (this.equipmentXml != null) 
						return this.equipmentXml.Descendants("Item");
					break;
				case TranslationType.OperationMaps:
					if (this.operationsXml != null) 
						return this.operationsXml.Descendants("Map");
					break;
				case TranslationType.OperationSortie:
					if (this.operationsXml != null) 
						return this.operationsXml.Descendants("Sortie");
					break;
				case TranslationType.Quests:
				case TranslationType.QuestTitle:
				case TranslationType.QuestDetail:
					if (this.questsXml != null) 
						return this.questsXml.Descendants("Quest");
					break;
                case TranslationType.Expeditions:
                case TranslationType.ExpeditionTitle:
                case TranslationType.ExpeditionDetail:
                    if (this.expeditionsXml != null)
                        return this.expeditionsXml.Descendants("Expedition");
                    break;
			}

			return null;
		}

		private char[] mapEdges;

		private void CompileData()
		{
			// (6 worlds * 6 maps + 8 event maps) * max. 32 edges * 2 nodes
			mapEdges = new char[(6 * 6 + 8) * 32 * 2]; 
			try
			{
				var mapList = dataXml.Descendants("Map");
				foreach (var mapEl in mapList)
				{
					var code = int.Parse(mapEl.Element("Code")?.Value ?? "-1");
					if (code >= 0 && code < 6 * 6 + 8)
					{
						IEnumerable<XElement> edgeList = mapEl.Elements("Edge");
						foreach (var edgeEl in edgeList)
						{
							var apiNo = int.Parse(edgeEl.Element("ApiNo")?.Value ?? "-1");
							if (apiNo > 0 && apiNo <= 32)
							{
								char from = edgeEl.Element("From").Value[0];
								char to = edgeEl.Element("To").Value[0];
								mapEdges[2 * 32 * code + 2 * (apiNo - 1)] = from;
								mapEdges[2 * 32 * code + 2 * (apiNo - 1) + 1] = to;
							}
						}
					}
				}
			}
			catch { }
		}

		public Tuple<string, string> GetMapLabels(int world, int map, int path)
		{
			try
			{
				var code = (world - 1) * 6 + (map - 1);
				if (code >= 0 && code < 6 * 6 + 8 && path > 0 && path <= 32)
				{
					var from = mapEdges[2 * 32 * code + 2 * (path - 1)];
					var to = mapEdges[2 * 32 * code + 2 * (path - 1) + 1];
					if (from != 0 && to != 0)
					{
						return Tuple.Create(from == 'S' ? "Start" : from.ToString(), to.ToString());
					}
				}
			}
			catch { }
			return null;
		}

		public string GetQuestTranslation(int id)
		{
			try
			{
				IEnumerable<XElement> translationList = this.GetTranslationList(TranslationType.QuestTitle);
				foreach (var el in translationList)
				{
					if (int.Parse(el.Element("ID").Value) == id)
					{
						return el.Element("TR-Name").Value;
					}
				}
			}
			catch { }
			return null;
		}

		public string GetTranslation(string jpString, TranslationType type, Object rawData, int id = -1)
		{
			if (!this.EnableTranslations || this.currentCulture == "ja-JP")
				return jpString;

			try
			{
				IEnumerable<XElement> translationList = this.GetTranslationList(type);

				if (translationList == null)
				{
					return jpString;
				}

				string jpChildElement = "JP-Name";
				string trChildElement = "TR-Name";

				if (type == TranslationType.QuestDetail || type == TranslationType.ExpeditionDetail)
				{
					jpChildElement = "JP-Detail";
					trChildElement = "TR-Detail";
				}

				string translate = jpString;
				if (this.GetTranslation(jpString, translationList, jpChildElement, trChildElement, id, ref translate))
				{
					return translate;
				}
#if DEBUG
				Debug.WriteLine("Can't find Translation: {0,-20} {1}", jpString, id);
#endif
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex);
			}
			return jpString;
		}

		public bool GetTranslation(string jpString, IEnumerable<XElement> translationList, String jpChildElement, String trChildElement, int id , ref string translate)
		{
			// ReSharper disable once PossibleMultipleEnumeration
			IEnumerable<XElement> foundTranslation = translationList.Where(el =>
			{
				try
				{
					// ReSharper disable PossibleNullReferenceException
					if (el.Element(jpChildElement).Value.Equals(jpString)) return true;
					if (el.Attribute("mode") != null)
					{
						if (el.Attribute("mode").Value.Equals("suffix"))
						{
							int sl = el.Element(jpChildElement).Value.Length;
							if (jpString.Length > sl)
							{
								if (el.Element(jpChildElement).Value.Equals(jpString.Substring(jpString.Length - sl))) return true;
							}
						}
					}
					// ReSharper restore PossibleNullReferenceException
				}
				catch
				{
					return false;
				}
				return false;
			});

			bool foundWrongId = false;
			int n;
			foreach (XElement el in foundTranslation)
			{
				if (el.Attribute("mode") != null && !el.Attribute("mode").Value.Equals("normal"))
				{
					if (el.Attribute("mode").Value.Equals("suffix"))
					{
						try
						{
							// ReSharper disable PossibleNullReferenceException
							string t = jpString.Substring(0, jpString.Length - el.Element(jpChildElement).Value.Length);
							// ReSharper disable once PossibleMultipleEnumeration
							if (this.GetTranslation(t, translationList, jpChildElement, trChildElement, -1, ref t))
							{
								if ((el.Attribute("suffixType") != null) && el.Attribute("suffixType").Value.Equals("pre")) translate = el.Element(trChildElement).Value + t;
								else translate = t + el.Element(trChildElement).Value;
								return true;
							}
							// ReSharper restore PossibleNullReferenceException
						}
						catch (NullReferenceException)
						{
						}
					}
					continue;
				}

				try
				{
					// ReSharper disable PossibleNullReferenceException
					if (id >= 0)
					{
						if (!Int32.TryParse(el.Element("ID").Value, out n))
						{
							foundWrongId = true;
							translate = el.Element(trChildElement).Value;
						}
						else
						{
							if (id >= 0 && el.Element("ID") != null && Convert.ToInt32(el.Element("ID").Value) == id)
							{
								translate = el.Element(trChildElement).Value;
								return true;
							}
						}
					}
					else
					{
						translate = el.Element(trChildElement).Value;
						return true;
					}
					// ReSharper restore PossibleNullReferenceException
				}
				catch (NullReferenceException)
				{
				}
			}

			if (foundWrongId)
			{
				return true;
			}

			return false;
		}
	}
}
