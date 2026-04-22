using Microsoft.AspNetCore.SignalR.Client;
using StreamerHelpDeskClient.Services;
using StreamerHelpDeskClient.ViewModels;
using StreamerHelpDeskClient.Views;
using System.Windows;
using System.Windows.Controls;

namespace StreamerHelpDeskClient
{
    /// <summary>
    /// Interaction logic for StreamerHelpDeskClient.xaml
    /// </summary>
    public partial class MainClientWindow : Window
    {
        private readonly SignalRClientService _signalRService;
        private readonly ClientConfigService _configService;
        private readonly MainClientViewModel _viewModel;
        private ChatWindow? _activeChatWindow;

        public MainClientWindow(SignalRClientService signalRService, ClientConfigService configService)
        {
            InitializeComponent();

            _signalRService = signalRService;
            _configService = configService;
            _viewModel = new MainClientViewModel();
            DataContext = _viewModel;

            Closed += MainClientWindow_Closed;
        }

        private async void MainClientWindow_Closed(object? sender, EventArgs e)
        {
            if (_signalRService.IsConnected)
                await _signalRService.DisconnectAsync();
        }

        private async void ClientConnect_Click(object sender, RoutedEventArgs e)
        {
            if (_signalRService.IsConnected)
            {
                MessageBox.Show("Already connected to the server.", "Connection",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                var config = _configService.Load();
                await _signalRService.ConnectAsync(config);

                _signalRService.Connection!.On<string>("ReceiveChatRequest", (senderName) =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        var result = MessageBox.Show($"{senderName} wants to chat with you. Accept?",
                            "Chat Request", MessageBoxButton.YesNo, MessageBoxImage.Question);

                        if (result == MessageBoxResult.Yes)
                        {
                            if (_activeChatWindow != null)
                            {
                                _activeChatWindow.Activate();
                                return;
                            }

                            _activeChatWindow = new ChatWindow(_signalRService.Connection!, config.ClientName)
                            {
                                Owner = this
                            };

                            _activeChatWindow.Closed += (s, args) =>
                            {
                                _activeChatWindow = null;
                            };

                            _signalRService.Connection.On<string, string>("ReceiveChatMessage", (sender, message) =>
                            {
                                _activeChatWindow?.ReceiveMessage(sender, message);
                            });

                            _activeChatWindow.Show();
                        }
                    });
                });

                await _signalRService.Connection.InvokeAsync("RegisterClient", config.ClientName);

                _viewModel.IsConnected = true;

                MessageBox.Show($"Connected to {config.ServerIP}:{config.PortNumber}",
                    "Connection Successful", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not connect to server: {ex.Message}",
                    "Connection Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void ClientDisconnect_Click(object sender, RoutedEventArgs e)
        {
            if (!_signalRService.IsConnected)
            {
                MessageBox.Show("Not currently connected.", "Disconnection",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            await _signalRService.DisconnectAsync();

            _viewModel.IsConnected = false;

            MessageBox.Show("Disconnected from the server.", "Disconnection Successful",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ClientConfigure_Click(object sender, RoutedEventArgs e)
        {
            var configWindow = new ConfigurationWindow(_configService)
            {
                Owner = this
            };
            configWindow.ShowDialog();
        }

        private async void HelpRequest_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && _signalRService.IsConnected)
            {
                var config = _configService.Load();
                var category = button.Content?.ToString() ?? "Unknown";
                await _signalRService.Connection!.InvokeAsync("SendHelpRequest",
                    config.ClientName, category, $"{category} request");
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
