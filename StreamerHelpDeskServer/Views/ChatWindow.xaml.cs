using StreamerHelpDeskServer.Hubs;
using StreamerHelpDeskServer.Models;
using StreamerHelpDeskServer.Services;
using System.Collections.ObjectModel;
using System.Windows;
using Microsoft.AspNetCore.SignalR;

namespace StreamerHelpDeskServer.Views
{
    /// <summary>
    /// Interaction logic for ChatWindow.xaml
    /// </summary>
    public partial class ChatWindow : Window
    {
        private readonly string _clientConnectionId;
        private readonly string _clientName;
        private readonly IHubContext<HelpDeskHub> _hubContext;
        private readonly LoggingService _loggingService;
        private readonly ObservableCollection<ChatMessage> _messages = new();

        public ChatWindow(string clientConnectionId, string clientName, IHubContext<HelpDeskHub> hubContext, LoggingService loggingService)
        {
            InitializeComponent();

            _clientConnectionId = clientConnectionId;
            _clientName = clientName;
            _hubContext = hubContext;
            _loggingService = loggingService;

            Title = $"Chat with {clientName}";
            MessagesItemsControl.ItemsSource = _messages;
        }

        public void ReceiveMessage(string senderName, string messageText)
        {
            Dispatcher.Invoke(() =>
            {
                var message = new ChatMessage
                {
                    SenderName = senderName,
                    MessageText = messageText,
                    Timestamp = DateTime.Now
                };

                _messages.Add(message);
                _loggingService.LogChatMessage(_clientName, _clientConnectionId, message);
            });
        }

        private async void Send_Click(object sender, RoutedEventArgs e)
        {
            var messageText = MessageTextBox.Text.Trim();
            if (string.IsNullOrEmpty(messageText))
                return;

            var message = new ChatMessage
            {
                SenderName = "Help Desk",
                MessageText = messageText,
                Timestamp = DateTime.Now
            };

            _messages.Add(message);
            _loggingService.LogChatMessage(_clientName, _clientConnectionId, message);

            await _hubContext.Clients.Client(_clientConnectionId)
                .SendAsync("ReceiveChatMessage", "Help Desk", messageText);

            MessageTextBox.Clear();
        }
    }
}
