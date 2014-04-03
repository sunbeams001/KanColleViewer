using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppSettings = Grabacr07.KanColleViewer.Models.Settings;

namespace Grabacr07.KanColleViewer.Views.Catalogs
{
	/// <summary>
	/// 艦娘一覧ウィンドウを表します。
	/// </summary>
	partial class ShipCatalogWindow
	{
		public ShipCatalogWindow()
		{
			this.InitializeComponent();

			MainWindow.Current.Closed += (sender, args) => this.Close();
		}

		protected override void OnClosed(EventArgs e)
		{
			base.OnClosed(e);

			AppSettings.Current.Save();
		}
	}
}
