using Microsoft.AspNetCore.SignalR.Client;
using StreamerHelpDeskClient.Models;
using System.Collections.ObjectModel;
using System.Windows;

namespace StreamerHelpDeskClient.Views
{
    /// <summary>
    /// Interaction logic for ChatWindow.xaml
    /// </summary>
    public partial class ChatWindow : Window
    {
        private readonly HubConnection _connection;
        private readonly string _clientName;
        private readonly ObservableCollection<ChatMessage> _messages = new();

        public ChatWindow(HubConnection connection, string clientName)
        {
            InitializeComponent();

            _connection = connection;
            _clientName = clientName;

            MessagesItemsControl.ItemsSource = _messages;
        }

        public void ReceiveMessage(string senderName, string messageText)
        {
            Dispatcher.Invoke(() =>
            {
                _messages.Add(new ChatMessage
                {
                    SenderName = senderName,
                    MessageText = messageText,
                    Timestamp = DateTime.Now
                });
            });
        }

        private async void Send_Click(object sender, RoutedEventArgs e)
        {
            var messageText = MessageTextBox.Text.Trim();
            if (string.IsNullOrEmpty(messageText))
                return;

            _messages.Add(new ChatMessage
            {
                SenderName = _clientName,
                MessageText = messageText,
                Timestamp = DateTime.Now
            });

            await _connection.InvokeAsync("SendChatMessageToServer", _clientName, messageText);

            MessageTextBox.Clear();
        }
    }
}
