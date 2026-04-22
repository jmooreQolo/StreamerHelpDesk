using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using StreamerHelpDeskServer.Hubs;
using StreamerHelpDeskServer.Models;
using StreamerHelpDeskServer.ViewModels;

namespace StreamerHelpDeskServer.Services
{
    public class SignalRHostService
    {
        private readonly MainWindowViewModel _viewModel;
        private readonly LoggingService _loggingService;
        private WebApplication? _app;

        public SignalRHostService(MainWindowViewModel viewModel, LoggingService loggingService)
        {
            _viewModel = viewModel;
            _loggingService = loggingService;
        }

        public bool IsRunning => _app != null;

        public IHubContext<HelpDeskHub>? GetHubContext()
        {
            return _app?.Services.GetService<IHubContext<HelpDeskHub>>();
        }

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
            builder.Services.AddSingleton(_loggingService);

            _app = builder.Build();
            _app.MapHub<HelpDeskHub>(config.HubPath);

            _loggingService.LogServerEvent($"SignalR server starting on {config.Host}:{config.Port}");
            await _app.StartAsync();
            _loggingService.LogServerEvent("SignalR server started successfully");
        }

        public async Task StopAsync()
        {
            if (_app != null)
            {
                _loggingService.LogServerEvent("SignalR server stopping");
                await _app.StopAsync();
                await _app.DisposeAsync();
                _app = null;
                _loggingService.LogServerEvent("SignalR server stopped");
            }
        }
    }
}
