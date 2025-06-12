namespace TimeTracker.Features.History.Models
{
    public class HistoryEntry
    {
        public int TimerId { get; set; }
        public string StartTime { get; set; } = string.Empty;
        public string Duration {get; set; } = string.Empty;
        public string Comment { get; set; } = string.Empty;

        public string Price {get; set; } = string.Empty;
    }
}