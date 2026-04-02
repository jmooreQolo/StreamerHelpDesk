namespace StreamerHelpDeskClient.Models
{
    public class ClientConfig
    {
        public string ServerIP { get; set; } = "localhost";
        public int PortNumber { get; set; } = 5000;
        public string ClientName { get; set; } = Environment.MachineName;
    }
}
