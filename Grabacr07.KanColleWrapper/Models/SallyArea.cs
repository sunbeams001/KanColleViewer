using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grabacr07.KanColleWrapper.Globalization;

namespace Grabacr07.KanColleWrapper.Models
{
	public interface ISallyArea
	{
		int Id { get; }

		string Name { get; }

		string Color { get; }
	}

	public class SallyArea1 : ISallyArea
	{
		public int Id
		{
			get { return 1; }
		}

		public string Name
		{
			get { return Resources.SallyArea_Area1_Name; }
		}

		public string Color
		{
			get { return "#FF202080"; }
		}
	}

	public class SallyArea2 : ISallyArea
	{
		public int Id
		{
			get { return 2; }
		}

		public string Name
		{
			get { return Resources.SallyArea_Area2_Name; }
		}

		public string Color
		{
			get { return "#FF208020"; }
		}
	}

	public class SallyArea3 : ISallyArea
	{
		public int Id
		{
			get { return 3; }
		}

		public string Name
		{
			get { return Resources.SallyArea_Area3_Name; }
		}

		public string Color
		{
			get { return "#FFC0C020"; }
		}
	}

	public class SallyArea4 : ISallyArea
	{
		public int Id
		{
			get { return 4; }
		}

		public string Name
		{
			get { return Resources.SallyArea_Area4_Name; }
		}

		public string Color
		{
			get { return "#FFC08020"; }
		}
	}

	public class SallyAreaDummy : ISallyArea
	{
		public int Id
		{
			get { return -1; }
		}

		public string Name
		{
			get { return ""; }
		}

		public string Color
		{
			get { return "#00000000"; }
		}
	}

	public class SallyArea
	{
		private static readonly Dictionary<int, ISallyArea> areas = new Dictionary<int, ISallyArea>();

		public static IEnumerable<ISallyArea> Areas
		{
			get { return areas.Values; }
		}

		static SallyArea()
		{
			areas.Add(1, new SallyArea1());
			areas.Add(2, new SallyArea2());
			areas.Add(3, new SallyArea3());
			areas.Add(4, new SallyArea4());
		}

		public static ISallyArea Get(Ship ship)
		{
			ISallyArea area;
			return areas.TryGetValue(ship.SallyArea, out area) ? area : new SallyAreaDummy();
		}
	}
}
