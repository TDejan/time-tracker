using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Threading;
using TimeTracker.Features.History.Models;
using TimeTracker.Shared;

namespace TimeTracker.Features.Timer
{
	public class TimerViewModel : INotifyPropertyChanged
	{
		public string TimerLabel => $"Console {Id}";

		public static readonly int NumberOfTimers = 6;
		public int Id { get; set; }

		private string _preview = string.Empty;
		public string Preview
		{
			get => _preview;
			set
			{
				if (_preview != value)
				{
					_preview = value;
					OnPropertyChanged(nameof(Preview));
				}
			}
		}

		private int _elapsed = 0;
		private int ElapsedSeconds => _elapsed + (IsRunning ? (int)(DateTime.Now - _startTime).TotalSeconds : 0);
		public string ElapsedTime => TimeSpan.FromSeconds(ElapsedSeconds).ToString(@"hh\:mm\:ss");

		private string _price = string.Empty;
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


		private readonly DispatcherTimer? _timer;
		
		private DateTime _startTime;

		private DateTime _lastPriceUpdate;

		private DateTime? _sessionStartTime = null;

		private string? _comment;
		public string? Comment
		{
			get => _comment;
			set
			{
				_comment = value;
				OnPropertyChanged(nameof(Comment));
			}
		}

		public ICommand StartStopCommand { get; }
		public ICommand RefreshPriceCommand { get; }

		public Action<HistoryEntry>? AddHistoryEntry { get; set; }

		public event PropertyChangedEventHandler? PropertyChanged;

		public TimerViewModel(int id)
		{
			Id = id;

			_timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };

			_timer.Tick += (s, e) =>
			{
				OnPropertyChanged(nameof(ElapsedTime));

				RecalculatePriceAfter(60);

				void RecalculatePriceAfter(int seconds)
				{
					if ((DateTime.Now - _lastPriceUpdate).TotalSeconds >= seconds)
					{
						CalculatePrice();
						_lastPriceUpdate = DateTime.Now;
					}
				}
			};

			StartStopCommand = new RelayCommand(ToggleStartStop);
			PauseResumeCommand = new RelayCommand(TogglePause);
			RefreshPriceCommand = new RelayCommand(RefreshPrice);
		}

		private void TogglePause()
		{
			if (!IsRunning && !IsPaused)
			{
				return;
			};

			if (!IsPaused)
			{
				_timer?.Stop();
				_elapsed += (int)(DateTime.Now - _startTime).TotalSeconds;

				CalculatePrice();

				IsPaused = true;
			}
			else
			{
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

				CalculatePrice();

				string durationString = TimeSpan.FromSeconds(_elapsed).ToString(@"hh\:mm\:ss");
				
				HistoryEntry entry = new()
				{
					TimerId = Id,
					StartTime = _sessionStartTime?.ToString("HH:mm:ss") ?? DateTime.Now.ToString("HH:mm:ss"),
					Duration = durationString,
					Comment = Comment ?? string.Empty,
					Price = Price
				};

				AddHistoryEntry?.Invoke(entry);

				_sessionStartTime = null;
			}

			else
			{
				_elapsed = 0;
				_startTime = DateTime.Now;
				_lastPriceUpdate = _startTime;

				if (_sessionStartTime == null)
				{
					_sessionStartTime = _startTime;
				}

				if (IsPaused)
				{
					IsPaused = false;
				}

				Price = string.Empty;

				_timer?.Start();
			}

			OnPropertyChanged(nameof(IsRunning));
			OnPropertyChanged(nameof(ElapsedTime));
		}

		private void RefreshPrice()
		{
			CalculatePrice();
			OnPropertyChanged(nameof(Price));
		}

		private void CalculatePrice()
		{
			double minutes = ElapsedSeconds / 60.0;
			Price = $"{(minutes * 0.084):0.00} BAM";
		}

		protected void OnPropertyChanged(string propertyName)
		{
			ArgumentNullException.ThrowIfNull(propertyName);
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
