using Discord;
using Discord.Webhook;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;

namespace Lita_Logging
{
    class Program
    {

        static TwitchClient Client;
        static DiscordWebhookClient Webhook;
        static Config Config;
        static void Main(string[] args)
        {
            Config = Config.MakeConfig("./config.json");
            Client = new TwitchClient();
            Client.Initialize(new ConnectionCredentials(Config.BotUsername, Config.BotOAuth), Config.LoggingChannel);
            Webhook = new DiscordWebhookClient(Config.DiscordWebhook);
            Client.OnMessageReceived += Client_OnMessageReceived;
            Client.OnConnected += Client_OnConnected;
            Client.Connect();
            Run().GetAwaiter().GetResult();
        }

        public static async Task Run()
        {
           await Task.Delay(-1);
        }

        private static void Client_OnConnected(object sender, OnConnectedArgs e)
        {
            Console.WriteLine(e.BotUsername + " connected to " + e.AutoJoinChannel);
        }

        private static void Client_OnMessageReceived(object sender, OnMessageReceivedArgs e)
        {
            if (e.ChatMessage.Channel.Equals(Config.LoggingChannel)) {

                var embed = new EmbedBuilder();
                string author = ((!e.ChatMessage.UserType.ToString().Equals("Viewer"))?"[" + e.ChatMessage.UserType + "] ":"") + e.ChatMessage.DisplayName;
                embed.WithAuthor(author,url:"https://twitch.tv/" + e.ChatMessage.Username);
                embed.WithDescription(e.ChatMessage.Message);
                embed.WithCurrentTimestamp();
                embed.WithFooter(e.ChatMessage.Id);
                

                var list = new List<Embed>
                {
                    embed.Build()
                };

                Webhook.SendMessageAsync(embeds: list,username:e.ChatMessage.Channel + " Chat");
            }
            

        }
    }
}
