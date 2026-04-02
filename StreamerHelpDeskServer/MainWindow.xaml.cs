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

        public MainWindow(
            MainWindowViewModel viewModel,
            SignalRHostService signalRService,
            ServerConfigService configService)
        {
            InitializeComponent();
            DataContext = viewModel;

            _viewModel = viewModel;
            _signalRService = signalRService;
            _configService = configService;
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

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
