namespace StreamerHelpDeskServer.Models
{
    public class ChatMessage
    {
        public string SenderName { get; set; } = string.Empty;
        public string MessageText { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}
