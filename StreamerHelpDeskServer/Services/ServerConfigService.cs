using StreamerHelpDeskServer.Models;
using System.IO;
using System.Text.Json;

namespace StreamerHelpDeskServer.Services
{
    public class ServerConfigService
    {
        private static readonly string ConfigPath =
            Path.Combine(AppContext.BaseDirectory, "serverconfig.json");

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true
        };

        public ServerConfig Load()
        {
            if (File.Exists(ConfigPath))
            {
                var json = File.ReadAllText(ConfigPath);
                return JsonSerializer.Deserialize<ServerConfig>(json, JsonOptions) ?? new ServerConfig();
            }

            return new ServerConfig();
        }

        public void Save(ServerConfig config)
        {
            var json = JsonSerializer.Serialize(config, JsonOptions);
            File.WriteAllText(ConfigPath, json);
        }
    }
}
