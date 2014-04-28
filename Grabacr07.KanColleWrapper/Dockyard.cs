using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Grabacr07.KanColleWrapper.Internal;
using Grabacr07.KanColleWrapper.Models;
using Grabacr07.KanColleWrapper.Models.Raw;
using Livet;

namespace Grabacr07.KanColleWrapper
{
	/// <summary>
	/// 複数の建造ドックを持つ工廠を表します。
	/// </summary>
	public class Dockyard : NotificationObject
	{
		private readonly Homeport homeport;

		#region Dock 変更通知プロパティ

		private MemberTable<BuildingDock> _Docks;

		public MemberTable<BuildingDock> Docks
		{
			get { return this._Docks; }
			set
			{
				if (this._Docks != value)
				{
					this._Docks = value;
					this.RaisePropertyChanged();
				}
			}
		}

		#endregion

		internal Dockyard(Homeport parent, KanColleProxy proxy)
		{
			this.homeport = parent;
			this.Docks = new MemberTable<BuildingDock>();

			proxy.ApiSessionSource.Where(x => x.PathAndQuery == "/kcsapi/api_get_member/kdock")
				.TryParse<kcsapi_kdock[]>()
				.Subscribe(this.Update);

			proxy.ApiSessionSource.Where(x => x.PathAndQuery == "/kcsapi/api_req_kousyou/getship")
				.TryParse<kcsapi_getship>()
				.Subscribe(x => 
				{
					this.Update(x.api_kdock);
					this.homeport.AddShip(new Ship(this.homeport, x.api_ship));
				});

			proxy.ApiSessionSource.Where(x => x.PathAndQuery == "/kcsapi/api_req_kousyou/createship_speedchange")
				.Select(x => { SvData<kcsapi_createship_speedchange> result; return SvData.TryParse(x, out result) ? result : null; })
				.Where(x => x != null && x.IsSuccess)
				.Subscribe(x => this.ChangeSpeed(x.RequestBody));
		}

		private void Update(kcsapi_kdock[] source)
		{
			if (this.Docks.Count == source.Length)
			{
				foreach (var raw in source)
				{
					var target = this.Docks[raw.api_id];
					if (target != null) target.Update(raw);
				}
			}
			else
			{
				this.Docks.ForEach(x => x.Value.Dispose());
				this.Docks = new MemberTable<BuildingDock>(source.Select(x => new BuildingDock(x)));
			}
		}

		private void ChangeSpeed(NameValueCollection RawRequest)
		{
			int api_kdock_id = Int32.Parse(RawRequest["api_kdock_id"]);
			int api_highspeed = Int32.Parse(RawRequest["api_highspeed"]);

			if (api_highspeed > 0 && this.Docks[api_kdock_id] != null)
				this.Docks[api_kdock_id].SetComplete();
		}
	}
}
