using Microsoft.AspNetCore.SignalR.Client;
using StreamerHelpDeskClient.Models;

namespace StreamerHelpDeskClient.Services
{
    public class SignalRClientService
    {
        private HubConnection? _connection;

        public bool IsConnected => _connection?.State == HubConnectionState.Connected;

        public HubConnection? Connection => _connection;

        public async Task ConnectAsync(ClientConfig config)
        {
            if (_connection != null)
                await DisconnectAsync();

            _connection = new HubConnectionBuilder()
                .WithUrl($"http://{config.ServerIP}:{config.PortNumber}/helpdesk")
                .WithAutomaticReconnect()
                .Build();

            await _connection.StartAsync();
        }

        public async Task DisconnectAsync()
        {
            if (_connection != null)
            {
                await _connection.StopAsync();
                await _connection.DisposeAsync();
                _connection = null;
            }
        }
    }
}
