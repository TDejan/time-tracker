using ClosedXML.Excel;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using System.Windows.Threading;
using TimeTracker.Shared;

namespace TimeTracker.Features.Timer
{
    public class TimerViewModel : INotifyPropertyChanged
    {
        // PlayStation identifier
        public int Id { get; set; }

        // Name you assign to this timer session
        public string Name { get; set; } = "";

        // Label to show PlayStation number
        public string TimerLabel => $"PlayStation {Id}";

        // Private backing field for elapsed seconds
        private int _elapsed = 0;

        // Private backing field for price display string
        private string _price = "";

        // New computed property: total elapsed seconds (changed)
        private int ElapsedSeconds => _elapsed + (IsRunning ? (int)(DateTime.Now - _startTime).TotalSeconds : 0);

        // Public read-only property that formats total elapsed seconds into hh:mm:ss (changed)
        public string ElapsedTime => TimeSpan.FromSeconds(ElapsedSeconds).ToString(@"hh\:mm\:ss");

        // Public property for displaying the price after timer stops
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

        // Returns true if timer is currently running
        public bool IsRunning => _timer?.IsEnabled == true;

        // Internal timer that ticks every second
        private DispatcherTimer? _timer;

        // The DateTime when timer was started
        private DateTime _startTime;

        // Command bound to Start/Stop button
        public ICommand StartStopCommand { get; }

        // INotifyPropertyChanged event
        public event PropertyChangedEventHandler? PropertyChanged;

        public TimerViewModel(int id)
        {
            Id = id;

            // Initialize DispatcherTimer once here (changed)
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += (s, e) =>
            {
                // Only notify UI to update elapsed time (changed)
                OnPropertyChanged(nameof(ElapsedTime));
            };

            StartStopCommand = new RelayCommand(Toggle);
        }

        // Start or stop the timer
        private void Toggle()
        {
            if (IsRunning)
            {
                // Stop the timer
                _timer?.Stop();

                // Add elapsed seconds since last start (changed)
                _elapsed += (int)(DateTime.Now - _startTime).TotalSeconds;

                // Calculate price: minutes * 0.084 and format as currency (changed)
                double minutes = _elapsed / 60.0;
                Price = $"{(minutes * 0.084):0.00} BAM";

                // Log the session to Excel
                LogSession();
            }
            else
            {
                // Start the timer
                _startTime = DateTime.Now;

                // Clear previous price when starting new session
                Price = "";

                _timer?.Start();
            }

            // Notify UI about running status and elapsed time changes
            OnPropertyChanged(nameof(IsRunning));
            OnPropertyChanged(nameof(ElapsedTime));
        }

        // Write session data to daily Excel file
        private void LogSession()
        {
            string folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
            Directory.CreateDirectory(folder);
            string filePath = Path.Combine(folder, $"logs_{DateTime.Now:yyyy-MM-dd}.xlsx");

            using var workbook = File.Exists(filePath) ? new XLWorkbook(filePath) : new XLWorkbook();
            var sheetName = $"PS{Id}";
            var sheet = workbook.Worksheets.FirstOrDefault(ws => ws.Name == sheetName) ?? workbook.Worksheets.Add(sheetName);

            // Get next available row
            var row = sheet.LastRowUsed()?.RowNumber() + 1 ?? 1;
            sheet.Cell(row, 1).Value = DateTime.Now.ToString("HH:mm:ss");
            sheet.Cell(row, 2).Value = Name;
            sheet.Cell(row, 3).Value = ElapsedTime;

            workbook.SaveAs(filePath);

            // Reset elapsed seconds after logging session
            _elapsed = 0;

            // Notify UI that elapsed time changed
            OnPropertyChanged(nameof(ElapsedTime));
        }

        // Helper method to raise PropertyChanged events
        protected void OnPropertyChanged(string propertyName)
        {
            ArgumentNullException.ThrowIfNull(propertyName);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
