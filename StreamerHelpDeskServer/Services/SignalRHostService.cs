using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.Extensions.DependencyInjection;
using StreamerHelpDeskServer.Hubs;
using StreamerHelpDeskServer.Models;
using StreamerHelpDeskServer.ViewModels;

namespace StreamerHelpDeskServer.Services
{
    public class SignalRHostService
    {
        private readonly MainWindowViewModel _viewModel;
        private WebApplication? _app;

        public SignalRHostService(MainWindowViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public bool IsRunning => _app != null;

        public async Task StartAsync(ServerConfig config)
        {
            if (_app != null)
                return;

            var builder = WebApplication.CreateBuilder();
            builder.WebHost.UseUrls($"http://{config.Host}:{config.Port}");

            if (config.MaxConnections > 0)
            {
                builder.WebHost.ConfigureKestrel(options =>
                {
                    options.Limits.MaxConcurrentConnections = config.MaxConnections;
                });
            }

            builder.Services.AddSignalR(options =>
            {
                options.KeepAliveInterval = TimeSpan.FromSeconds(config.KeepAliveIntervalSeconds);
                options.ClientTimeoutInterval = TimeSpan.FromSeconds(config.ClientTimeoutSeconds);
                options.EnableDetailedErrors = config.EnableDetailedErrors;
            });
            builder.Services.AddSingleton(_viewModel);

            _app = builder.Build();
            _app.MapHub<HelpDeskHub>(config.HubPath);

            await _app.StartAsync();
        }

        public async Task StopAsync()
        {
            if (_app != null)
            {
                await _app.StopAsync();
                await _app.DisposeAsync();
                _app = null;
            }
        }
    }
}
