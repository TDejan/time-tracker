using System.Collections.ObjectModel;
using System.Windows;
using TimeTracker.Features.History.Models;

namespace TimeTracker.Features.History
{
	public partial class HistoryWindow : Window
	{
		public ObservableCollection<HistoryEntry> HistoryEntries { get; }

		public HistoryWindow(ObservableCollection<HistoryEntry> entries)
		{
			InitializeComponent();
			HistoryEntries = entries;
			DataContext = this;
		}

		private void ClearHistory_Click(object sender, RoutedEventArgs e)
		{
			var result = MessageBox.Show("Are you sure?", "Clear Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning);
			if (result == MessageBoxResult.Yes)
			{
				HistoryEntries.Clear();
			}
		}
	}
}
