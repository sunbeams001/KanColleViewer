using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grabacr07.KanColleWrapper.Models;
using Livet;
using Livet.EventListeners;
using Livet.EventListeners.WeakEvents;
using Grabacr07.KanColleWrapper;

namespace Grabacr07.KanColleViewer.ViewModels.Contents.Fleets
{
	/// <summary>
	/// 母港で待機中の艦隊のステータスを表します。
	/// </summary>
	public class RepairingViewModel : ViewModel
	{
		private readonly Fleet Fleet;

		#region Dock

		private RepairingDockViewModel _Dock;

		private RepairingDockViewModel Dock
		{
			get
			{
				return this._Dock;
			}
			set
			{
				if (value != this._Dock)
				{
					this._Dock = value;
					this.RaisePropertyChanged();
				}
			}
		}

		#endregion

		public string ship { get { return this.Dock != null ? this.Dock.Ship : "----"; } }
		public string CompleteTime { get { return this.Dock != null ? this.Dock.CompleteTime : "--/-- --:--:--"; } }
		public string Remaining { get { return this.Dock != null ? this.Dock.Remaining : "--:--:--"; } }

		public RepairingViewModel(Fleet fleet)
		{
			this.Fleet = fleet;

			if (this.Fleet.IsRepairling) this.Update();
			this.CompositeDisposable.Add(new PropertyChangedEventListener(this.Fleet) 
			{
				{
					"Ships",
					(sender, args) => {if (this.Fleet.State == FleetState.Homeport) this.Update();}
				},
			});
		}

		internal void Update()
		{
			if (this.Dock != null) this.Dock.Dispose();
			if (!KanColleClient.Current.Homeport.Repairyard.CheckRepairing(this.Fleet)) return;
			var shipIds = this.Fleet.Ships.Select(x => x.Id).ToArray();
			var repairyards = KanColleClient.Current.Homeport.Repairyard.Docks.Values.Where(x => x.Ship != null).Where(x => shipIds.Any(X => X == x.ShipId));
			this.Dock = new RepairingDockViewModel(repairyards.MaxBy(x => x.Remaining).First());
			this.Dock.CompositeDisposable.Add(
				new PropertyChangedEventListener(this.Dock, (sender, args) => this.RaisePropertyChanged(args.PropertyName))
			);
		}


		public virtual new void Dispose()
		{
			if (this.Dock != null) this.Dock.Dispose();

			base.Dispose();
		}
	}
}
