using Microsoft.AspNetCore.SignalR.Client;
using StreamerHelpDeskClient.Services;
using StreamerHelpDeskClient.Views;
using System.Windows;
using System.Windows.Controls;

namespace StreamerHelpDeskClient
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        private readonly SignalRClientService _signalRService;
        private readonly ClientConfigService _configService;

        public Window1(SignalRClientService signalRService, ClientConfigService configService)
        {
            InitializeComponent();

            _signalRService = signalRService;
            _configService = configService;

            Closed += Window1_Closed;
        }

        private async void Window1_Closed(object? sender, EventArgs e)
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
