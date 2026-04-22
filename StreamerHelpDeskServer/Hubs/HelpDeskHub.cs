using Microsoft.AspNetCore.SignalR;
using StreamerHelpDeskServer.Models;
using StreamerHelpDeskServer.Services;
using StreamerHelpDeskServer.ViewModels;

namespace StreamerHelpDeskServer.Hubs
{
    public class HelpDeskHub : Hub
    {
        private readonly MainWindowViewModel _viewModel;
        private readonly LoggingService _loggingService;

        public HelpDeskHub(MainWindowViewModel viewModel, LoggingService loggingService)
        {
            _viewModel = viewModel;
            _loggingService = loggingService;
        }

        public override async Task OnConnectedAsync()
        {
            _loggingService.LogServerEvent($"Client attempting connection - ConnectionId: {Context.ConnectionId}");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var clientName = _viewModel.GetClientName(Context.ConnectionId);
            _loggingService.LogClientDisconnection(clientName ?? "Unknown", Context.ConnectionId);

            _viewModel.RemoveConnectedClient(Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }

        public async Task RegisterClient(string clientName)
        {
            _loggingService.LogClientConnection(clientName, Context.ConnectionId);
            _viewModel.AddConnectedClient(Context.ConnectionId, clientName);

            var message = new HelpDeskMessage
            {
                ClientName = clientName,
                Category = "Connected",
                MessageText = "Client connected to the server",
                Timestamp = DateTime.Now
            };
            _viewModel.AddMessage(message);

            await Task.CompletedTask;
        }

        public async Task SendHelpRequest(string clientName, string category, string messageText)
        {
            var message = new HelpDeskMessage
            {
                ClientName = clientName,
                Category = category,
                MessageText = messageText,
                Timestamp = DateTime.Now
            };

            _loggingService.LogHelpRequest(message);
            _viewModel.AddMessage(message);

            await Clients.All.SendAsync("ReceiveHelpRequest", clientName, category, messageText);
        }

        public async Task SendChatMessage(string targetConnectionId, string senderName, string messageText)
        {
            await Clients.Client(targetConnectionId).SendAsync("ReceiveChatMessage", senderName, messageText);
        }

        public async Task SendChatMessageToServer(string senderName, string messageText)
        {
            _viewModel.RouteChatMessageToWindow(Context.ConnectionId, senderName, messageText);
            await Task.CompletedTask;
        }
    }
}
