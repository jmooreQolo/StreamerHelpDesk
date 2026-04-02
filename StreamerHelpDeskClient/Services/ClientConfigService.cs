using StreamerHelpDeskClient.Models;
using System.IO;
using System.Text.Json;

namespace StreamerHelpDeskClient.Services
{
    public class ClientConfigService
    {
        private static readonly string ConfigPath =
            Path.Combine(AppContext.BaseDirectory, "clientconfig.json");

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true
        };

        public ClientConfig Load()
        {
            if (File.Exists(ConfigPath))
            {
                var json = File.ReadAllText(ConfigPath);
                return JsonSerializer.Deserialize<ClientConfig>(json, JsonOptions) ?? new ClientConfig();
            }

            return new ClientConfig();
        }

        public void Save(ClientConfig config)
        {
            var json = JsonSerializer.Serialize(config, JsonOptions);
            File.WriteAllText(ConfigPath, json);
        }
    }
}
