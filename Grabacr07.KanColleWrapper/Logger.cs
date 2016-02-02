using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
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

		public enum LogType
		{
			BuildItem,
			BuildShip,
			ShipDrop,
			Materials,
			Expedition,
			Levels,
			Quests,
			Battle,
			Sortie
		};

		public struct LogTypeInfo
		{
			public readonly string Parameters;
			public readonly string FileName;

			public LogTypeInfo(string parameters, string fileName)
			{
				Parameters = parameters;
				FileName = fileName;
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
					LogType.ShipDrop, new LogTypeInfo("Date,Result,Map,Node,Enemy Fleet,Rank",
													   "ShipDropLog.csv") 
				},
				{ 
					LogType.Materials, new LogTypeInfo("Date,Fuel,Ammo,Steel,Bauxite,DevMats,Buckets,Builds,Screws",
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
				{
					LogType.Quests, new LogTypeInfo("Date,ID,Title",
													"Quests.csv")
				},
				{
					LogType.Battle, new LogTypeInfo("Date,Fleets,Map,Node,Rank,HQExp,MVP,Ships,Exps",
													"Battle.csv")
				},
				{
					LogType.Sortie, new LogTypeInfo("Date,Fleets,Map,From,To,Ships",
													"Sortie.csv")
				},
			};

		internal Logger(KanColleProxy proxy)
		{
			EnableLogging = KanColleClient.Current.Settings.EnableLogging;

			shipmats = new int[5];
			mats = new int[8];

			proxy.api_req_kousyou_createitem.TryParse<kcsapi_createitem>().Subscribe(x => CreateItem(x.Data, x.Request));

			proxy.api_req_kousyou_createship.TryParse<kcsapi_createship>().Subscribe(x => CreateShip(x.Request));

			proxy.api_get_member_kdock.TryParse<kcsapi_kdock[]>().Subscribe(x => KDock(x.Data));

			proxy.api_req_sortie_battleresult.TryParse<kcsapi_battleresult>().Subscribe(x => Drop(x.Data));
			proxy.api_req_combined_battle_battleresult.TryParse<kcsapi_combined_battle_battleresult>().Subscribe(x => Drop(x.Data));

			proxy.api_port.TryParse<kcsapi_port>().Subscribe(x => MaterialsHistory(x.Data));

			// TODO: add kcsapi_practice_battle
			proxy.api_req_practice_battle.TryParse().Subscribe(x => fleetInPractice = int.Parse(x.Request["api_deck_id"]));
			proxy.api_req_practice_battle_result.TryParse <kcsapi_practice_battle_result>().Subscribe(x => Levels(x.Data));
			proxy.api_req_sortie_battleresult.TryParse<kcsapi_battleresult>().Subscribe(x => Levels(x.Data));
			proxy.api_req_combined_battle_battleresult.TryParse<kcsapi_combined_battle_battleresult>().Subscribe(x => Levels(x.Data));
			proxy.api_req_mission_result.TryParse<kcsapi_mission_result>().Subscribe(x => Levels(x.Data, x.Request));

			proxy.api_req_mission_result.TryParse<kcsapi_mission_result>().Subscribe(x => Expedition(x.Data, x.Request));

			// TODO: kcsapi_quest_clearitemget
			proxy.api_req_quest_clearitemget.TryParse().Subscribe(x => Quests(x.Request));

			proxy.api_req_sortie_battleresult.TryParse<kcsapi_battleresult>().Subscribe(x => { Battle(x.Data); });
			proxy.api_req_sortie_battleresult.TryParse<kcsapi_combined_battle_battleresult>().Subscribe(x => { Battle(x.Data); });
			proxy.api_req_map_start.TryParse<kcsapi_map_start>().Subscribe(x => { Sortie(x); });
			proxy.api_req_map_next.TryParse<kcsapi_map_start>().Subscribe(x => { Sortie(x); });
		}

		private void Battle(kcsapi_battleresult data)
		{
			try
			{
				var fleet = KanColleClient.Current.Homeport.Organization.GetFleetInSortie();
				var ships = new List<string>();
				foreach (var ship in fleet.Ships)
				{
					ships.Add(ship.Info.Name);
				}
				string mvp = fleet.Ships[data.api_mvp - 1].Info.Name;
				var exps = string.Join(";", data.api_get_ship_exp.Where(exp => exp != -1).Select(exp => exp.ToString()));
				var world = fleet.SortieInfo.World;
				var map = fleet.SortieInfo.Map;
				var path = fleet.SortieInfo.Path;
				var isBoss = fleet.SortieInfo.IsNextBoss;
				var labels = KanColleClient.Current.Translations.GetMapLabels(world, map, path);
				Log(
					LogType.Battle,
					fleet.Name, // Fleets
					$"{world}-{map}", // Map
					$"{(labels != null ? $"{labels.Item2}" : $"{path}")}{(isBoss ? " (Boss)" : "")}", // Node
					data.api_win_rank, // Rank
					data.api_get_exp, // HQExp
					mvp, // MVP
					string.Join(";", ships), // Ships
					string.Join(";", exps) // Exps
				);
			}
			catch { }
		}

		private void Battle(kcsapi_combined_battle_battleresult data)
		{
			try
			{
				var fleets = KanColleClient.Current.Homeport.Organization.GetFleetsInSortie();
				var ships = new List<string>();
				var fleetNames = new List<string>();
				foreach (var fleet in fleets)
				{
					fleetNames.Add(fleet.Name);
					foreach (var ship in fleet.Ships)
					{
						ships.Add(ship.Info.Name);
					}
				}
				string mvp = "";
				try
				{
					if (data.api_mvp > 6)
					{
						mvp = fleets[1].Ships[data.api_mvp - 6 - 1].Info.Name;
					}
					else
					{
						mvp = fleets[0].Ships[data.api_mvp - 1].Info.Name;
					}
				}
				catch { }
				var exps = string.Join(";", data.api_get_ship_exp.Where(exp => exp != -1).Select(exp => exp.ToString())) +
					string.Join(";", data.api_get_ship_exp_combined.Where(exp => exp != -1).Select(exp => exp.ToString()));
				var world = fleets[0].SortieInfo.World;
				var map = fleets[0].SortieInfo.Map;
				var path = fleets[0].SortieInfo.Path;
				var isBoss = fleets[0].SortieInfo.IsNextBoss;
				var labels = KanColleClient.Current.Translations.GetMapLabels(world, map, path);
				Log(
					LogType.Battle,
					string.Join(";", fleetNames), // Fleets
					$"{world}-{map}", // Map
					$"{(labels != null ? $"{labels.Item2}" : $"{path}")}{(isBoss ? " (Boss)" : "")}", // Node
					data.api_win_rank, // Rank
					data.api_get_exp, // HQExp
					mvp, // MVP
					string.Join(";", ships), // Ships
					string.Join(";", exps) // Exps
				);
			}
			catch { }
		}

		// TODO: Material drops (kcsapi_map_next and api_itemget)
		private void Sortie(SvData<kcsapi_map_start> data)
		{
			try
			{
				var fleets = KanColleClient.Current.Homeport.Organization.GetFleetsInSortie();
				// Logger can get kcsapi_map_start notification earlier than Organization...
				int id;
				if ((fleets == null || fleets.Length == 0) && int.TryParse(data.Request["api_deck_id"], out id))
				{
					if (KanColleClient.Current.Homeport.Organization.Combined && id == 1)
					{
						fleets = new Fleet[] {
							KanColleClient.Current.Homeport.Organization.Fleets[1],
							KanColleClient.Current.Homeport.Organization.Fleets[2]
						};
					}
					else
					{
						fleets = new Fleet[] { KanColleClient.Current.Homeport.Organization.Fleets[id] };
					}
				}
				var ships = new List<string>();
				var fleetNames = new List<string>();
				foreach (var fleet in fleets)
				{
					fleetNames.Add(fleet.Name);
					foreach (var ship in fleet.Ships)
					{
						ships.Add(ship.Info.Name);
					}
				}
				var world = data.Data.api_maparea_id;
				var map = data.Data.api_mapinfo_no;
				var path = data.Data.api_no;
				var isBoss = data.Data.api_no == data.Data.api_bosscell_no;
				var labels = KanColleClient.Current.Translations.GetMapLabels(world, map, path);
				Log(LogType.Sortie,
					string.Join("/", fleetNames), // Fleets
					$"{world}-{map}", // Map
					$"{(labels != null ? $"{labels.Item1}" : $"{path}")}", // From
					$"{(labels != null ? $"{labels.Item2}" : $"{path}")}{(isBoss ? " (Boss)" : "")}", // To
					string.Join(";", ships) // Ships
				);
			}
			catch { }
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
				Log(LogType.Expedition, args.ToArray());
			}
			catch { }
		}

		private void Quests(NameValueCollection request)
		{
			try
			{
				var id = int.Parse(request["api_quest_id"]);
				var title = KanColleClient.Current.Translations.GetQuestTranslation(id);
				Log(LogType.Quests, id, title?.Replace(',', ';') ?? "untranslated quest");
			}
			catch { }
		}
		
		private void CreateItem(kcsapi_createitem item, NameValueCollection req)
		{
			try
			{
				Log(LogType.BuildItem,
					item.api_create_flag == 1 ? KanColleClient.Current.Master.SlotItems[item.api_slot_item.api_slotitem_id].Name : "Penguin", //Result
					KanColleClient.Current.Homeport.Organization.Fleets[1].Ships[0].Info.Name, //Secretary
					KanColleClient.Current.Homeport.Organization.Fleets[1].Ships[0].Level, //Secretary Level
					req["api_item1"], //Fuel
					req["api_item2"], //Ammo
					req["api_item3"], //Steel
					req["api_item4"] //Bauxite
				);
			}
			catch { }
		}

		private void CreateShip(NameValueCollection req)
		{
			waitingForShip = true;
			dockid = int.Parse(req["api_kdock_id"]);
			shipmats[0] = int.Parse(req["api_item1"]);
			shipmats[1] = int.Parse(req["api_item2"]);
			shipmats[2] = int.Parse(req["api_item3"]);
			shipmats[3] = int.Parse(req["api_item4"]);
			shipmats[4] = int.Parse(req["api_item5"]);
		}

		private void KDock(IEnumerable<kcsapi_kdock> docks)
		{
			try
			{
				foreach (var dock in docks.Where(dock => waitingForShip && dock.api_id == dockid))
				{
					Log(LogType.BuildShip,
						KanColleClient.Current.Master.Ships[dock.api_created_ship_id].Name, //Result
						KanColleClient.Current.Homeport.Organization.Fleets[1].Ships[0].Info.Name, //Secretary
						KanColleClient.Current.Homeport.Organization.Fleets[1].Ships[0].Level, //Secretary Level
						shipmats[0], //Fuel
						shipmats[1], //Ammo
						shipmats[2], //Steel
						shipmats[3], //Bauxite
						shipmats[4] //Materials
					);
					waitingForShip = false;
				}
			}
			catch (Exception)
			{
				waitingForShip = false;
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
					Log(LogType.Levels, ship.Info.Name, lvl, ship.Id, newExp);
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
				Log(LogType.Levels, "HQ", lvl, "", newExp);
		}

		private void Levels(kcsapi_practice_battle_result pr)
		{
			try
			{
				Levels(pr.api_get_ship_exp.Skip(1).ToArray(), KanColleClient.Current.Homeport.Organization.Fleets[fleetInPractice]);
				Levels(pr.api_get_exp);
			}
			catch { }
		}

		private void Levels(kcsapi_battleresult br)
		{
			try
			{
				Levels(br.api_get_ship_exp.Skip(1).ToArray(), KanColleClient.Current.Homeport.Organization.GetFleetInSortie());
				Levels(br.api_get_exp);
			}
			catch { }
		}

		private void Levels(kcsapi_combined_battle_battleresult br)
		{
			try
			{
				Levels(br.api_get_ship_exp.Skip(1).ToArray(), KanColleClient.Current.Homeport.Organization.Fleets[1]);
				Levels(br.api_get_ship_exp_combined.Skip(1).ToArray(), KanColleClient.Current.Homeport.Organization.Fleets[2]);
				Levels(br.api_get_exp);
			}
			catch { }
		}

		private void Levels(kcsapi_mission_result br, NameValueCollection req)
		{
			try
			{
				Levels(br.api_get_ship_exp, KanColleClient.Current.Homeport.Organization.Fleets[int.Parse(req["api_deck_id"])]);
				Levels(br.api_get_exp);
			}
			catch { }
		}

		private void Drop(kcsapi_battleresult br)
		{
			try
			{
				if (br.api_get_ship == null) return;
				var fleet = KanColleClient.Current.Homeport.Organization.GetFleetInSortie();
				var labels = KanColleClient.Current.Translations.GetMapLabels(fleet.SortieInfo.World, fleet.SortieInfo.Map, fleet.SortieInfo.Path);
				Log(LogType.ShipDrop,
					KanColleClient.Current.Translations.GetTranslation(br.api_get_ship.api_ship_name, TranslationType.Ships, br), //Result
					KanColleClient.Current.Translations.GetTranslation(br.api_quest_name, TranslationType.OperationMaps, br), //Map
					$"{(labels != null ? $"{labels.Item2}" : $"{fleet.SortieInfo.Path}")}", //Node
					KanColleClient.Current.Translations.GetTranslation(br.api_enemy_info.api_deck_name, TranslationType.OperationSortie, br), //Enemy Fleet
					br.api_win_rank //Rank
				);
			}
			catch { }
		}

		private void Drop(kcsapi_combined_battle_battleresult br)
		{
			try
			{
				if (br.api_get_ship == null) return;
				var fleet = KanColleClient.Current.Homeport.Organization.GetFleetInSortie();
				var labels = KanColleClient.Current.Translations.GetMapLabels(fleet.SortieInfo.World, fleet.SortieInfo.Map, fleet.SortieInfo.Path);
				Log(LogType.ShipDrop,
					KanColleClient.Current.Translations.GetTranslation(br.api_get_ship.api_ship_name, TranslationType.Ships, br), //Result
					KanColleClient.Current.Translations.GetTranslation(br.api_quest_name, TranslationType.OperationMaps, br), //Map
					$"{(labels != null ? $"{labels.Item2}" : $"{fleet.SortieInfo.Path}")}", //Node
					KanColleClient.Current.Translations.GetTranslation(br.api_enemy_info.api_deck_name, TranslationType.OperationSortie, br), //Enemy Fleet
					br.api_win_rank //Rank
					);
			}
			catch { }
		}

		private void MaterialsHistory(kcsapi_port source)
		{
			try
			{
				if (source.api_material[0].api_value != mats[0] ||
					source.api_material[1].api_value != mats[1] ||
					source.api_material[2].api_value != mats[2] ||
					source.api_material[3].api_value != mats[3] ||
					source.api_material[4].api_value != mats[6] ||
					source.api_material[5].api_value != mats[5] ||
					source.api_material[6].api_value != mats[4] ||
					source.api_material[7].api_value != mats[7])
				{
					mats[0] = source.api_material[0].api_value;
					mats[1] = source.api_material[1].api_value;
					mats[2] = source.api_material[2].api_value;
					mats[3] = source.api_material[3].api_value;
					mats[6] = source.api_material[4].api_value;
					mats[5] = source.api_material[5].api_value;
					mats[4] = source.api_material[6].api_value;
					mats[7] = source.api_material[7].api_value;
					Log(LogType.Materials,
						mats[0], mats[1], mats[2], mats[3],
						mats[4], mats[5], mats[6], mats[7]);
				}
			}
			catch { }
		}

		private void Log(LogType type, params object[] args)
		{
			if (!EnableLogging) return;
			var logPath = CreateLogFile(type);
			if (string.IsNullOrEmpty(logPath)) return;
			using (var w = System.IO.File.AppendText(logPath))
			{
				w.WriteLine(DateTime.Now.ToString(logTimestampFormat) + "," + string.Join(",", args));
			}
		}

		private static string CreateLogFile(LogType type)
		{
			try
			{
				var dir = KanColleClient.Current.Settings.LoggerFolder;
				var info = LogParameters[type];
				var fullPath = System.IO.Path.Combine(dir, info.FileName);
				if (!System.IO.Directory.Exists(dir))
					System.IO.Directory.CreateDirectory(dir);
				if (!System.IO.File.Exists(fullPath))
					System.IO.File.WriteAllText(fullPath, info.Parameters + Environment.NewLine, new UTF8Encoding(true));
				return fullPath;
			}
			catch { }
			return null;
		}
	}
}
