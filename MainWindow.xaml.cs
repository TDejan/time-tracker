using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using TimeTracker.Features.History;
using TimeTracker.Features.History.Models;
using TimeTracker.Features.Timer;
using ClosedXML.Excel;
using Microsoft.Win32;
using System.ComponentModel;


namespace TimeTracker
{
	public partial class MainWindow : Window
	{
		public ObservableCollection<TimerViewModel> Timers { get; set; }
		public ObservableCollection<HistoryEntry> HistoryEntries { get; } = [];

		private readonly object? _activeTimerContext;

		private TimerViewModel? _activeTimer;

		public MainWindow()
		{
			InitializeComponent();
			Timers = [];

			for (int i = 1; i <= TimerViewModel.NumberOfTimers; i++)
			{
				var timer = new TimerViewModel(i)
				{
					AddHistoryEntry = entry => HistoryEntries.Add(entry)
				};
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

		private void EditComments_Click(object sender, RoutedEventArgs e)
		{
			if (sender is Button btn && btn.DataContext is TimerViewModel timer)
			{
				_activeTimer = timer;
				CommentTextBox.Text = timer.Comment;
				CommentPopup.IsOpen = true;
			}
		}

		private void SaveComment_Click(object sender, RoutedEventArgs e)
		{
			if (_activeTimer != null)
			{
				string fullComment = CommentTextBox.Text.Trim();

				_activeTimer.Comment = fullComment;

				string preview = fullComment.Length > 10 ? string.Concat(fullComment.AsSpan(0, 10), "...") : fullComment;
				_activeTimer.Preview = preview;

				CommentPopup.IsOpen = false;
				CommentTextBox.Clear();
				_activeTimer = null;
			}
		}

		private void CancelComment_Click(object sender, RoutedEventArgs e)
		{
			CommentPopup.IsOpen = false;
		}

		private void ExportToExcel_Click(object sender, RoutedEventArgs e)
		{
			var dlg = new SaveFileDialog
			{
				FileName = $"TimeTrackerExport_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.xlsx",
				DefaultExt = ".xlsx",
				Filter = "Excel Files (*.xlsx)|*.xlsx|All Files (*.*)|*.*",
				Title = "Export All Timers to Excel"
			};

			if (dlg.ShowDialog() == true)
			{
				try
				{
					ExportAllTimersToExcel(dlg.FileName);

					MessageBox.Show($"Export successful!\nFile saved to:\n{dlg.FileName}", "Export Complete", MessageBoxButton.OK, MessageBoxImage.Information);
				}
				catch (Exception ex)
				{
					MessageBox.Show($"Failed to export:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				}
			}
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			base.OnClosing(e);

			if (HistoryEntries.Any())
			{
				var result = MessageBox.Show(
					"Do you want to save the logged sessions before exiting?",
					"Save Sessions",
					MessageBoxButton.YesNoCancel,
					MessageBoxImage.Question);

				if (result == MessageBoxResult.Cancel)
				{
					e.Cancel = true;
					return;
				}

				if (result == MessageBoxResult.Yes)
				{
					var dialog = new SaveFileDialog
					{
						FileName = $"TimeTrackerExport_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.xlsx",
						Filter = "Excel Files (*.xlsx)|*.xlsx",
						Title = "Save Logged Sessions"
					};

					if (dialog.ShowDialog() == true)
					{
						try
						{
							ExportAllTimersToExcel(dialog.FileName);
						}
						catch (Exception ex)
						{
							MessageBox.Show($"Failed to save file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
							e.Cancel = true;
						}
					}
					else
					{
						e.Cancel = true;
					}
				}
			}
		}

        private void ExportAllTimersToExcel(string filePath)
        {
            using var workbook = new XLWorkbook();
            var sheet = workbook.Worksheets.Add("All Sessions");

            sheet.Cell(1, 1).Value = "Console";
            sheet.Cell(1, 2).Value = "Start Time";
            sheet.Cell(1, 3).Value = "Duration";
            sheet.Cell(1, 4).Value = "Price";
            sheet.Cell(1, 5).Value = "Comment";

			var orderedEntries = HistoryEntries
				.OrderBy(he => he.StartTime)
				.ToList();

            for (int i = 0; i < orderedEntries.Count; i++)
            {
                var entry = orderedEntries[i];
                int row = i + 2;

                sheet.Cell(row, 1).Value = $"Console {entry.TimerId}";
                sheet.Cell(row, 2).Value = entry.StartTime ?? string.Empty;
                sheet.Cell(row, 3).Value = entry.Duration ?? string.Empty;
                sheet.Cell(row, 4).Value = entry.Price ?? string.Empty;
                sheet.Cell(row, 5).Value = entry.Comment ?? string.Empty;
            }

            workbook.SaveAs(filePath);
        }
    }
}
