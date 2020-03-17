
using System.IO;
using System.Text.Json;

namespace Lita_Logging
{
    public class Config
    {

        public string BotUsername { get;  set; }

        public string BotOAuth { get;  set; }

        public string LoggingChannel { get;  set; }

        public string DiscordWebhook { get;  set; }

        public static Config MakeConfig(string filepath)
        {
            var s = File.ReadAllText(filepath);
            var Config = JsonSerializer.Deserialize<Config>(s);
            return Config;
        }

    }
}
