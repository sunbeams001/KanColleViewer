using Grabacr07.KanColleWrapper.Models;

namespace Grabacr07.KanColleViewer.ViewModels.Contents.Fleets
{
	public class SortieViewModel : QuickStateViewViewModel
	{
		// QuickStateView は ContentControl に対し型ごとの DataTemplate を適用する形で実現するので
		// 状況に応じた型がそれぞれ必要。これはその 1 つ。

		public SortieInfo SortieInfo { get; }

		public SortieViewModel(FleetState state) : base(state) {
            SortieInfo = state.GetFleet().SortieInfo;
		}
	}
}
