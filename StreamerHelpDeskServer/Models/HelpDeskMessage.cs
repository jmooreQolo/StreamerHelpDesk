namespace StreamerHelpDeskServer.Models
{
    public class HelpDeskMessage
    {
        public string ClientName { get; set; } = string.Empty;
        public string MessageText { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}
