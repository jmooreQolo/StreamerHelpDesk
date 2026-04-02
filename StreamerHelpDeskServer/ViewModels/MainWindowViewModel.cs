using CommunityToolkit.Mvvm.ComponentModel;
using StreamerHelpDeskServer.Models;
using System.Collections.ObjectModel;
using System.Windows;

namespace StreamerHelpDeskServer.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        public ObservableCollection<HelpDeskMessage> Messages { get; } = new();

        public void AddMessage(HelpDeskMessage message)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Messages.Add(message);
            });
        }
    }
}
