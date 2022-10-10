
using System.IO;
using System.Text.Json;

namespace TwitchLogger
{
    public class Config
    {

        public string BotUsername { get; set; }

        public string BotOAuth { get; set; }

        public string APIClientId { get; set; }

        public string APISecret { get; set;}

        public string LoggingChannel { get;  set; }

        public string ChatWebhook { get;  set; }

        public string StatsWebhook { get; set; }

        public static Config MakeConfig(string filepath)
        {
            var s = File.ReadAllText(filepath);
            var Config = JsonSerializer.Deserialize<Config>(s);
            return Config;
        }

    }
}
