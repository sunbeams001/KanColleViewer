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
	[ExportMetadata("Title", "Calculator")]
	[ExportMetadata("Description", "start2 で取得される、艦これのマスター データを閲覧するためのビュー機能を提供します。")]
	[ExportMetadata("Version", "1.0")]
	[ExportMetadata("Author", "@Grabacr07")]
	public class KancolleCalculator : IToolPlugin
	{
        private CalculatorViewModel calculatorViewModel;

		public string ToolName
		{
			get { return "Calculator"; }
		}


		public object GetSettingsView()
		{
			return null;
		}

		public object GetToolView()
		{
            if (this.calculatorViewModel == null)
            {
                if (KanColleClient.Current.Homeport != null) this.calculatorViewModel = new CalculatorViewModel();
            }
            return new Calculator { DataContext = this.calculatorViewModel };
		}
	}
}
