using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
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
		private const string logTimestampFormat = "yyyy-MM-dd HH:mm:ss";

		// TODO: extend Organization, etc. with practice info instead (can be used in Overview view as well)
		private int fleetInPractice;

		public bool EnableLogging { private get; set; }

		public static readonly string Directory = Path.Combine(KanColleClient.Directory, "Logs");

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
			public readonly string Parameters;
			public readonly string FileName;

			public LogTypeInfo(string parameters, string fileName)
			{
				this.Parameters = parameters;
				this.FileName = fileName;
			}
		}

		public static readonly Dictionary<LogType, LogTypeInfo> LogParameters =
			new Dictionary<LogType, LogTypeInfo>
			{
				{
					LogType.BuildItem, new LogTypeInfo("Date,Result,Secretary,Secretary level,Fuel,Ammo,Steel,Bauxite",
													   "BuildItemLog.csv")
				},
				{
					LogType.BuildShip, new LogTypeInfo("Date,Result,Secretary,Secretary level,Fuel,Ammo,Steel,Bauxite,DevMats",
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
					LogType.Expedition, new LogTypeInfo("Date,Expedition,Result,HQExp,Fuel,Ammo,Steel,Bauxite,ItemFlags", // (,Ship,Level,Condition,HP,Fuel,Ammo,Exp,Drums)+
														"Expedition.csv")
				},
				{
					LogType.Levels, new LogTypeInfo("Date,Name,Level,ID,Exp",
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
			proxy.api_req_practice_battle.TryParse().Subscribe(x => this.fleetInPractice = int.Parse(x.Request["api_deck_id"]));
			proxy.api_req_practice_battle_result.TryParse <kcsapi_practice_battle_result>().Subscribe(x => this.Levels(x.Data));
			proxy.api_req_sortie_battleresult.TryParse<kcsapi_battleresult>().Subscribe(x => this.Levels(x.Data));
			proxy.api_req_combined_battle_battleresult.TryParse<kcsapi_combined_battle_battleresult>().Subscribe(x => this.Levels(x.Data));
			proxy.api_req_mission_result.TryParse<kcsapi_mission_result>().Subscribe(x => this.Levels(x.Data, x.Request));

			proxy.api_req_mission_result.TryParse<kcsapi_mission_result>().Subscribe(x => this.Expedition(x.Data, x.Request));
		}

		private void Expedition(kcsapi_mission_result res, NameValueCollection req)
		{
			try
			{
				var fleet = KanColleClient.Current.Homeport.Organization.Fleets[int.Parse(req["api_deck_id"])];

				var args = new List<object>();

				args.AddRange(new object[] {
					fleet.Expedition.Id, // Expedition
					res.api_clear_result == 2 ? "GS" : res.api_clear_result == 1 ? "NS" : "Fail", // Result
					res.api_get_exp, // HQExp
					string.Join(",", res.api_get_material), // Fuel,Ammunition,Steel,Bauxite
					string.Join("-", res.api_useitem_flag) // ItemFlags
				});

				for (var i = 0; i < fleet.Ships.Length; ++i)
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
			catch (Exception)
			{
				// ignored
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
			this.dockid = int.Parse(req["api_kdock_id"]);
			this.shipmats[0] = int.Parse(req["api_item1"]);
			this.shipmats[1] = int.Parse(req["api_item2"]);
			this.shipmats[2] = int.Parse(req["api_item3"]);
			this.shipmats[3] = int.Parse(req["api_item4"]);
			this.shipmats[4] = int.Parse(req["api_item5"]);
		}

		private void KDock(IEnumerable<kcsapi_kdock> docks)
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

		// Ships
		private void Levels(IReadOnlyList<int> exps, Fleet fleet)
		{
			for (var i = 0; i < fleet.Ships.Length; ++i)
			{
				var ship = fleet.Ships[i];
				int exp = exps[i], currExp = ship.Exp, newExp = currExp + exp;
				var lvl = Ship.ExpToLevel(newExp);
				if (lvl > 4 &&
					ship.ExpForNextLevel != 0 &&
					(exp >= ship.ExpForNextLevel || newExp / 10000 > currExp / 10000 && newExp - Ship.ExpTable[lvl] >= 10000 && Ship.ExpTable[lvl + 1] - newExp >= 10000))
					this.Log(LogType.Levels, ship.Info.Name, lvl, ship.Id, newExp);
			}
		}

		// HQ
		private void Levels(int exp)
		{
			int currExp = KanColleClient.Current.Homeport.Admiral.Experience, newExp = currExp + exp;
			var expForNexeLevel = KanColleClient.Current.Homeport.Admiral.ExperienceForNexeLevel;
			var lvl = KanColleClient.Current.Homeport.Admiral.Level;
			lvl = lvl < 99 ? Ship.ExpToLevel(newExp) : exp >= expForNexeLevel ? lvl + 1 : lvl;
			if (expForNexeLevel != 0 &&
				(exp >= expForNexeLevel || newExp / 10000 > currExp / 10000 && (lvl >= 99 || newExp - Ship.ExpTable[lvl] >= 10000 && Ship.ExpTable[lvl + 1] - newExp >= 10000)))
				this.Log(LogType.Levels, "HQ", lvl, "", newExp);
		}

		private void Levels(kcsapi_practice_battle_result pr)
		{
			try
			{
				this.Levels(pr.api_get_ship_exp.Skip(1).ToArray(), KanColleClient.Current.Homeport.Organization.Fleets[this.fleetInPractice]);
				this.Levels(pr.api_get_exp);
			}
			catch (Exception)
			{
				// ignored
			}
		}

		private void Levels(kcsapi_battleresult br)
		{
			try
			{
				this.Levels(br.api_get_ship_exp.Skip(1).ToArray(), KanColleClient.Current.Homeport.Organization.GetFleetInSortie());
				this.Levels(br.api_get_exp);
			}
			catch (Exception)
			{
				// ignored
			}
		}

		private void Levels(kcsapi_combined_battle_battleresult br)
		{
			try
			{
				this.Levels(br.api_get_ship_exp.Skip(1).ToArray(), KanColleClient.Current.Homeport.Organization.Fleets[1]);
				this.Levels(br.api_get_ship_exp_combined.Skip(1).ToArray(), KanColleClient.Current.Homeport.Organization.Fleets[2]);
				this.Levels(br.api_get_exp);
			}
			catch (Exception)
			{
				// ignored
			}
		}

		private void Levels(kcsapi_mission_result br, NameValueCollection req)
		{
			try
			{
				this.Levels(br.api_get_ship_exp, KanColleClient.Current.Homeport.Organization.Fleets[int.Parse(req["api_deck_id"])]);
				this.Levels(br.api_get_exp);
			}
			catch (Exception)
			{
				// ignored
			}
		}

		private void BattleResult(kcsapi_battleresult br)
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

			var logPath = CreateLogFile(type);

			if (string.IsNullOrEmpty(logPath))
				return;

			using (var w = File.AppendText(logPath))
			{
				w.WriteLine(DateTime.Now.ToString(logTimestampFormat) + "," + string.Join(",", args));
			}
		}

		private static string CreateLogFile(LogType type)
		{
			try
			{
				var info = LogParameters[type];
				var fullPath = Path.Combine(Directory, info.FileName);

				if (!System.IO.Directory.Exists(Directory))
					System.IO.Directory.CreateDirectory(Directory);

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
