using StreamerHelpDeskClient.Models;
using StreamerHelpDeskClient.Services;
using System.Windows;

namespace StreamerHelpDeskClient.Views
{
    /// <summary>
    /// Interaction logic for ConfigurationWindow.xaml
    /// </summary>
    public partial class ConfigurationWindow : Window
    {
        private readonly ClientConfigService _configService;

        public ConfigurationWindow(ClientConfigService configService)
        {
            InitializeComponent();

            _configService = configService;

            var config = _configService.Load();
            ServerIPTextBox.Text = config.ServerIP;
            PortNumberTextBox.Text = config.PortNumber.ToString();
            ClientNameTextBox.Text = config.ClientName;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            var serverIP = ServerIPTextBox.Text.Trim();
            if (string.IsNullOrEmpty(serverIP))
            {
                ShowValidationError("Server IP cannot be empty.");
                return;
            }

            if (!int.TryParse(PortNumberTextBox.Text, out int port) || port is <= 0 or > 65535)
            {
                ShowValidationError("Please enter a valid port number (1–65535).");
                return;
            }

            var clientName = ClientNameTextBox.Text.Trim();
            if (string.IsNullOrEmpty(clientName))
            {
                ShowValidationError("Client Name cannot be empty.");
                return;
            }

            var config = new ClientConfig
            {
                ServerIP = serverIP,
                PortNumber = port,
                ClientName = clientName
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
