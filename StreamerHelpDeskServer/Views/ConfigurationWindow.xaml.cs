using StreamerHelpDeskServer.Models;
using StreamerHelpDeskServer.Services;
using System.Windows;

namespace StreamerHelpDeskServer.Views
{
    /// <summary>
    /// Interaction logic for ConfigurationWindow.xaml
    /// </summary>
    public partial class ConfigurationWindow : Window
    {
        private readonly ServerConfigService _configService;

        public ConfigurationWindow(ServerConfigService configService)
        {
            InitializeComponent();

            _configService = configService;

            var config = _configService.Load();
            HostTextBox.Text = config.Host;
            PortTextBox.Text = config.Port.ToString();
            HubPathTextBox.Text = config.HubPath;
            MaxConnectionsTextBox.Text = config.MaxConnections.ToString();
            KeepAliveTextBox.Text = config.KeepAliveIntervalSeconds.ToString();
            ClientTimeoutTextBox.Text = config.ClientTimeoutSeconds.ToString();
            DetailedErrorsCheckBox.IsChecked = config.EnableDetailedErrors;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            var host = HostTextBox.Text.Trim();
            if (string.IsNullOrEmpty(host))
            {
                ShowValidationError("Host / Bind Address cannot be empty.");
                return;
            }

            if (!int.TryParse(PortTextBox.Text, out int port) || port is <= 0 or > 65535)
            {
                ShowValidationError("Please enter a valid port number (1–65535).");
                return;
            }

            var hubPath = HubPathTextBox.Text.Trim();
            if (string.IsNullOrEmpty(hubPath) || !hubPath.StartsWith('/'))
            {
                ShowValidationError("Hub Path must start with '/'.");
                return;
            }

            if (!int.TryParse(MaxConnectionsTextBox.Text, out int maxConnections) || maxConnections < 0)
            {
                ShowValidationError("Max Connections must be 0 (unlimited) or a positive number.");
                return;
            }

            if (!int.TryParse(KeepAliveTextBox.Text, out int keepAlive) || keepAlive <= 0)
            {
                ShowValidationError("Keep-Alive Interval must be a positive number of seconds.");
                return;
            }

            if (!int.TryParse(ClientTimeoutTextBox.Text, out int clientTimeout) || clientTimeout <= 0)
            {
                ShowValidationError("Client Timeout must be a positive number of seconds.");
                return;
            }

            var config = new ServerConfig
            {
                Host = host,
                Port = port,
                HubPath = hubPath,
                MaxConnections = maxConnections,
                KeepAliveIntervalSeconds = keepAlive,
                ClientTimeoutSeconds = clientTimeout,
                EnableDetailedErrors = DetailedErrorsCheckBox.IsChecked == true
            };

            _configService.Save(config);
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private static void ShowValidationError(string message)
        {
            MessageBox.Show(message, "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}
