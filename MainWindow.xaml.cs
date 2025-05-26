using System.Collections.ObjectModel;
using System.Windows;
using TimeTracker.Features.Timer;
using TimeTracker.Shared.Converters;

namespace TimeTracker
{
    public partial class MainWindow : Window
    {
        public ObservableCollection<TimerViewModel> Timers { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            Timers = [];
            for (int i = 1; i <= 6; i++)
            {
                Timers.Add(new TimerViewModel(i));
            }
            DataContext = this;
        }
    }
}