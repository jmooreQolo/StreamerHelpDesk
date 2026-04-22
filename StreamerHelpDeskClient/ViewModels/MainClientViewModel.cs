using CommunityToolkit.Mvvm.ComponentModel;

namespace StreamerHelpDeskClient.ViewModels
{
    public partial class MainClientViewModel : ObservableObject
    {
        [ObservableProperty]
        private bool _isConnected;
    }
}
