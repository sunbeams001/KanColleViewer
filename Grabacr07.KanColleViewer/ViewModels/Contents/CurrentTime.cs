using Grabacr07.KanColleWrapper;
using System;

namespace Grabacr07.KanColleViewer.ViewModels.Contents
{
	public class CurrentTime : TimerNotifier
	{
		private String _TimeString;
		public String TimeString
		{
			get { return this._TimeString; }
			set
			{
				if (this._TimeString == value) return;
				this._TimeString = value;
				this.RaisePropertyChanged();
			}
		}

		static private readonly TimeZoneInfo jstZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time");

		protected override void Tick()
		{
			base.Tick();
			TimeString = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, jstZoneInfo).ToString("M\\/dd ddd H:mm:ss");
		}

	}
}
