using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fiddler;
using Grabacr07.KanColleWrapper.Globalization;
using Grabacr07.KanColleWrapper.Internal;
using Grabacr07.KanColleWrapper.Models.Raw;

namespace Grabacr07.KanColleWrapper.Models
{
	/// <summary>
	/// 母港に所属している艦娘を表します。
	/// </summary>
	public class Ship : RawDataWrapper<kcsapi_ship2>, IIdentifiable
	{
		private readonly Homeport homeport;

		// TODO: lv 151-155?
		private int[] BaseRepairTime = {
			0,  10,  20,  30,  40,  50,  60,  70,  80,  90,  100, 110, 120, 125, 130, 145, 150, 155, 160, 165, 180,
				185, 190, 195, 200, 205, 210, 225, 230, 235, 240, 245, 250, 255, 260, 265, 280, 285, 290, 295, 300,
				305, 310, 315, 320, 325, 330, 345, 350, 355, 360, 365, 370, 375, 380, 385, 390, 395, 400, 405, 420,
				425, 430, 435, 440, 445, 450, 455, 460, 465, 470, 475, 480, 485, 490, 505, 510, 515, 520, 525, 530,
				535, 540, 545, 550, 555, 560, 565, 570, 575, 580, 585, 600, 605, 610, 615, 620, 652, 630, 635, 635,
				650, 655, 660, 665, 670, 675, 680, 685, 690, 695, 700, 705, 710, 715, 720, 725, 730, 735, 740, 745,
				750, 755, 760, 765, 770, 775, 780, 785, 790, 795, 800, 805, 810, 815, 820, 825, 830, 835, 840, 845,
				850, 855, 860, 865, 870, 875, 880, 885, 890, 895, 900, 905, 910, 915
		};

		/// <summary>
		/// Completely experience table from 1 to 155. Each line = 20 levels
		/// </summary>
		public static int[] ExpTable = new int[] {
			0, 0, 100, 300, 600, 1000, 1500, 2100, 2800, 3600, 4500, 5500, 6600, 7800, 9100, 10500, 12000, 13600, 15300, 17100, 19000, 
			21000, 23100, 25300, 27600, 30000, 32500, 35100, 37800, 40600, 43500, 46500, 49600, 52800, 56100, 59500, 63000, 66600, 70300, 74100, 78000, 
			82000, 86100, 90300, 94600, 99000, 103500, 108100, 112800, 117600, 122500, 127500, 132700, 138100, 143700, 149500, 155500, 161700, 168100, 174700, 181500, 
			188500, 195800, 203400, 211300, 219500, 228000, 236800, 245900, 255300, 265000, 275000, 285400, 296200, 307400, 319000, 331000, 343400, 356200, 369400, 383000, 
			397000, 411500, 426500, 442000, 458000, 474500, 491500, 509000, 527000, 545500, 564500, 584500, 606500, 631500, 661500, 701500, 761500, 851500, 1000000, 1000000, 
			1010000, 1011000, 1013000, 1016000, 1020000, 1025000, 1031000, 1038000, 1046000, 1055000, 1065000, 1077000, 1091000, 1107000, 1125000, 1145000, 1168000, 1194000, 1223000, 1255000, 
			1290000, 1329000, 1372000, 1419000, 1470000, 1525000, 1584000, 1647000, 1714000, 1785000, 1860000, 1940000, 2025000, 2115000, 2210000, 2310000, 2415000, 2525000, 2640000, 2760000, 
			2887000, 3021000, 3162000, 3310000, 3465000, 3628000, 3799000, 3978000, 4165000, 4360000, 4564000, 4777000, 4999000, 5230000, 5470000
		};

		public static int ExpToLevel(int exp)
		{
			return Array.FindIndex(ExpTable, x => x > exp) - 1;
		}

		/// <summary>
		/// この艦娘を識別する ID を取得します。
		/// </summary>
		public int Id
		{
			get { return this.RawData.api_id; }
		}

		/// <summary>
		/// 艦娘の種類に基づく情報を取得します。
		/// </summary>
		public ShipInfo Info { get; private set; }

		public int SortNumber
		{
			get { return this.RawData.api_sortno; }
		}

		/// <summary>
		/// 艦娘の現在のレベルを取得します。
		/// </summary>
		public int Level
		{
			get { return this.RawData.api_lv; }
		}

		/// <summary>
		/// 艦娘がロックされているかどうかを示す値を取得します。
		/// </summary>
		public bool IsLocked
		{
			get { return this.RawData.api_locked == 1; }
		}

		/// <summary>
		/// 艦娘の現在の累積経験値を取得します。
		/// </summary>
		public int Exp
		{
			get { return this.RawData.api_exp.Get(0) ?? 0; }
		}

		/// <summary>
		/// この艦娘が次のレベルに上がるために必要な経験値を取得します。
		/// </summary>
		public int ExpForNextLevel
		{
			get { return this.RawData.api_exp.Get(1) ?? 0; }
		}

		public int ExpForNextRemodelingLevel
		{
			get
			{
				var lvl = Info.NextRemodelingLevel;
				if (lvl != null && ExpTable[lvl.Value] > Exp)
				{
					return ExpTable[lvl.Value] - Exp;
				}
				return 0;
			}
		}

		public string NextRemodelingInfo
		{
			get {
				return ExpForNextRemodelingLevel > 0
					? $"Next remodel: {Info.NextRemodelingLevel} lv, {ExpForNextRemodelingLevel} exp."
					: "max. remodel";
			}
		}

		#region HP 変更通知プロパティ

		private LimitedValue _HP;

		/// <summary>
		/// 耐久値を取得します。
		/// </summary>
		public LimitedValue HP
		{
			get { return this._HP; }
			private set
			{
				this._HP = value;
				this.RaisePropertyChanged();

				if (value.IsHeavilyDamage())
				{
					this.Situation |= ShipSituation.HeavilyDamaged;
				}
				else
				{
					this.Situation &= ~ShipSituation.HeavilyDamaged;
				}
			}
		}

		#endregion

		#region Fuel 変更通知プロパティ

		private LimitedValue _Fuel;

		/// <summary>
		/// 燃料を取得します。
		/// </summary>
		public LimitedValue Fuel
		{
			get { return this._Fuel; }
			private set
			{
				this._Fuel = value;
				this.RaisePropertyChanged();
			}
		}

		#endregion

		#region Bull 変更通知プロパティ

		private LimitedValue _Bull;

		public LimitedValue Bull
		{
			get { return this._Bull; }
			private set
			{
				this._Bull = value;
				this.RaisePropertyChanged();
			}
		}

		#endregion


		#region Firepower 変更通知プロパティ

		private ModernizableStatus _Firepower;

		/// <summary>
		/// 火力ステータス値を取得します。
		/// </summary>
		public ModernizableStatus Firepower
		{
			get { return this._Firepower; }
			private set
			{
				this._Firepower = value;
				this.RaisePropertyChanged();

			}
		}

		#endregion

		#region Torpedo 変更通知プロパティ

		private ModernizableStatus _Torpedo;

		/// <summary>
		/// 雷装ステータス値を取得します。
		/// </summary>
		public ModernizableStatus Torpedo
		{
			get { return this._Torpedo; }
			private set
			{
				this._Torpedo = value;
				this.RaisePropertyChanged();

			}
		}

		#endregion

		#region AA 変更通知プロパティ

		private ModernizableStatus _AA;

		/// <summary>
		/// 対空ステータス値を取得します。
		/// </summary>
		public ModernizableStatus AA
		{
			get { return this._AA; }
			private set
			{
				this._AA = value;
				this.RaisePropertyChanged();
			}

		}

		#endregion

		#region Armer 変更通知プロパティ

		private ModernizableStatus _Armer;

		/// <summary>
		/// 装甲ステータス値を取得します。
		/// </summary>
		public ModernizableStatus Armer
		{
			get { return this._Armer; }
			private set
			{
				this._Armer = value;
				this.RaisePropertyChanged();

			}
		}

		#endregion

		#region Luck 変更通知プロパティ

		private ModernizableStatus _Luck;

		/// <summary>
		/// 運のステータス値を取得します。
		/// </summary>
		public ModernizableStatus Luck
		{
			get { return this._Luck; }
			private set
			{
				this._Luck = value;
				this.RaisePropertyChanged();
			}
		}

		#endregion

		private static string ShowStat(ModernizableStatus stat, int? total)
		{
			return $"{stat.Current} ({(stat.IsMax ? "MAX" : "+" + (stat.Max - stat.Current))}){(total != null && total != stat.Current ? $", {total} with equipment" : "")}";
		}

		private static string ShowStat(LimitedValue stat, bool showCurrent = false)
		{
			return $"{stat.Minimum} ({(stat.Maximum == stat.Minimum ? "MAX" : "+" + (stat.Maximum - stat.Minimum))}){(showCurrent && stat.Current > stat.Minimum ? $", {stat.Current} with equipment" : "")}";
		}

		public string AllStats
		{
			get
			{
				try
				{
					var totalFirepower = this.Firepower.Current + this.EquippedSlots.Sum(slot => slot.Item.Info.Firepower);
					var totalTorpedo = this.Torpedo.Current + this.EquippedSlots.Sum(slot => slot.Item.Info.Torpedo);
					var totalAA = this.AA.Current + this.EquippedSlots.Sum(slot => slot.Item.Info.AA);
					var totalArmor = this.Armer.Current + this.EquippedSlots.Sum(slot => slot.Item.Info.Armor);
					var equipmentAntiSub = this.EquippedSlots.Sum(slot => slot.Item.Info.AntiSub);

					var diveBomb = this.EquippedSlots.Sum(slot => slot.Item.Info.DiveBomb);
					var accuracy = this.EquippedSlots.Sum(slot => slot.Item.Info.Accuracy);
					var range = this.EquippedSlots.Select(slot => slot.Item.Info.AttackRange).DefaultIfEmpty().Max();
					var airPowerMin = this.CalcMinAirPower();
					var airPowerMax = this.CalcMaxAirPower();

					var details = "";
					details += $"{Resources.Stats_Firepower}: {ShowStat(this.Firepower, totalFirepower)}\n";
					details += $"{Resources.Stats_Torpedo}: {ShowStat(this.Torpedo, totalTorpedo)}{(diveBomb > 0 ? $", {Resources.Stats_DiveBomb}: {diveBomb}" : "")}{(totalFirepower > 0 && totalTorpedo > 0 ? $", {totalFirepower + totalTorpedo} with firepower" : "")}\n";
                    details += $"{Resources.Stats_AntiAir}: {ShowStat(this.AA, totalAA)}\n";
					details += $"{Resources.Stats_Armor}: {ShowStat(this.Armer, totalArmor)}\n";
					details += $"{Resources.Stats_Luck}: {ShowStat(this.Luck, null)}\n";
					details += $"{Resources.Stats_Evasion}: {ShowStat(this.Evasion, true)}\n";
					details += $"{Resources.Stats_AntiSub}: {ShowStat(this.AntiSub)}{(equipmentAntiSub > 0 ? $", equipment: {equipmentAntiSub}" : "")}\n";
					details += $"{Resources.Stats_SightRange}: {ShowStat(this.LineOfSight, true)}";
					if (accuracy > 0) details += $"\nEquipment accuracy: {accuracy}";
					if (range > 0) details += $"\nEquipment range: {range}";
					if (airPowerMin > 0 || airPowerMax > 0) details += $"\nAir power: {airPowerMin}-{airPowerMax}";

					return details;
				}
				catch (Exception)
				{
					return "can't show stats";
				}
			}
		}

		public string RepairTime
		{
			get
			{
				if (!this.IsDamaged)
					return "OK";

				// Only need to show Facility time when they are not the same time and if the ship is lightly damaged
				return string.Format(Resources.Ship_RepairDockToolTip, this.RepairDockTime)
					+ (this.IsLightlyDamaged && this.RepairFacilityTime != this.RepairDockTime ? "\n" + string.Format(Resources.Ship_RepairFacilityToolTip, this.RepairFacilityTime) : "");
			}
		}

		#region Slots 変更通知プロパティ

		private ShipSlot[] _Slots;

		public ShipSlot[] Slots
		{
			get { return this._Slots; }
			set
			{
				if (this._Slots != value)
				{
					this._Slots = value;
					this.RaisePropertyChanged();
				}
			}
		}

		#endregion

		#region EquippedSlots 変更通知プロパティ

		private ShipSlot[] _EquippedSlots;

		public ShipSlot[] EquippedSlots
		{
			get { return this._EquippedSlots; }
			set
			{
				if (this._EquippedSlots != value)
				{
					this._EquippedSlots = value;
					this.RaisePropertyChanged();
				}
			}
		}

		#endregion

		#region ExSlot 変更通知プロパティ

		private ShipSlot _ExSlot;

		public ShipSlot ExSlot
		{
			get { return this._ExSlot; }
			set
			{
				if (this._ExSlot != value)
				{
					this._ExSlot = value;
					this.RaisePropertyChanged();
				}
			}
		}

		#endregion

		/// <summary>
		/// 装備によるボーナスを含めた索敵ステータス値を取得します。
		/// </summary>
		public int ViewRange
		{
			get { return this.RawData.api_sakuteki.Get(0) ?? 0; }
		}

		/// <summary>
		/// Anti-Submarine stat with and without equipment.
		/// </summary>
		public LimitedValue AntiSub { get; private set; }

		/// <summary>
		/// Line of Sight stat with and without equipment.
		/// </summary>
		public LimitedValue LineOfSight { get; private set; }

		/// <summary>
		/// Evasion stat with and without equipment.
		/// </summary>
		public LimitedValue Evasion { get; private set; }

		/// <summary>
		/// 火力・雷装・対空・装甲のすべてのステータス値が最大値に達しているかどうかを示す値を取得します。
		/// </summary>
		public bool IsMaxModernized
		{
			get { return this.Firepower.IsMax && this.Torpedo.IsMax && this.AA.IsMax && this.Armer.IsMax; }
		}

		/// <summary>
		/// 現在のコンディション値を取得します。
		/// </summary>
		public int Condition
		{
			get { return this.RawData.api_cond; }
		}

		/// <summary>
		/// コンディションの種類を示す <see cref="ConditionType" /> 値を取得します。
		/// </summary>
		public ConditionType ConditionType
		{
			get { return ConditionTypeHelper.ToConditionType(this.RawData.api_cond); }
		}
		
		/// <summary>
		/// この艦が出撃した海域を識別する整数値を取得します。
		/// </summary>
		public int SallyArea
		{
			get { return this.RawData.api_sally_area; }
		}

		/// <summary>
		/// Repair time taking in consideration HP, LV, and Ship Type.
		/// </summary>
		public string RepairDockTime
		{
			get
			{
				return TimeSpan.FromSeconds(Math.Floor((this.HP.Maximum - this.HP.Current) * this.BaseRepairTime[Math.Min(this.Level, 150)] * this.Info.ShipType.RepairMultiplier) + 30).ToString();
			}
		}

		public string RepairFacilityTime
		{
			get
			{
				// Time it takes to heal 1HP
				double minDockTime = Math.Floor(this.BaseRepairTime[Math.Min(this.Level, 150)] * this.Info.ShipType.RepairMultiplier) + 30;

				if (minDockTime < 1200)
					return this.RepairDockTime;
					
				return TimeSpan.FromMinutes((this.HP.Maximum - this.HP.Current) * 20).ToString();
			}
		}

		public bool IsDamaged
		{
			get { return (this.HP.Maximum - this.HP.Current) > 0; }
		}

		public bool IsLightlyDamaged
		{
			get { return this.IsDamaged && (this.HP.Current / (double)this.HP.Maximum) > 0.5; }
		}

		public bool IsModeratelyDamaged
		{
			get { return this.IsDamaged && (this.HP.Current / (double)this.HP.Maximum) <= 0.5 && (this.HP.Current / (double)this.HP.Maximum) > 0.25; }
		}

		public bool IsBadlyDamaged
		{
			get { return (this.HP.Current / (double)this.HP.Maximum) <= 0.25; }
		}


		#region Status 変更通知プロパティ

		private ShipSituation situation;

		public ShipSituation Situation
		{
			get { return this.situation; }
			set
			{
				if (this.situation != value)
				{
					this.situation = value;
					this.RaisePropertyChanged();
				}
			}
		}

		#endregion
        
		internal Ship(Homeport parent, kcsapi_ship2 rawData)
			: base(rawData)
		{
			this.homeport = parent;
			this.Update(rawData);
		}

		internal void Update(kcsapi_ship2 rawData)
		{
			this.UpdateRawData(rawData);

			this.Info = KanColleClient.Current.Master.Ships[rawData.api_ship_id] ?? ShipInfo.Dummy;
			this.HP = new LimitedValue(this.RawData.api_nowhp, this.RawData.api_maxhp, 0);
			this.Fuel = new LimitedValue(this.RawData.api_fuel, this.Info.RawData.api_fuel_max, 0);
			this.Bull = new LimitedValue(this.RawData.api_bull, this.Info.RawData.api_bull_max, 0);

			if (this.RawData.api_kyouka.Length >= 5)
			{
				this.Firepower = new ModernizableStatus(this.Info.RawData.api_houg, this.RawData.api_kyouka[0]);
				this.Torpedo = new ModernizableStatus(this.Info.RawData.api_raig, this.RawData.api_kyouka[1]);
				this.AA = new ModernizableStatus(this.Info.RawData.api_tyku, this.RawData.api_kyouka[2]);
				this.Armer = new ModernizableStatus(this.Info.RawData.api_souk, this.RawData.api_kyouka[3]);
				this.Luck = new ModernizableStatus(this.Info.RawData.api_luck, this.RawData.api_kyouka[4]);
			}

			this.UpdateSlots();

			// Minimum removes equipped values.
			int eqAntiSub = 0, eqEvasion = 0, eqLineOfSight = 0;

			foreach (ShipSlot item in this.EquippedSlots)
			{
				if (item == null)
					continue;

				eqAntiSub += item.Item.Info.RawData.api_tais;
				eqEvasion += item.Item.Info.RawData.api_houk;
				eqLineOfSight += item.Item.Info.RawData.api_saku;
			}

			this.AntiSub = new LimitedValue(this.RawData.api_taisen[0], this.RawData.api_taisen[1], this.RawData.api_taisen[0] - eqAntiSub);
			this.Evasion = new LimitedValue(this.RawData.api_kaihi[0], this.RawData.api_kaihi[1], this.RawData.api_kaihi[0] - eqEvasion);
			this.LineOfSight = new LimitedValue(this.RawData.api_sakuteki[0], this.RawData.api_sakuteki[1], this.RawData.api_sakuteki[0] - eqLineOfSight);
		}

		public void UpdateSlots()
		{
			this.Slots = this.RawData.api_slot
				.Select(id => this.homeport.Itemyard.SlotItems[id])
				.Select((t, i) => new ShipSlot(t, this.Info.RawData.api_maxeq.Get(i) ?? 0, this.RawData.api_onslot.Get(i) ?? 0))
				.ToArray();
			this.ExSlot = new ShipSlot(this.homeport.Itemyard.SlotItems[this.RawData.api_slot_ex], 0, 0);
			this.EquippedSlots = this.EnumerateAllEquippedItems().ToArray();

			if (this.EquippedSlots.Any(x => x.Item.Info.Type == SlotItemType.応急修理要員))
			{
				this.Situation |= ShipSituation.DamageControlled;

			}
			else
			{
				this.Situation &= ~ShipSituation.DamageControlled;
			}
		}

		internal void Charge(int fuel, int bull, int[] onslot)
		{
			this.Fuel = this.Fuel.Update(fuel);
			this.Bull = this.Bull.Update(bull);
			for (var i = 0; i < this.Slots.Length; i++) this.Slots[i].Current = onslot.Get(i) ?? 0;
		}

		internal void Repair()
		{
			var max = this.HP.Maximum;
			this.HP = this.HP.Update(max);
		}

		public override string ToString()
		{
			return string.Format("ID = {0}, Name = \"{1}\", ShipType = \"{2}\", Level = {3}", this.Id, this.Info.Name, this.Info.ShipType.Name, this.Level);
		}

		private IEnumerable<ShipSlot> EnumerateAllEquippedItems()
		{
			foreach (var slot in this.Slots.Where(x => x.Equipped)) yield return slot;
			if (this.ExSlot.Equipped) yield return this.ExSlot;
		}

	}
}
