using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Grabacr07.KanColleWrapper.Internal;
using Grabacr07.KanColleWrapper.Models;
using Grabacr07.KanColleWrapper.Models.Raw;
using Livet;

namespace Grabacr07.KanColleWrapper
{
	public class Logger : NotificationObject
	{
		private bool waitingForShip;
		private int dockid;
		private readonly int[] shipmats;
		private readonly int[] mats;
		private readonly string LogTimestampFormat = "yyyy-MM-dd HH:mm:ss";

		// TODO: extend Organization, etc. with practice info instead (can be used in Overview view as well)
		private int FleetInPractice;

		public bool EnableLogging { get; set; }

		// ReSharper disable once AssignNullToNotNullAttribute
		public static string LogFolder = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "Logs");

		public enum LogType
		{
			BuildItem,
			BuildShip,
			ShipDrop,
			Materials,
			Expedition,
			Levels
		};

		public struct LogTypeInfo
		{
			public string Parameters;
			public string FileName;

			public LogTypeInfo(string parameters, string fileName)
			{
				this.Parameters = parameters;
				this.FileName = fileName;
			}
		}

		public static Dictionary<LogType, LogTypeInfo> LogParameters =
			new Dictionary<LogType, LogTypeInfo>
			{
				{
					LogType.BuildItem, new LogTypeInfo("Date,Result,Secretary,Secretary level,Fuel,Ammo,Steel,Bauxite",
													   "BuildItemLog.csv")
				},
				{
					LogType.BuildShip, new LogTypeInfo("Date,Result,Secretary,Secretary level,Fuel,Ammo,Steel,Bauxite,# of Build Materials",
													   "BuildShipLog.csv")
				},
				{ 
					LogType.ShipDrop, new LogTypeInfo("Date,Result,Operation,Enemy Fleet,Rank", 
													   "ShipDropLog.csv") 
				},
				{ 
					LogType.Materials, new LogTypeInfo("Date,Fuel,Ammo,Steel,Bauxite,DevMats,Buckets,Flamethrowers,Screws",
													   "MaterialsLog.csv") 
				},
				{
					LogType.Expedition, new LogTypeInfo("Date,Expedition,Result,HQExp,Fuel,Ammunition,Steel,Bauxite,ItemFlags(,Ship,Level,Condition,HP,Fuel,Ammo,Exp,Drums)+",
														"Expedition.csv")
				},
				{
					LogType.Levels, new LogTypeInfo("Date,Name,Level",
													"Levels.csv")
				},
			};

		internal Logger(KanColleProxy proxy)
		{
			this.EnableLogging = KanColleClient.Current.Settings.EnableLogging;

			this.shipmats = new int[5];
			this.mats = new int[8];

			proxy.api_req_kousyou_createitem.TryParse<kcsapi_createitem>().Subscribe(x => this.CreateItem(x.Data, x.Request));
			proxy.api_req_kousyou_createship.TryParse<kcsapi_createship>().Subscribe(x => this.CreateShip(x.Request));

			proxy.api_get_member_kdock.TryParse<kcsapi_kdock[]>().Subscribe(x => this.KDock(x.Data));

			proxy.api_req_sortie_battleresult.TryParse<kcsapi_battleresult>().Subscribe(x => this.BattleResult(x.Data));
			proxy.api_req_combined_battle_battleresult.TryParse<kcsapi_combined_battle_battleresult>().Subscribe(x => this.BattleResult(x.Data));

			proxy.api_port.TryParse<kcsapi_port>().Subscribe(x => this.MaterialsHistory(x.Data));

			// TODO: add kcsapi_practice_battle
			proxy.api_req_practice_battle.TryParse().Subscribe(x => this.FleetInPractice = int.Parse(x.Request["api_deck_id"]));
			proxy.api_req_practice_battle_result.TryParse <kcsapi_practice_battle_result>().Subscribe(x => this.Levels(x.Data));

			proxy.api_req_mission_result.TryParse<kcsapi_mission_result>().Subscribe(x => this.Expedition(x.Data, x.Request));
		}

		private void Expedition(kcsapi_mission_result res, NameValueCollection req)
		{
			try
			{
				Fleet fleet = KanColleClient.Current.Homeport.Organization.Fleets[int.Parse(req["api_deck_id"])];

				List<object> args = new List<object>();

				args.AddRange(new object[] {
					fleet.Expedition.Id, // Expedition
					res.api_clear_result == 2 ? "GS" : res.api_clear_result == 1 ? "NS" : "Fail", // Result
					res.api_get_exp, // HQExp
					String.Join(",", res.api_get_material), // Fuel,Ammunition,Steel,Bauxite
					String.Join("-", res.api_useitem_flag) // ItemFlags
				});

				for (int i = 0; i < fleet.Ships.Length; ++i)
				{
					args.AddRange(new object[] {
						fleet.Ships[i].Info.Name, // Ship
						fleet.Ships[i].Level, // Level
						fleet.Ships[i].Condition, // Condition
						fleet.Ships[i].HP.Current, // HP
						fleet.Ships[i].Fuel.Current, // Fuel
						fleet.Ships[i].Bull.Current, // Ammo
						res.api_get_ship_exp[i], // Exp
						fleet.Ships[i].EquippedSlots.Count(slot => slot.Item.Info.Id == 75) // Drums
					});
				}

				this.Log(LogType.Expedition, args.ToArray());
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Logger.Expedition: {0}", ex);
			}
		}
		
		private void CreateItem(kcsapi_createitem item, NameValueCollection req)
		{
			try
			{
				this.Log(LogType.BuildItem,
						 item.api_create_flag == 1 ? KanColleClient.Current.Master.SlotItems[item.api_slot_item.api_slotitem_id].Name : "Penguin", //Result
						 KanColleClient.Current.Homeport.Organization.Fleets[1].Ships[0].Info.Name, //Secretary
						 KanColleClient.Current.Homeport.Organization.Fleets[1].Ships[0].Level, //Secretary Level
						 req["api_item1"], //Fuel
						 req["api_item2"], //Ammo
						 req["api_item3"], //Steel
						 req["api_item4"] //Bauxite
					);
			}
			catch (Exception)
			{
				// ignored
			}
		}

		private void CreateShip(NameValueCollection req)
		{
			this.waitingForShip = true;
			this.dockid = Int32.Parse(req["api_kdock_id"]);
			this.shipmats[0] = Int32.Parse(req["api_item1"]);
			this.shipmats[1] = Int32.Parse(req["api_item2"]);
			this.shipmats[2] = Int32.Parse(req["api_item3"]);
			this.shipmats[3] = Int32.Parse(req["api_item4"]);
			this.shipmats[4] = Int32.Parse(req["api_item5"]);
		}

		private void KDock(kcsapi_kdock[] docks)
		{
			try
			{
				foreach (var dock in docks.Where(dock => this.waitingForShip && dock.api_id == this.dockid))
				{
					this.Log(LogType.BuildShip,
							 KanColleClient.Current.Master.Ships[dock.api_created_ship_id].Name, //Result
							 KanColleClient.Current.Homeport.Organization.Fleets[1].Ships[0].Info.Name, //Secretary
							 KanColleClient.Current.Homeport.Organization.Fleets[1].Ships[0].Level, //Secretary Level
							 this.shipmats[0], //Fuel
							 this.shipmats[1], //Ammo
							 this.shipmats[2], //Steel
							 this.shipmats[3], //Bauxite
							 this.shipmats[4] //Materials
						);

					this.waitingForShip = false;
				}
			}
			catch (Exception)
			{
				this.waitingForShip = false;
			}
		}

		// TODO: log within levels too (e.g. each 1000 xp)
		// TODO: support for combined fleet
		// TODO: support for expeditions
		private void Levels(int[] exps, Fleet fleet)
		{
			int i = 0;
			foreach (int exp in exps)
				if (exp != -1)
				{
					Ship ship = fleet.Ships[i];
					if (ship.ExpForNextLevel != 0 && exp >= ship.ExpForNextLevel)
						this.Log(LogType.Levels, ship.Info.Name, Ship.ExpToLevel(ship.Exp + exp));
					++i;
				}
		}

		private void Levels(kcsapi_practice_battle_result pr)
		{
			try
			{
				Fleet fleet = KanColleClient.Current.Homeport.Organization.Fleets[FleetInPractice];
				Levels(pr.api_get_ship_exp, fleet);
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Logger.Levels: {0}", ex);
			}
		}

		private void BattleResult(kcsapi_battleresult br)
		{
			// Levels
			try
			{
				Fleet fleet = KanColleClient.Current.Homeport.Organization.GetFleetInSortie();
				Levels(br.api_get_ship_exp, fleet);
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Logger.Levels: {0}", ex);
			}

			// Drops
			try
			{
				if (br.api_get_ship != null)
				{
				this.Log(LogType.ShipDrop,
						 KanColleClient.Current.Translations.GetTranslation(br.api_get_ship.api_ship_name, TranslationType.Ships, br), //Result
						 KanColleClient.Current.Translations.GetTranslation(br.api_quest_name, TranslationType.OperationMaps, br), //Operation
						 KanColleClient.Current.Translations.GetTranslation(br.api_enemy_info.api_deck_name, TranslationType.OperationSortie, br), //Enemy Fleet
						 br.api_win_rank //Rank
					);
			}
			}
			catch (Exception)
			{
				// ignored
			}
		}

		private void BattleResult(kcsapi_combined_battle_battleresult br)
		{
			try
			{
				if (br.api_get_ship == null)
					return;

				this.Log(LogType.ShipDrop,
						 KanColleClient.Current.Translations.GetTranslation(br.api_get_ship.api_ship_name, TranslationType.Ships, br), //Result
						 KanColleClient.Current.Translations.GetTranslation(br.api_quest_name, TranslationType.OperationMaps, br), //Operation
						 KanColleClient.Current.Translations.GetTranslation(br.api_enemy_info.api_deck_name, TranslationType.OperationSortie, br), //Enemy Fleet
						 br.api_win_rank //Rank
					);
			}
			catch (Exception)
			{
				// ignored
			}
		}

		private void MaterialsHistory(kcsapi_port source)
		{
			try
			{
				if (source.api_material[0].api_value != this.mats[0] ||
					source.api_material[1].api_value != this.mats[1] ||
					source.api_material[2].api_value != this.mats[2] ||
					source.api_material[3].api_value != this.mats[3] ||
					source.api_material[4].api_value != this.mats[6] ||
					source.api_material[5].api_value != this.mats[5] ||
					source.api_material[6].api_value != this.mats[4] ||
					source.api_material[7].api_value != this.mats[7])
				{
					this.mats[0] = source.api_material[0].api_value;
					this.mats[1] = source.api_material[1].api_value;
					this.mats[2] = source.api_material[2].api_value;
					this.mats[3] = source.api_material[3].api_value;
					this.mats[6] = source.api_material[4].api_value;
					this.mats[5] = source.api_material[5].api_value;
					this.mats[4] = source.api_material[6].api_value;
					this.mats[7] = source.api_material[7].api_value;

					this.Log(LogType.Materials,
						this.mats[0], this.mats[1], this.mats[2], this.mats[3],
						this.mats[4], this.mats[5], this.mats[6], this.mats[7]);
				}
			}
			catch (Exception)
			{
				// ignored
			}
		}

		private void Log(LogType type, params object[] args)
		{
			if (!this.EnableLogging) return;

			string logPath = this.CreateLogFile(type);

			if (String.IsNullOrEmpty(logPath))
				return;

			using (var w = File.AppendText(logPath))
			{
				w.WriteLine(DateTime.Now.ToString(this.LogTimestampFormat) + "," + string.Join(",", args));
			}
		}

		private string CreateLogFile(LogType type)
		{
			try
			{
				var info = LogParameters[type];
				string fullPath = Path.Combine(LogFolder, info.FileName);

				if (!Directory.Exists(LogFolder))
					Directory.CreateDirectory(LogFolder);

				if (!File.Exists(fullPath))
					File.WriteAllText(fullPath, info.Parameters + Environment.NewLine, new UTF8Encoding(true));

				return fullPath;
			}
			catch (Exception)
			{
				return null;
			}
		}
	}
}
