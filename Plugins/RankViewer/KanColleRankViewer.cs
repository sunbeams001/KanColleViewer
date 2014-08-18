using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using Grabacr07.KanColleViewer.Composition;
using Grabacr07.KanColleViewer.Plugins.ViewModels;
using Grabacr07.KanColleViewer.Plugins.Views;
using Grabacr07.KanColleWrapper;

namespace Grabacr07.KanColleViewer.Plugins
{
	[Export(typeof(IToolPlugin))]
    [ExportMetadata("Title", "RankViewer")]
	[ExportMetadata("Description", "View ranking")]
	[ExportMetadata("Version", "1.0")]
    [ExportMetadata("Author", "@m-kc")]
	public class KancolleCalculator : IToolPlugin
	{
        private RankingsViewModel rankingsViewModel;

		public string ToolName
		{
			get { return "Rankings Viewer"; }
		}


		public object GetSettingsView()
		{
			return null;
		}

		public object GetToolView()
		{
            if (this.rankingsViewModel == null)
            {
                if (KanColleClient.Current.Homeport != null) this.rankingsViewModel = new RankingsViewModel();
            }
            return new Calculator { DataContext = this.rankingsViewModel };
		}
	}
}
