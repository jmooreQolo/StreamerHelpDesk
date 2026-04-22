using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace StreamerHelpDeskServer.Views
{
    /// <summary>
    /// Interaction logic for ClientSelectionWindow.xaml
    /// </summary>
    public partial class ClientSelectionWindow : Window
    {
        public string? SelectedConnectionId { get; private set; }
        public string? SelectedClientName { get; private set; }

        public ClientSelectionWindow(Dictionary<string, string> connectedClients)
        {
            InitializeComponent();

            ClientComboBox.ItemsSource = connectedClients;
            if (connectedClients.Any())
            {
                ClientComboBox.SelectedIndex = 0;
            }
        }

        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            if (ClientComboBox.SelectedItem is KeyValuePair<string, string> selected)
            {
                SelectedConnectionId = selected.Key;
                SelectedClientName = selected.Value;
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Please select a client.", "Selection Required",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
