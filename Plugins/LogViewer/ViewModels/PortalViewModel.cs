using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows;
using Grabacr07.KanColleWrapper;
using Livet;

namespace Grabacr07.KanColleViewer.Plugins.ViewModels
{
    public class PortalViewModel : ViewModel
    {
        #region LogCollection 変更通知プロパティ

        private LogItemCollection logCollection;

        public LogItemCollection LogCollection
        {
            get { return this.logCollection; }
            set
            {
	            if (this.logCollection == value) return;
	            this.logCollection = value;
	            this.RaisePropertyChanged();
            }
        }

        #endregion

        #region IsReloading 変更通知プロパティ

        private bool isReloading;

	    private bool IsReloading
        {
            get { return this.isReloading; }
            set
            {
	            if (this.isReloading == value) return;
	            this.isReloading = value;
	            this.RaisePropertyChanged();
            }
        }

        #endregion

        #region SelectorBuildItem 変更通知プロパティ

        private bool selectorBuildItem;

        public bool SelectorBuildItem
        {
            get { return this.selectorBuildItem; }
            set
            {
	            if (this.selectorBuildItem == value) return;
	            this.selectorBuildItem = value;
	            if (value)
		            this.CurrentLogType = Logger.LogType.BuildItem;
	            this.RaisePropertyChanged();
            }
        }

        #endregion

        #region SelectorBuildShip 変更通知プロパティ

        private bool selectorBuildShip;

        public bool SelectorBuildShip
        {
            get { return this.selectorBuildShip; }
            set
            {
	            if (this.selectorBuildShip == value) return;
	            this.selectorBuildShip = value;
	            if (value)
		            this.CurrentLogType = Logger.LogType.BuildShip;
	            this.RaisePropertyChanged();
            }
        }

        #endregion

        #region SelectorShipDrop 変更通知プロパティ

        private bool selectorShipDrop = true;

        public bool SelectorShipDrop
        {
            get { return this.selectorShipDrop; }
            set
            {
	            if (this.selectorShipDrop == value) return;
	            this.selectorShipDrop = value;
	            if (value)
		            this.CurrentLogType = Logger.LogType.ShipDrop;
	            this.RaisePropertyChanged();
            }
        }

        #endregion

		#region SelectorMaterials 変更通知プロパティ

		private bool selectorMaterials;

		public bool SelectorMaterials
		{
			get { return this.selectorMaterials; }
			set
			{
				if (this.selectorMaterials == value) return;
				this.selectorMaterials = value;
				if (value)
					this.CurrentLogType = Logger.LogType.Materials;
				this.RaisePropertyChanged();
			}
		}

		#endregion

		#region SelectorExpedition 変更通知プロパティ

		private bool selectorExpedition;

		public bool SelectorExpedition
		{
			get { return this.selectorExpedition; }
			set
			{
				if (this.selectorExpedition == value) return;
				this.selectorExpedition = value;
				if (value)
					this.CurrentLogType = Logger.LogType.Expedition;
				this.RaisePropertyChanged();
			}
		}

		#endregion

		#region SelectorLevels 変更通知プロパティ

		private bool selectorLevels;

		public bool SelectorLevels
		{
			get { return this.selectorLevels; }
			set
			{
				if (this.selectorLevels == value) return;
				this.selectorLevels = value;
				if (value)
					this.CurrentLogType = Logger.LogType.Levels;
				this.RaisePropertyChanged();
			}
		}

		#endregion

		#region SelectorQuests 変更通知プロパティ

		private bool selectorQuests;

		public bool SelectorQuests
		{
			get { return this.selectorQuests; }
			set
			{
				if (this.selectorQuests == value) return;
				this.selectorQuests = value;
				if (value)
					this.CurrentLogType = Logger.LogType.Quests;
				this.RaisePropertyChanged();
			}
		}

		#endregion

		#region CurrentLogType

		private Logger.LogType currentLogType;

        private Logger.LogType CurrentLogType
        {
            get { return this.currentLogType; }
            set
            {
	            if (this.currentLogType == value) return;
	            this.currentLogType = value;

	            this._watcher.Filter = Logger.LogParameters[value].FileName;

	            this.CurrentPage = 1;
	            this.Update();
            }
        }

		#endregion

		#region HasPreviousPage 変更通知プロパティ

		public bool HasPreviousPage => this.TotalPage > 1 && this.CurrentPage > 1;

	    #endregion

		#region HasNextPage 変更通知プロパティ

		public bool HasNextPage => this.TotalPage > 1 && this.CurrentPage < this.TotalPage;

	    #endregion

		#region CurrentPage 変更通知プロパティ

		private int _CurrentPage;

	    private int CurrentPage
		{
			get { return this._CurrentPage; }
			set
			{
				if (this._CurrentPage == value) return;
				this._CurrentPage = value;
				this.RaisePropertyChanged("HasPreviousPage");
				this.RaisePropertyChanged("HasNextPage");
				this.RaisePropertyChanged();
			}
		}

		#endregion

		#region TotalPage 変更通知プロパティ

		private int _TotalPage;

	    private int TotalPage
		{
			get { return this._TotalPage; }
			set
			{
				if (this._TotalPage == value) return;
				this._TotalPage = value;
				this.RaisePropertyChanged("HasPreviousPage");
				this.RaisePropertyChanged("HasNextPage");
				this.RaisePropertyChanged();
			}
		}

		#endregion

        private FileSystemWatcher _watcher;

	    private int logPerPage = 10;

        public PortalViewModel()
        {
            this.SelectorShipDrop = true;
            this.currentLogType = Logger.LogType.ShipDrop;
			this.CurrentPage = 1;

            this.Update();

            try
            {
                this._watcher = new FileSystemWatcher(Directory.GetParent(Logger.Directory).ToString())
                {
                    IncludeSubdirectories = true,
                    NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.CreationTime | NotifyFilters.FileName,
                    Filter = Logger.LogParameters[this.currentLogType].FileName,
                    EnableRaisingEvents = true
                };

                this._watcher.Changed += (sender, e) => { this.Update(); };
                this._watcher.Created += (sender, e) => { this.Update(); };
                this._watcher.Deleted += (sender, e) => { this.Update(); };
                this._watcher.Renamed += (sender, e) => { this.Update(); };
            }
            catch (Exception)
            {
                if (this._watcher != null)
                    this._watcher.EnableRaisingEvents = false;
            }
        }

	    private async void Update()
        {
            this.IsReloading = true;
            this.LogCollection = await this.UpdateCore();
            this.IsReloading = false;
        }

		private static IEnumerable<string> ReadLines(string path)
		{
			using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 0x1000, FileOptions.SequentialScan))
			using (var sr = new StreamReader(fs, Encoding.UTF8))
			{
				string line;
				while ((line = sr.ReadLine()) != null)
				{
					yield return line;
				}
			}
		}

        private Task<LogItemCollection> UpdateCore()
        {
            var items = new LogItemCollection();

            return Task.Factory.StartNew(() =>
            {
                try
                {
					var file = Path.Combine(Logger.Directory, Logger.LogParameters[this.CurrentLogType].FileName);

                    if (!File.Exists(file))
                        return items;

                    var lines = ReadLines(file);

					// ReSharper disable once PossibleMultipleEnumeration
					this.TotalPage = (lines.Count() - 1)/20 + 1;

	                // ReSharper disable once PossibleMultipleEnumeration
                    lines.Take(1).First().Split(',').ToList().ForEach((col => items.Columns.Add(col)));

	                // ReSharper disable once PossibleMultipleEnumeration
					lines.Skip(1).Reverse().Skip((this.CurrentPage - 1) * this.logPerPage).Take(this.logPerPage).ToList().ForEach(line =>
                    {
                        var elements = line.Split(',');
                        items.Rows.Add(elements
                            .Take(items.Columns.Count)
                            .ToArray());
                    });

                    return items;
                }
                catch (Exception ex)
                {
					System.Diagnostics.Debug.WriteLine(ex.ToString());
                    return items;
                }
            });
        }

	    public void ToPreviousPage()
	    {
		    --this.CurrentPage;
		    this.Update();
	    }

		public void ToNextPage()
		{
			++this.CurrentPage;
			this.Update();
		}

		private static readonly string directory = Path.Combine(KanColleClient.Directory, "Charts");

		public void ShowCharts()
		{
			try
			{
				var index = Path.Combine(directory, "index.html");
				var chrome = Path.Combine(directory, "Chrome");
				Process.Start("chrome.exe", $"--allow-file-access-from-files --new-window \"{index}\" --user-data-dir=\"{chrome}\"");
			}
			catch (Exception ex)
			{
				MessageBox.Show($"An error occurred \"{ex.Message}\", make sure Chrome is installed");
			}
		}

		public void OpenCsv()
		{
			try
			{
				System.Diagnostics.Process.Start(Path.Combine(Logger.Directory, Logger.LogParameters[this.CurrentLogType].FileName));
			}
			catch (Exception ex)
			{
				MessageBox.Show($"An error occurred \"{ex.Message}\"");
			}
		}

	}
}
