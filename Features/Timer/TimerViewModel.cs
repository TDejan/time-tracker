using ClosedXML.Excel;
using System.ComponentModel;
using System.IO;
using System.Windows.Input;
using System.Windows.Threading;
using TimeTracker.Features.History.Models;
using TimeTracker.Shared;

namespace TimeTracker.Features.Timer
{
	public class TimerViewModel : INotifyPropertyChanged
	{
		public int Id { get; set; }
		public string Name { get; set; } = "";
		public string TimerLabel => $"PlayStation {Id}";

		private int _elapsed = 0;
		private int ElapsedSeconds => _elapsed + (IsRunning ? (int)(DateTime.Now - _startTime).TotalSeconds : 0);
		public string ElapsedTime => TimeSpan.FromSeconds(ElapsedSeconds).ToString(@"hh\:mm\:ss");

		private string _price = "";
		public string Price
		{
			get => _price;
			private set
			{
				if (_price != value)
				{
					_price = value;
					OnPropertyChanged(nameof(Price));
				}
			}
		}

		public bool IsRunning => _timer?.IsEnabled == true;

		private bool _isPaused = false;
		public bool IsPaused
		{
			get => _isPaused;
			set
			{
				if (_isPaused != value)
				{
					_isPaused = value;
					OnPropertyChanged(nameof(IsPaused));
				}
			}
		}


		public ICommand PauseResumeCommand { get; }


		private DispatcherTimer? _timer;
		private DateTime _startTime;

		private DateTime _lastPriceUpdate;

		public ICommand StartStopCommand { get; }

		public Action<HistoryEntry>? AddHistoryEntry { get; set; }


		public event PropertyChangedEventHandler? PropertyChanged;

		/// <summary>
		/// Initializes <see cref="TimerNewModel", update price per minut./>
		/// </summary>
		/// <param name="id"></param>
		public TimerViewModel(int id)
		{
			Id = id;

			_timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
			_timer.Tick += (s, e) =>
			{
				OnPropertyChanged(nameof(ElapsedTime));

				if ((DateTime.Now - _lastPriceUpdate).TotalMinutes >= 1)
				{
					double minutes = ElapsedSeconds / 60.0;
					Price = $"{(minutes * 0.084):0.00} BAM";
					_lastPriceUpdate = DateTime.Now;
				}
			};

			StartStopCommand = new RelayCommand(ToggleStartStop);
			PauseResumeCommand = new RelayCommand(TogglePause);
		}

		private void TogglePause()
		{
			if (!IsRunning && !IsPaused) return;

			if (!IsPaused)
			{
				// Pause the timer
				_timer?.Stop();
				_elapsed += (int)(DateTime.Now - _startTime).TotalSeconds;

				// Calculate and show price immediately
				double minutes = _elapsed / 60.0;
				Price = $"{(minutes * 0.084):0.00} BAM";

				IsPaused = true;
			}
			else
			{
				// Resume the timer
				_startTime = DateTime.Now;
				_lastPriceUpdate = _startTime;

				_timer?.Start();
				IsPaused = false;
			}

			OnPropertyChanged(nameof(ElapsedTime));
		}

		private void ToggleStartStop()
		{
			if (IsRunning && !IsPaused)
			{
				_timer?.Stop();
				_elapsed += (int)(DateTime.Now - _startTime).TotalSeconds;

				double minutes = _elapsed / 60.0;
				Price = $"{(minutes * 0.084):0.00} BAM";

				string durationString = TimeSpan.FromSeconds(_elapsed).ToString(@"hh\:mm\:ss");
				
				LogSession();

				HistoryEntry entry = new()
				{
					Time = DateTime.Now.ToString("HH:mm:ss"),
					Name = Name,
					Duration = durationString,
					Description = string.Empty,
					Price = Price
				};

				AddHistoryEntry?.Invoke(entry);
			}

			else
			{
				_startTime = DateTime.Now;
				_lastPriceUpdate = _startTime;

				if (IsPaused)
					IsPaused = false;

				Price = "";

				_timer?.Start();
			}

			OnPropertyChanged(nameof(IsRunning));
			OnPropertyChanged(nameof(ElapsedTime));
		}

		private void LogSession()
		{
			string folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
			Directory.CreateDirectory(folder);
			string filePath = Path.Combine(folder, $"logs_{DateTime.Now:yyyy-MM-dd}.xlsx");

			using var workbook = File.Exists(filePath) ? new XLWorkbook(filePath) : new XLWorkbook();
			var sheetName = $"PS{Id}";
			var sheet = workbook.Worksheets.FirstOrDefault(ws => ws.Name == sheetName) ?? workbook.Worksheets.Add(sheetName);

			var row = sheet.LastRowUsed()?.RowNumber() + 1 ?? 1;
			sheet.Cell(row, 1).Value = DateTime.Now.ToString("HH:mm:ss");
			sheet.Cell(row, 2).Value = Name;
			sheet.Cell(row, 3).Value = ElapsedTime;

			workbook.SaveAs(filePath);

			_elapsed = 0;

			OnPropertyChanged(nameof(ElapsedTime));
		}

		protected void OnPropertyChanged(string propertyName)
		{
			ArgumentNullException.ThrowIfNull(propertyName);
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
