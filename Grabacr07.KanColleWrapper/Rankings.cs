using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using Codeplex.Data;
using Fiddler;
using Grabacr07.KanColleWrapper.Internal;
using Grabacr07.KanColleWrapper.Models;
using Grabacr07.KanColleWrapper.Models.Raw;
using Livet;

namespace Grabacr07.KanColleWrapper
{
	public class Rankings : NotificationObject
	{
		private readonly ConcurrentDictionary<int, Ranking> currentData;

		public int TotalRanked { get; private set; }
		public int TotalPages  { get; private set; }
		public int CurrentPage { get; private set; }

		public IReadOnlyCollection<Ranking> Current
		{
			get
			{
				return this.currentData
					.Select(kvp => kvp.Value)
					.OrderBy(x => x.Id)
					.ToList();
			}
		}

		internal Rankings(KanColleProxy proxy)
		{
			this.currentData = new ConcurrentDictionary<int, Ranking>();

			this.TotalRanked = 0;
			this.TotalPages = 0;
			this.CurrentPage = 0;

			proxy.ApiSessionSource.Where(x => x.PathAndQuery == "/kcsapi/api_req_ranking/getlist")
				.Select(Serialize)
				.Where(x => x != null)
				.Subscribe(this.Update);
		}

		private static kcsapi_ranking_getlist Serialize(Session session)
		{
			try
			{
				var djson = DynamicJson.Parse(session.GetResponseAsJson());
				var rankings = new kcsapi_ranking_getlist
				{
					api_count = Convert.ToInt32(djson.api_data.api_count),
					api_disp_page = Convert.ToInt32(djson.api_data.api_disp_page),
					api_page_count = Convert.ToInt32(djson.api_data.api_page_count),
				};

				var list = new List<kcsapi_ranking>();
				var serializer = new DataContractJsonSerializer(typeof(kcsapi_ranking));
				foreach (var x in (object[])djson.api_data.api_list)
				{
					try
					{
						list.Add(serializer.ReadObject(new MemoryStream(Encoding.UTF8.GetBytes(x.ToString()))) as kcsapi_ranking);
					}
					catch (SerializationException ex)
					{
						Debug.WriteLine(ex.Message);
					}
				}
				rankings.api_list = list.ToArray();
				return rankings;
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex);
				return null;
			}
		}

		private void Update(kcsapi_ranking_getlist ranklist)
		{
			this.TotalRanked = ranklist.api_count;
			this.TotalPages = ranklist.api_page_count;
			this.CurrentPage = ranklist.api_disp_page;

			this.currentData.Clear();
			ranklist.api_list.Select(x => new Ranking(x))
				.ForEach(x => this.currentData.AddOrUpdate(x.Id, x, (_, __) => x));

			this.RaisePropertyChanged("Current");
		}
	}
}
