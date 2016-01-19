namespace Grabacr07.KanColleWrapper.Models
{
	public class SortieInfo : DisposableNotifier
	{
		public SortieInfo()
		{
			Formatted = "";
		}

		public int World;
		public int Map;

		private int _Path;
		public int Path
		{
			get { return _Path; }
			set
			{
				if (value == _Path) return;
				_Path = value;
				Format();
			}
		}

		private bool _IsNextBoss;
		public bool IsNextBoss
		{
			get { return _IsNextBoss; }
			set
			{
				if (value == _IsNextBoss) return;
				_IsNextBoss = value;
				Format();
			}
		}

		private bool _IsInBattle;
		public bool IsInBattle
		{
			get { return _IsInBattle; }
			set
			{
				if (value == _IsInBattle) return;
				_IsInBattle = value;
				Format();
			}
		}

		private string _BattleRank;
		public string BattleRank
		{
			get { return _BattleRank; }
			set
			{
				if (value == _BattleRank) return;
				_BattleRank = value;
				Format();
			}
		}

		private string _Formatted;
		public string Formatted
		{
			get { return _Formatted; }
			set
			{
				_Formatted = value;
				this.RaisePropertyChanged();
			}
		}

		private void Format()
		{
			var path = KanColleClient.Current.Translations.GetMapLabels(World, Map, Path);
			Formatted = $"World {World}-{Map}, {(path != null ? $"{path.Item1} → {path.Item2}" : $"Path {Path}")}{(IsNextBoss ? " (Boss)" : "")}{(IsInBattle ? ", Battle" : "")}{(BattleRank != null ? $": {BattleRank} Rank" : "")}";
        }
	}
}
