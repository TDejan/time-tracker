using System.Collections.ObjectModel;
using System.Windows;
using TimeTracker.Features.History;
using TimeTracker.Features.History.Models;
using TimeTracker.Features.Timer;

namespace TimeTracker
{
	public partial class MainWindow : Window
	{
		public ObservableCollection<TimerViewModel> Timers { get; set; }
		public ObservableCollection<HistoryEntry> HistoryEntries { get; } = [];

		private readonly int NumberOfTimers = 6;

		public MainWindow()
		{
			InitializeComponent();
			Timers = [];

			for (int i = 1; i <= NumberOfTimers; i++)
			{
				var timer = new TimerViewModel(i);
				timer.AddHistoryEntry = entry => HistoryEntries.Add(entry);
				Timers.Add(timer);
			}

			DataContext = this;
		}

		private void ViewRecentPlays_Click(object sender, RoutedEventArgs e)
		{
			var historyWindow = new HistoryWindow(HistoryEntries)
			{
				Owner = this
			};

			historyWindow.ShowDialog();
		}
	}
}
