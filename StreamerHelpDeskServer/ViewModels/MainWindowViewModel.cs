using CommunityToolkit.Mvvm.ComponentModel;
using StreamerHelpDeskServer.Models;
using System.Collections.ObjectModel;
using System.Windows;

namespace StreamerHelpDeskServer.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        private readonly Dictionary<string, string> _connectedClients = new();
        private Action<string, string, string>? _chatMessageRouter;

        [ObservableProperty]
        private bool _isServerRunning;

        public ObservableCollection<HelpDeskMessage> Messages { get; } = new();

        public void SetChatMessageRouter(Action<string, string, string> router)
        {
            _chatMessageRouter = router;
        }

        public void AddMessage(HelpDeskMessage message)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Messages.Add(message);
            });
        }

        public void RemoveMessage(HelpDeskMessage message)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Messages.Remove(message);
            });
        }

        public void AddConnectedClient(string connectionId, string clientName)
        {
            lock (_connectedClients)
            {
                _connectedClients[connectionId] = clientName;
            }
        }

        public void RemoveConnectedClient(string connectionId)
        {
            lock (_connectedClients)
            {
                _connectedClients.Remove(connectionId);
            }
        }

        public Dictionary<string, string> GetConnectedClients()
        {
            lock (_connectedClients)
            {
                return new Dictionary<string, string>(_connectedClients);
            }
        }

        public string? GetClientName(string connectionId)
        {
            lock (_connectedClients)
            {
                return _connectedClients.TryGetValue(connectionId, out var name) ? name : null;
            }
        }

        public void RouteChatMessageToWindow(string connectionId, string senderName, string messageText)
        {
            _chatMessageRouter?.Invoke(connectionId, senderName, messageText);
        }
    }
}
