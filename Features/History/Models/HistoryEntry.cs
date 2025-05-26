namespace TimeTracker.Features.History.Models
{
    public class HistoryEntry
    {
        public string Time { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Duration {get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public string Price {get; set; } = string.Empty;
    }
}