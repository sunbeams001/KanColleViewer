using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grabacr07.KanColleWrapper.Internal;
using Grabacr07.KanColleWrapper.Models;

namespace Grabacr07.KanColleWrapper
{
	internal static class Calculator
	{
		public static int CalcAirPowerOld(this SlotItem slotItem, int onslot)
		{
			return slotItem.Info.IsAirSuperiorityFighter ? (int)(slotItem.Info.AA * Math.Sqrt(onslot)) : 0;
		}

		private static readonly int[] minAirPowerBonus = { 0, 1, 4, 6, 11, 16, 17, 25 };

		public static int CalcMinAirPowerBonus(this SlotItem slotItem, int onslot)
		{
			if (onslot < 1)
				return 0;
			else if (slotItem.Info.Type == SlotItemType.艦上戦闘機 && slotItem.Adept >= 0 && slotItem.Adept <= 7)
				return minAirPowerBonus[slotItem.Adept];
			else
				return 0;
		}

		private static readonly int[] maxAirPowerBonus = { 0, 2, 5, 8, 12, 18, 18, 26 };

		public static int CalcMaxAirPowerBonus(this SlotItem slotItem, int onslot)
		{
			if (onslot < 1)
				return 0;
			else if (slotItem.Info.Type == SlotItemType.艦上戦闘機 && slotItem.Adept >= 0 && slotItem.Adept <= 7)
				return maxAirPowerBonus[slotItem.Adept];
			else if (slotItem.Info.Type == SlotItemType.艦上攻撃機 || slotItem.Info.Type == SlotItemType.艦上爆撃機)
				return (int)Math.Ceiling(3 * slotItem.Adept / 7d);
			else if (slotItem.Info.Type == SlotItemType.水上爆撃機)
				return (int)Math.Ceiling(9 * slotItem.Adept / 7d);
			else
				return 0;
		}

		/// <summary>
		/// 指定した艦の制空能力を計算します。
		/// </summary>
		public static int CalcAirPower(this Ship ship)
		{
			return ship.EquippedSlots.Select(x => x.Item.CalcAirPowerOld(x.Current)).Sum();
		}

		public static int CalcMinAirPower(this Ship ship)
		{
			return ship.EquippedSlots.Select(x => x.Item.CalcAirPowerOld(x.Current) + x.Item.CalcMinAirPowerBonus(x.Current)).Sum();
		}

		public static int CalcMaxAirPower(this Ship ship)
		{
			return ship.EquippedSlots.Select(x => x.Item.CalcAirPowerOld(x.Current) + x.Item.CalcMaxAirPowerBonus(x.Current)).Sum();
		}

		public static double CalcViewRange(this Fleet fleet)
		{
			return ViewRangeCalcLogic.Get(KanColleClient.Current.Settings.ViewRangeCalcType).Calc(fleet.Ships);
		}

		public static bool IsHeavilyDamage(this LimitedValue hp)
		{
			return (hp.Current / (double)hp.Maximum) <= 0.25;
		}

		/// <summary>
		/// 現在のシーケンスから護衛退避した艦娘を除きます。
		/// </summary>
		public static IEnumerable<Ship> WithoutEvacuated(this IEnumerable<Ship> ships)
		{
			return ships.Where(ship => !ship.Situation.HasFlag(ShipSituation.Evacuation) && !ship.Situation.HasFlag(ShipSituation.Tow));
		}
	}
}
