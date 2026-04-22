using Microsoft.AspNetCore.SignalR;
using StreamerHelpDeskServer.Hubs;
using StreamerHelpDeskServer.Models;
using StreamerHelpDeskServer.Services;
using StreamerHelpDeskServer.ViewModels;
using StreamerHelpDeskServer.Views;
using System.Windows;

namespace StreamerHelpDeskServer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainWindowViewModel _viewModel;
        private readonly SignalRHostService _signalRService;
        private readonly ServerConfigService _configService;
        private readonly LoggingService _loggingService;
        private readonly Dictionary<string, ChatWindow> _activeChatWindows = new();

        public MainWindow(
            MainWindowViewModel viewModel,
            SignalRHostService signalRService,
            ServerConfigService configService,
            LoggingService loggingService)
        {
            InitializeComponent();
            DataContext = viewModel;

            _viewModel = viewModel;
            _signalRService = signalRService;
            _configService = configService;
            _loggingService = loggingService;

            _viewModel.SetChatMessageRouter(RouteChatMessage);
        }

        private async void ServerStart_Click(object sender, RoutedEventArgs e)
        {
            if (_signalRService.IsRunning)
            {
                _viewModel.AddMessage(new HelpDeskMessage
                {
                    ClientName = "Server",
                    Category = "Info",
                    MessageText = "Server is already running."
                });
                return;
            }

            try
            {
                var config = _configService.Load();
                await _signalRService.StartAsync(config);

                _viewModel.IsServerRunning = true;

                _viewModel.AddMessage(new HelpDeskMessage
                {
                    ClientName = "Server",
                    Category = "Info",
                    MessageText = $"Server started on {config.Host}:{config.Port}{config.HubPath}"
                });
            }
            catch (Exception ex)
            {
                _viewModel.AddMessage(new HelpDeskMessage
                {
                    ClientName = "Server",
                    Category = "Error",
                    MessageText = $"Failed to start server: {ex.Message}"
                });
            }
        }

        private async void ServerStop_Click(object sender, RoutedEventArgs e)
        {
            if (!_signalRService.IsRunning)
            {
                _viewModel.AddMessage(new HelpDeskMessage
                {
                    ClientName = "Server",
                    Category = "Info",
                    MessageText = "Server is not running."
                });
                return;
            }

            await _signalRService.StopAsync();

            _viewModel.IsServerRunning = false;

            _viewModel.AddMessage(new HelpDeskMessage
            {
                ClientName = "Server",
                Category = "Info",
                MessageText = "Server stopped."
            });
        }

        private void ServerConfigure_Click(object sender, RoutedEventArgs e)
        {
            var configWindow = new ConfigurationWindow(_configService)
            {
                Owner = this
            };
            configWindow.ShowDialog();
        }

        private void Accept_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button button && button.Tag is HelpDeskMessage message)
            {
                // TODO: Implement accept logic - send response to client
                // For now, just remove the message from the queue
                _viewModel.RemoveMessage(message);
            }
        }

        private void Decline_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button button && button.Tag is HelpDeskMessage message)
            {
                _viewModel.RemoveMessage(message);
            }
        }

        private async void OpenChatWindow_Click(object sender, RoutedEventArgs e)
        {
            var connectedClients = _viewModel.GetConnectedClients();

            if (!connectedClients.Any())
            {
                MessageBox.Show("No clients connected.", "No Clients",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var selectionWindow = new ClientSelectionWindow(connectedClients)
            {
                Owner = this
            };

            if (selectionWindow.ShowDialog() == true && 
                selectionWindow.SelectedConnectionId != null && 
                selectionWindow.SelectedClientName != null)
            {
                var connectionId = selectionWindow.SelectedConnectionId;
                var clientName = selectionWindow.SelectedClientName;

                if (_activeChatWindows.ContainsKey(connectionId))
                {
                    _activeChatWindows[connectionId].Activate();
                    return;
                }

                var hubContext = _signalRService.GetHubContext();
                if (hubContext == null)
                {
                    MessageBox.Show("Server is not running.", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                await hubContext.Clients.Client(connectionId)
                    .SendAsync("ReceiveChatRequest", "Help Desk");

                var chatWindow = new ChatWindow(connectionId, clientName, hubContext, _loggingService);
                _activeChatWindows[connectionId] = chatWindow;

                chatWindow.Closed += (s, args) =>
                {
                    _activeChatWindows.Remove(connectionId);
                };

                chatWindow.Show();
            }
        }

        public void RouteChatMessage(string connectionId, string senderName, string messageText)
        {
            if (_activeChatWindows.TryGetValue(connectionId, out var chatWindow))
            {
                chatWindow.ReceiveMessage(senderName, messageText);
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
