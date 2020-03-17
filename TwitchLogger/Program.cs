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

            //Pulling Config from directory
            Config = Config.MakeConfig("./config.json");

            //Initializing Clients
            Client = new TwitchClient();
            Client.Initialize(new ConnectionCredentials(Config.BotUsername, Config.BotOAuth), Config.LoggingChannel);
            Webhook = new DiscordWebhookClient(Config.DiscordWebhook);

            //Forwarding Events
            Client.OnMessageReceived += Client_OnMessageReceived;
            Client.OnConnected += Client_OnConnected;

            //Connect Client to Twitch IRC
            Client.Connect();

            //Run for Indef 
            Run().GetAwaiter().GetResult();
        }

        public static async Task Run()
        {
           await Task.Delay(-1);
        }

        private static void Client_OnConnected(object sender, OnConnectedArgs e)
        {
            //Log Connecting to Twitch
            Console.WriteLine(e.BotUsername + " connected to " + e.AutoJoinChannel);
        }

        private static void Client_OnMessageReceived(object sender, OnMessageReceivedArgs e)
        {

            //Incase the Client Somehow connected to other sources
            if (e.ChatMessage.Channel.Equals(Config.LoggingChannel)) {

                
                var embed = new EmbedBuilder();

                //Making String for Author that Declares User Status if Higher then Viewer
                string author = ((!e.ChatMessage.UserType.ToString().Equals("Viewer"))?"[" + e.ChatMessage.UserType + "] ":"") + e.ChatMessage.DisplayName;

                //Set Author with Link to User Page
                embed.WithAuthor(author,url:"https://twitch.tv/" + e.ChatMessage.Username);
                embed.WithDescription(e.ChatMessage.Message);
                embed.WithCurrentTimestamp();

                //Chat Message Id for "/delete {msg-id}" command
                embed.WithFooter(e.ChatMessage.Id);

                //Using a List because Webhooks can technically send up to 5 embeds
                var list = new List<Embed>
                {
                    embed.Build()
                };

                //Send Message to Discord Pipeline
                Webhook.SendMessageAsync(embeds: list,username:e.ChatMessage.Channel + " Chat");
            }
            

        }
    }
}
