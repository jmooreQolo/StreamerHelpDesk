using Microsoft.AspNetCore.SignalR;
using StreamerHelpDeskServer.Models;
using StreamerHelpDeskServer.ViewModels;

namespace StreamerHelpDeskServer.Hubs
{
    public class HelpDeskHub : Hub
    {
        private readonly MainWindowViewModel _viewModel;

        public HelpDeskHub(MainWindowViewModel viewModel)
        {
            _viewModel = viewModel;
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

            _viewModel.AddMessage(message);

            await Clients.All.SendAsync("ReceiveHelpRequest", clientName, category, messageText);
        }
    }
}
