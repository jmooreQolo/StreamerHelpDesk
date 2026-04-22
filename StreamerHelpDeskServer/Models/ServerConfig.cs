namespace StreamerHelpDeskServer.Models
{
    public class ServerConfig
    {
        public string Host { get; set; } = "0.0.0.0";
        public int Port { get; set; } = 5000;
        public string HubPath { get; set; } = "/helpdesk";
        public int MaxConnections { get; set; } = 0;
        public int KeepAliveIntervalSeconds { get; set; } = 15;
        public int ClientTimeoutSeconds { get; set; } = 30;
        public bool EnableDetailedErrors { get; set; } = false;
    }
}
