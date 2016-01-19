using System.Collections.Generic;
using System.Linq;
using Grabacr07.KanColleWrapper;
using Livet;

namespace Grabacr07.KanColleViewer.ViewModels.Contents
{
	public class MaterialsViewModel : ViewModel
	{
		public Materials Model { get; private set; }

		public ICollection<MaterialViewModel> Values { get; private set; }

		#region SelectedItem1 変更通知プロパティ

		private MaterialViewModel _SelectedItem1;

		public MaterialViewModel SelectedItem1
		{
			get { return this._SelectedItem1; }
			set
			{
				if (this._SelectedItem1 != value)
				{
					this._SelectedItem1 = value;
					this.RaisePropertyChanged();
					if (value != null)
					{
						Models.Settings.Current.DisplayMaterial1 = value.Key;
					}

				}
			}
		}

		#endregion

		#region SelectedItem2 変更通知プロパティ

		private MaterialViewModel _SelectedItem2;

		public MaterialViewModel SelectedItem2
		{
			get { return this._SelectedItem2; }
			set
			{
				if (this._SelectedItem2 != value)
				{
					this._SelectedItem2 = value;
					this.RaisePropertyChanged();
					if (value != null)
					{
						Models.Settings.Current.DisplayMaterial2 = value.Key;
					}
				}
			}
		}

		#endregion

		#region SelectedItem3 変更通知プロパティ

		private MaterialViewModel _SelectedItem3;

		public MaterialViewModel SelectedItem3
		{
			get { return this._SelectedItem3; }
			set
			{
				if (this._SelectedItem3 != value)
				{
					this._SelectedItem3 = value;
					this.RaisePropertyChanged();
					if (value != null)
					{
						Models.Settings.Current.DisplayMaterial3 = value.Key;
					}
				}
			}
		}

		#endregion

		#region SelectedItem4 変更通知プロパティ

		private MaterialViewModel _SelectedItem4;

		public MaterialViewModel SelectedItem4
		{
			get { return this._SelectedItem4; }
			set
			{
				if (this._SelectedItem4 != value)
				{
					this._SelectedItem4 = value;
					this.RaisePropertyChanged();
					if (value != null)
					{
						Models.Settings.Current.DisplayMaterial4 = value.Key;
					}
				}
			}
		}

		#endregion

		#region SelectedItem5 変更通知プロパティ

		private MaterialViewModel _SelectedItem5;

		public MaterialViewModel SelectedItem5
		{
			get { return this._SelectedItem5; }
			set
			{
				if (this._SelectedItem5 != value)
				{
					this._SelectedItem5 = value;
					this.RaisePropertyChanged();
					if (value != null)
					{
						Models.Settings.Current.DisplayMaterial5 = value.Key;
					}
				}
			}
		}

		#endregion

		#region SelectedItem6 変更通知プロパティ

		private MaterialViewModel _SelectedItem6;

		public MaterialViewModel SelectedItem6
		{
			get { return this._SelectedItem6; }
			set
			{
				if (this._SelectedItem6 != value)
				{
					this._SelectedItem6 = value;
					this.RaisePropertyChanged();
					if (value != null)
					{
						Models.Settings.Current.DisplayMaterial6 = value.Key;
					}
				}
			}
		}

		#endregion

		#region SelectedItem7 変更通知プロパティ

		private MaterialViewModel _SelectedItem7;

		public MaterialViewModel SelectedItem7
		{
			get { return this._SelectedItem7; }
			set
			{
				if (this._SelectedItem7 != value)
				{
					this._SelectedItem7 = value;
					this.RaisePropertyChanged();
					if (value != null)
					{
						Models.Settings.Current.DisplayMaterial7 = value.Key;
					}
				}
			}
		}

		#endregion

		#region SelectedItem8 変更通知プロパティ

		private MaterialViewModel _SelectedItem8;

		public MaterialViewModel SelectedItem8
		{
			get { return this._SelectedItem8; }
			set
			{
				if (this._SelectedItem8 != value)
				{
					this._SelectedItem8 = value;
					this.RaisePropertyChanged();
					if (value != null)
					{
						Models.Settings.Current.DisplayMaterial8 = value.Key;
					}
				}
			}
		}

		#endregion

		public MaterialsViewModel()
		{
			this.Model = KanColleClient.Current.Homeport.Materials;

			var fuel = new MaterialViewModel("fuel", Properties.Resources.Homeport_Fuel);
			var ammunition = new MaterialViewModel("ammunition", Properties.Resources.Homeport_Ammo);
			var steel = new MaterialViewModel("steel", Properties.Resources.Homeport_Steel);
			var bauxite = new MaterialViewModel("bauxite", Properties.Resources.Homeport_Bauxite);
			var develop = new MaterialViewModel("develop", Properties.Resources.Homeport_DevelopmentMaterial);
			var repair = new MaterialViewModel("repair", Properties.Resources.Homeport_InstantRepair);
			var build = new MaterialViewModel("build", Properties.Resources.Homeport_InstantBuild);
			var improvement = new MaterialViewModel("improvement", Properties.Resources.Homeport_ImprovementMaterial);
			var blank = new MaterialViewModel("blank", "");

			this.Values = new List<MaterialViewModel>
			{
				fuel,
				ammunition,
				steel,
				bauxite,
				develop,
				repair,
				build,
				improvement,
				blank
			};

			this._SelectedItem1 = this.Values.FirstOrDefault(x => x.Key == Models.Settings.Current.DisplayMaterial1) ?? fuel;
			this._SelectedItem2 = this.Values.FirstOrDefault(x => x.Key == Models.Settings.Current.DisplayMaterial2) ?? ammunition;
			this._SelectedItem3 = this.Values.FirstOrDefault(x => x.Key == Models.Settings.Current.DisplayMaterial3) ?? steel;
			this._SelectedItem4 = this.Values.FirstOrDefault(x => x.Key == Models.Settings.Current.DisplayMaterial4) ?? bauxite;
			this._SelectedItem5 = this.Values.FirstOrDefault(x => x.Key == Models.Settings.Current.DisplayMaterial5) ?? develop;
			this._SelectedItem6 = this.Values.FirstOrDefault(x => x.Key == Models.Settings.Current.DisplayMaterial6) ?? repair;
			this._SelectedItem7 = this.Values.FirstOrDefault(x => x.Key == Models.Settings.Current.DisplayMaterial7) ?? build;
			this._SelectedItem8 = this.Values.FirstOrDefault(x => x.Key == Models.Settings.Current.DisplayMaterial8) ?? improvement;

			this.Model.PropertyChanged += (sender, args) =>
			{
				fuel.Value = this.Model.Fuel.ToString();
				ammunition.Value = this.Model.Ammunition.ToString();
				steel.Value = this.Model.Steel.ToString();
				bauxite.Value = this.Model.Bauxite.ToString();
				develop.Value = this.Model.DevelopmentMaterials.ToString();
				repair.Value = this.Model.InstantRepairMaterials.ToString();
				build.Value = this.Model.InstantBuildMaterials.ToString();
				improvement.Value = this.Model.ImprovementMaterials.ToString();
			};

			KanColleClient.Current.Translations.PropertyChanged += (sender, args) =>
			{
				fuel.Display = Properties.Resources.Homeport_Fuel;
				ammunition.Display = Properties.Resources.Homeport_Ammo;
				steel.Display = Properties.Resources.Homeport_Steel;
				bauxite.Display = Properties.Resources.Homeport_Bauxite;
				develop.Display = Properties.Resources.Homeport_DevelopmentMaterial;
				repair.Display = Properties.Resources.Homeport_InstantRepair;
				build.Display = Properties.Resources.Homeport_InstantBuild;
				improvement.Display = Properties.Resources.Homeport_ImprovementMaterial;
			};
		}

		public class MaterialViewModel : ViewModel
		{
			public string Key;
			private string _Display;

			public string Display
			{
				get { return this._Display; }
				set
				{
					if (this._Display != value)
					{
						this._Display = value;
						this.RaisePropertyChanged();
					}
				}
			}

			#region Value 変更通知プロパティ

			private string _Value;

			public string Value
			{
				get { return this._Value; }
				set
				{
					if (this._Value != value)
					{
						this._Value = value;
						this.RaisePropertyChanged();
					}
				}
			}

			#endregion

			public MaterialViewModel(string key, string display)
			{
				this.Key = key;
				this.Display = display;
				this.Value = "";
			}
		}
	}
}
