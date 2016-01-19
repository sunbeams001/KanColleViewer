using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Livet;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Grabacr07.KanColleViewer.ViewModels.Catalogs;
using Grabacr07.KanColleWrapper;
using Grabacr07.KanColleWrapper.Models;
using Livet.EventListeners;

namespace Grabacr07.KanColleViewer.Plugins.ViewModels
{
    public class CalculatorViewModel : ViewModel
	{
		/// <summary>
		/// Sea exp table. Cannot be used properly in xaml without dumb workarounds.
		/// </summary>
		public IEnumerable<string> SeaList { get; private set; }
		public static Dictionary<string, int> SeaExpTable = new Dictionary<string, int> 
		{
			{"1-1", 30}, {"1-2", 50}, {"1-3", 80}, {"1-4", 100}, {"1-5", 150}, {"1-6", 220},
			{"2-1", 120}, {"2-2", 150}, {"2-3", 200},{"2-4", 300}, {"2-5", 250},
			{"3-1", 310}, {"3-2", 320}, {"3-3", 330}, {"3-4", 350}, {"3-5", 400},
			{"4-1", 310}, {"4-2", 320}, {"4-3", 330}, {"4-4", 340}, {"4-5", 440},
			{"5-1", 360}, {"5-2", 380}, {"5-3", 400}, {"5-4", 420}, {"5-5", 450},
			{"6-1", 400}, {"6-2", 420}, {"6-3", 400}
		};

		public IEnumerable<string> ResultList { get; private set; }
		public string[] Results =  { "S", "A", "B", "C", "D", "E" };

		private readonly Subject<Unit> updateSource = new Subject<Unit>();
		private readonly Homeport homeport = KanColleClient.Current.Homeport;

		public ShipCatalogSortWorker SortWorker { get; private set; }

		#region Ships 変更通知プロパティ

		private IReadOnlyCollection<ShipViewModel> _Ships;

		public IReadOnlyCollection<ShipViewModel> Ships
		{
			get { return this._Ships; }
			set
			{
				if (this._Ships != value)
				{
					this._Ships = value;
					this.RaisePropertyChanged();
				}
			}
		}

		#endregion

		#region CurrentShip 変更通知プロパティ

		private Ship _CurrentShip;

		public Ship CurrentShip
		{
			get { return this._CurrentShip; }
			set
			{
				if (this._CurrentShip != value)
				{
					this._CurrentShip = value;
					if (value != null)
					{
						this.CurrentLevel = this.CurrentShip.Level;
						this.TargetLevel = Math.Min(this.CurrentShip.Level + 1, 155);
						this.CurrentExp = this.CurrentShip.Exp;
						this.UpdateExpCalculator();
						this.RaisePropertyChanged();
					}
				}
			}
		}

		#endregion

		#region CurrentLevel 変更通知プロパティ

		private int _CurrentLevel;

		public int CurrentLevel
		{
			get { return this._CurrentLevel; }
			set
			{
				if (this._CurrentLevel != value && value >= 1 && value <= 155)
				{
					this._CurrentLevel = value;
					this.CurrentExp = Ship.ExpTable[value];
					this.TargetLevel = Math.Max(this.TargetLevel, Math.Min(value + 1, 155));
					this.RaisePropertyChanged();
					this.UpdateExpCalculator();
				}
			}
		}

		#endregion

		#region TargetLevel 変更通知プロパティ

		private int _TargetLevel;

		public int TargetLevel
		{
			get { return this._TargetLevel; }
			set
			{
				if (this._TargetLevel != value && value >= 1 && value <= 155)
				{
					this._TargetLevel = value;
					this.TargetExp = Ship.ExpTable[value];
					this.CurrentLevel = Math.Min(this.CurrentLevel, Math.Max(value - 1, 1));
					this.RaisePropertyChanged();
					this.UpdateExpCalculator();
				}
			}
		}

		#endregion

		#region SelectedSea 変更通知プロパティ

		private string _SelectedSea;

		public string SelectedSea
		{
			get { return this._SelectedSea; }
			set
			{
				if (this._SelectedSea != value)
				{
					this._SelectedSea = value;
					this.RaisePropertyChanged();
					this.UpdateExpCalculator();
				}
			}
		}

		#endregion

		#region SelectedResult 変更通知プロパティ

		private string _SelectedResult;

		public string SelectedResult
		{
			get { return this._SelectedResult; }
			set
			{
				if (this._SelectedResult != value)
				{
					this._SelectedResult = value;
					this.RaisePropertyChanged();
					this.UpdateExpCalculator();
				}
			}
		}

		#endregion

		#region IsFlagship 変更通知プロパティ

		private bool _IsFlagship;

		public bool IsFlagship
		{
			get { return this._IsFlagship; }
			set
			{
				if (this._IsFlagship != value)
				{
					this._IsFlagship = value;
					this.RaisePropertyChanged();
					this.UpdateExpCalculator();
				}
			}
		}

		#endregion

		// ReSharper disable InconsistentNaming
		#region IsMVP 変更通知プロパティ
		
		private bool _IsMVP;

		public bool IsMVP
		{
			get { return this._IsMVP; }
			set
			{
				if (this._IsMVP != value)
				{
					this._IsMVP = value;
					this.RaisePropertyChanged();
					this.UpdateExpCalculator();
				}
			}
		}

		#endregion
		// ReSharper restore InconsistentNaming

		#region IsReloading 変更通知プロパティ

		private bool _IsReloading;

		public bool IsReloading
		{
			get { return this._IsReloading; }
			set
			{
				if (this._IsReloading != value)
				{
					this._IsReloading = value;
					this.RaisePropertyChanged();
					this.UpdateExpCalculator();
				}
			}
		}

		#endregion

		#region CurrentExp 変更通知プロパティ

		private int _CurrentExp;

		public int CurrentExp
		{
			get { return this._CurrentExp; }
			private set
			{
				if (this._CurrentExp != value)
				{
					this._CurrentExp = value;
					this.RaisePropertyChanged();
				}
			}
		}

		#endregion

		#region TargetExp 変更通知プロパティ

		private int _TargetExp;

		public int TargetExp
		{
			get { return this._TargetExp; }
			private set
			{
				if (this._TargetExp != value)
				{
					this._TargetExp = value;
					this.RaisePropertyChanged();
				}
			}
		}

		#endregion

		#region SortieExp 変更通知プロパティ

		private int _SortieExp;

		public int SortieExp
		{
			get { return this._SortieExp; }
			private set
			{
				if (this._SortieExp != value)
				{
					this._SortieExp = value;
					this.RaisePropertyChanged();
				}
			}
		}

		#endregion

		#region RemainingExp 変更通知プロパティ

		private int _RemainingExp;

		public int RemainingExp
		{
			get { return this._RemainingExp; }
			private set
			{
				if (this._RemainingExp != value)
				{
					this._RemainingExp = value;
					this.RaisePropertyChanged();
				}
			}
		}

		#endregion

		#region RunCount 変更通知プロパティ

		private int _RunCount;

		public int RunCount
		{
			get { return this._RunCount; }
			private set
			{
				if (this._RunCount != value)
				{
					this._RunCount = value;
					this.RaisePropertyChanged();
				}
			}
		}

		#endregion

		public string Name
		{
			get { return "Calculator"; }
			protected set { throw new NotImplementedException(); }
		}

		public CalculatorViewModel()
		{
			this.SeaList = SeaExpTable.Keys.ToList();
			this.ResultList = this.Results.ToList();

			this.SortWorker = new ShipCatalogSortWorker();
            this.SortWorker.SetFirst(ShipCatalogSortWorker.LevelColumn);

			this.updateSource
				.Do(_ => this.IsReloading = true)
				.Throttle(TimeSpan.FromMilliseconds(7.0))
				.Do(_ => this.UpdateCore())
				.Subscribe(_ => this.IsReloading = false);
			this.CompositeDisposable.Add(this.updateSource);

            if (this.homeport != null)
			this.CompositeDisposable.Add(new PropertyChangedEventListener(this.homeport.Organization)
			{
				{ () => this.homeport.Organization.Ships, (sender, args) => this.Update() },
			});

			this.SelectedSea = SeaExpTable.Keys.FirstOrDefault();
			this.SelectedResult = this.Results.FirstOrDefault();

            this.Update();
		}

		public void Update()
		{
			this.RaisePropertyChanged("AllShipTypes");
			this.updateSource.OnNext(Unit.Default);
		}

		private void UpdateCore()
		{
			var list = this.homeport.Organization.Ships.Values
				.Where(x => x.Level != 1);

			this.Ships = this.SortWorker.Sort(list)
				.Select(x => new ShipViewModel(0, x)).ToList();
		}

		/// <summary>
		/// Calculates experience given parameters. Requires levels and experience to work with.
		/// </summary>
		public void UpdateExpCalculator()
		{
			if (this.TargetLevel < this.CurrentLevel || this.TargetExp < this.CurrentExp)
				return;

			// Lawl at that this inline conditional.
			double multiplier = (this.IsFlagship ? 1.5 : 1) * (this.IsMVP ? 2 : 1) * (this.SelectedResult == "S" ? 1.2 : (this.SelectedResult == "C" ? 0.8 : (this.SelectedResult == "D" ? 0.7 : (this.SelectedResult == "E" ? 0.5 : 1))));

			this.SortieExp = (int)Math.Round( SeaExpTable[this.SelectedSea] * multiplier );
			this.RemainingExp = this.TargetExp - this.CurrentExp;
			this.RunCount = (int)Math.Ceiling( this.RemainingExp / (double)this.SortieExp );
		}
	}
}
