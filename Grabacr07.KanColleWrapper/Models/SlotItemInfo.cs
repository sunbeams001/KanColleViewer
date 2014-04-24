using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grabacr07.KanColleWrapper.Internal;
using Grabacr07.KanColleWrapper.Models.Raw;

namespace Grabacr07.KanColleWrapper.Models
{
	/// <summary>
	/// 装備アイテムの種類に基づく情報を表します。
	/// </summary>
	public class SlotItemInfo : RawDataWrapper<kcsapi_mst_slotitem>, IIdentifiable
	{
		private SlotItemIconType? iconType;
		private int? categoryId;

		public int Id
		{
			get { return this.RawData.api_id; }
		}

		public string Name
		{
			get
			{
				return KanColleClient.Current.Homeport.Translations.GetTranslation(this.RawData.api_name, TranslationType.Equipment, this.RawData);
			}
		}

		public string DetailedName
		{
			get 
			{
				string _Detail = this.Detail;
				return this.Name + (_Detail != "" ? "\n" + _Detail : "");
			}
		}

		public string UntranslatedName
		{
			get
			{
				return (this.RawData.api_name != this.Name ? this.RawData.api_name : "");
			}
		}

		public string Detail
		{
			get
			{
				string AddDetail = "";

				if (this.RawData.api_houg > 0)
					AddDetail += " +" + this.RawData.api_houg + " " + KanColleClient.Current.Homeport.Translations.Firepower;
				if (this.RawData.api_tyku > 0)
					AddDetail += (AddDetail != "" ? "\n" : "") + " +" + this.RawData.api_tyku + " " + KanColleClient.Current.Homeport.Translations.AntiAir;
				if (this.RawData.api_raig > 0)
					AddDetail += (AddDetail != "" ? "\n" : "") + " +" + this.RawData.api_raig + " " + KanColleClient.Current.Homeport.Translations.Torpedo;
				if (this.RawData.api_tais > 0)
					AddDetail += (AddDetail != "" ? "\n" : "") + " +" + this.RawData.api_tais + " " + KanColleClient.Current.Homeport.Translations.AntiSub;
				if (this.RawData.api_saku > 0)
					AddDetail += (AddDetail != "" ? "\n" : "") + " +" + this.RawData.api_saku + " " + KanColleClient.Current.Homeport.Translations.SightRange;
				if (this.RawData.api_soku > 0)
					AddDetail += (AddDetail != "" ? "\n" : "") + " +" + this.RawData.api_soku + " " + KanColleClient.Current.Homeport.Translations.Speed;
				if (this.RawData.api_souk > 0)
					AddDetail += (AddDetail != "" ? "\n" : "") + " +" + this.RawData.api_souk + " " + KanColleClient.Current.Homeport.Translations.Armor;
				if (this.RawData.api_taik > 0)
					AddDetail += (AddDetail != "" ? "\n" : "") + " +" + this.RawData.api_taik + " " + KanColleClient.Current.Homeport.Translations.Health;
				if (this.RawData.api_luck > 0)
					AddDetail += (AddDetail != "" ? "\n" : "") + " +" + this.RawData.api_luck + " " + KanColleClient.Current.Homeport.Translations.Luck;
				if (this.RawData.api_houk > 0)
					AddDetail += (AddDetail != "" ? "\n" : "") + " +" + this.RawData.api_houk + " " + KanColleClient.Current.Homeport.Translations.Evasion;
				if (this.RawData.api_houm > 0)
					AddDetail += (AddDetail != "" ? "\n" : "") + " +" + this.RawData.api_houm + " " + KanColleClient.Current.Homeport.Translations.Accuracy;
				if (this.RawData.api_baku > 0)
					AddDetail += (AddDetail != "" ? "\n" : "") + " +" + this.RawData.api_baku + " " + KanColleClient.Current.Homeport.Translations.DiveBomb;
// 				if (this.RawData.api_raik > 0)
// 					AddDetail += (AddDetail != "" ? "\n" : "") + " +" + this.RawData.api_raik + " api_raik";
//				if (this.RawData.api_raim > 0)
//					AddDetail += (AddDetail != "" ? "\n" : "") + " +" + this.RawData.api_raim + " api_raim";
// 				if (this.RawData.api_sakb > 0)
// 					AddDetail += (AddDetail != "" ? "\n" : "") + " +" + this.RawData.api_sakb + " api_sakb";
// 				if (this.RawData.api_atap > 0)
// 					AddDetail += (AddDetail != "" ? "\n" : "") + " +" + this.RawData.api_atap + " api_atap";
//  			if (this.RawData.api_rare > 0)
//  				AddDetail += (AddDetail != "" ? "\n" : "") + " +" + this.RawData.api_rare + " api_rare";
// 				if (this.RawData.api_bakk > 0)
// 					AddDetail += (AddDetail != "" ? "\n" : "") + " +" + this.RawData.api_bakk + " api_bakk";
				if (this.RawData.api_leng > 0)
					AddDetail += (AddDetail != "" ? "\n" : "") + " " + KanColleClient.Current.Homeport.Translations.AttackRange + " (" + this.RawData.api_leng + ")";

				return AddDetail;
			}
		}

		public SlotItemIconType IconType
		{
			get { return this.iconType ?? (SlotItemIconType)(this.iconType = (SlotItemIconType)(this.RawData.api_type.Get(3) ?? 0)); }
		}

		public int CategoryId
		{
			get { return this.categoryId ?? (int)(this.categoryId = this.RawData.api_type.Get(2) ?? int.MaxValue); }
		}

		/// <summary>
		/// 対空値を取得します。
		/// </summary>
		public int AA
		{
			get { return this.RawData.api_tyku; }
		}

		/// <summary>
		/// この装備アイテムが艦載機かどうかを示す値を取得します。
		/// </summary>
		public bool IsAircraft
		{
			get
			{
				return this.IconType == SlotItemIconType.Fighter ||
					   this.IconType == SlotItemIconType.TorpedoBomber ||
					   this.IconType == SlotItemIconType.DiveBomber ||
					   this.IconType == SlotItemIconType.ReconPlane;
			}
		}

		/// <summary>
		/// この装備アイテムが水上機かどうかを示す値を取得します。
		/// </summary>
		public bool IsSeaplane
		{
			get { return this.IconType == SlotItemIconType.ReconSeaplane; }
		}

		internal SlotItemInfo(kcsapi_mst_slotitem rawData) : base(rawData) { }

		public override string ToString()
		{
			return string.Format("ID = {0}, Name = \"{1}\", Type = {{{2}}}", this.Id, this.Name, this.RawData.api_type.ToString(", "));
		}

		#region static members

		private static readonly SlotItemInfo dummy = new SlotItemInfo(new kcsapi_mst_slotitem()
		{
			api_id = 0,
			api_name = "？？？",
		});

		public static SlotItemInfo Dummy
		{
			get { return dummy; }
		}

		#endregion
	}
}
