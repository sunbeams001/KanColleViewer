using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grabacr07.KanColleWrapper.Models;

namespace Grabacr07.KanColleViewer.ViewModels.Catalogs
{
	public class SlotItemCounter
	{
		private readonly Dictionary<int, SlotItemCounterByLevel> itemsByLevel;

		public SlotItemInfo Target { get; private set; }

		public IEnumerable<SlotItemCounterByLevel> Levels
		{
			get { return this.itemsByLevel.OrderBy(x => x.Key).Select(x => x.Value).ToList(); }
		}

		public int Count
		{
			get { return this.itemsByLevel.Sum(x => x.Value.Count); }
		}

		public SlotItemCounter(SlotItemInfo target, IEnumerable<SlotItem> items)
		{
			this.Target = target;
			this.itemsByLevel = items
				.GroupBy(x => x.Adept > 0 ? 10 + x.Adept : x.Level)
				.ToDictionary(x => x.Key > 10 ? x.Key - 10 : x.Key, x => new SlotItemCounterByLevel
				{
					Level = x.Key > 10 ? x.Key - 10 : x.Key,
					Count = x.Count(),
					IsPlane = x.Key > 10,
					LevelText = x.Key > 10 ? x.First().AdeptText : x.First().LevelText
				});
		}

		public void AddShip(Ship ship, int itemLevel, bool isPlane)
		{
			this.itemsByLevel[itemLevel].AddShip(ship, isPlane);
		}
	}

	public class SlotItemCounterByLevel
	{
		private readonly Dictionary<int, SlotItemCounterByShip> itemsByShip;
		private int count;

		public bool IsPlane { get; set; }

		public int Level { get; set; }

		public string LevelText { get; set; }

		public IEnumerable<SlotItemCounterByShip> Ships
		{
			get { return this.itemsByShip.Values.OrderByDescending(x => x.Ship.Level).ThenBy(x => x.Ship.SortNumber).ToList(); }
		}

		public int Count
		{
			get { return this.count; }
			set { this.count = this.Remainder = value; }
		}

		public int Remainder { get; private set; }

		public SlotItemCounterByLevel()
		{
			this.itemsByShip = new Dictionary<int, SlotItemCounterByShip>();
		}

		public void AddShip(Ship ship, bool isPlane)
		{
			this.IsPlane = isPlane;
			SlotItemCounterByShip target;
			if (this.itemsByShip.TryGetValue(ship.Id, out target))
				target.Count++;
			else
				this.itemsByShip.Add(ship.Id, new SlotItemCounterByShip { Ship = ship, Count = 1 });
			this.Remainder--;
		}
	}

	public class SlotItemCounterByShip
	{
		public Ship Ship { get; set; }

		public int Count { get; set; }

		public string CountString
		{
			get { return this.Count == 1 ? "" : " x " + this.Count + " "; }
		}
	}
}
