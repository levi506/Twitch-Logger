using Discord;
using Discord.Webhook;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Events;

namespace Lita_Logging
{
    public static class Program
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

            Client.ForwardEvents();

            //Connect Client to Twitch IRC
            Client.Connect();

            //Run for Indef 
            Run().GetAwaiter().GetResult();
        }

        //Subscriber Events
        private static void Client_OnReSubscriber(object sender, OnReSubscriberArgs e)
        {
            //Incase the Client Somehow connected to other sources
            if (e.Channel.Equals(Config.LoggingChannel))
            {
                //Making String for Author that Declares User Status if Higher then Viewer
                string author = ((!e.ReSubscriber.UserType.ToString().Equals("Viewer")) ? "[" + e.ReSubscriber.UserType + "] " : "") + e.ReSubscriber.DisplayName;

                //Making Url to Author Page
                var authorUrl = "https://twitch.tv/" + e.ReSubscriber.Login;

                //Chat Message Id for "/delete {msg-id}" command
                var footer = e.ReSubscriber.Id;
                var desc = e.ReSubscriber.SystemMessageParsed + ((e.ReSubscriber.ResubMessage.Length > 0) ? "```" + e.ReSubscriber.ResubMessage + "```" : "");
                //Send Message to Discord Pipeline
                CreateAndSendEmbed(author, authorUrl, desc, footer);
            }
        }

        private static void Client_OnNewSubscriber(object sender, OnNewSubscriberArgs e)
        {
            //Incase the Client Somehow connected to other sources
            if (e.Channel.Equals(Config.LoggingChannel))
            {
                //Making String for Author that Declares User Status if Higher then Viewer
                string author = ((!e.Subscriber.UserType.ToString().Equals("Viewer")) ? "[" + e.Subscriber.UserType + "] " : "") + e.Subscriber.DisplayName;

                //Making Url to Author Page
                var authorUrl = "https://twitch.tv/" + e.Subscriber.Login;

                //Chat Message Id for "/delete {msg-id}" command
                var footer = e.Subscriber.Id;
                var desc = e.Subscriber.SystemMessageParsed + ((e.Subscriber.ResubMessage.Length > 0) ? "```" + e.Subscriber.ResubMessage + "```" : "");
                //Send Message to Discord Pipeline
                CreateAndSendEmbed(author, authorUrl, desc, footer);
            }
        }

        private static void Client_OnGiftedSubscription(object sender, OnGiftedSubscriptionArgs e)
        {
            //Incase the Client Somehow connected to other sources
            if (e.Channel.Equals(Config.LoggingChannel))
            {
                //Making String for Author that Declares User Status if Higher then Viewer
                string author = ((!e.GiftedSubscription.UserType.ToString().Equals("Viewer")) ? "[" + e.GiftedSubscription.UserType + "] " : "") + e.GiftedSubscription.DisplayName;

                //Making Url to Author Page
                var authorUrl = "https://twitch.tv/" + e.GiftedSubscription.Login;

                //Chat Message Id for "/delete {msg-id}" command
                var footer = e.GiftedSubscription.Id;

                //Send Message to Discord Pipeline
                CreateAndSendEmbed(author, authorUrl, e.GiftedSubscription.SystemMsgParsed, footer);
            }
        }

        private static void Client_OnCommunitySubscription(object sender, OnCommunitySubscriptionArgs e)
        {
            //Incase the Client Somehow connected to other sources
            if (e.Channel.Equals(Config.LoggingChannel))
            {
                //Making String for Author that Declares User Status if Higher then Viewer
                string author = ((!e.GiftedSubscription.UserType.ToString().Equals("Viewer")) ? "[" + e.GiftedSubscription.UserType + "] " : "") + e.GiftedSubscription.DisplayName;

                //Making Url to Author Page
                var authorUrl = "https://twitch.tv/" + e.GiftedSubscription.Login;

                //Chat Message Id for "/delete {msg-id}" command
                var footer = e.GiftedSubscription.Id;

                //Send Message to Discord Pipeline
                CreateAndSendEmbed(author, authorUrl, e.GiftedSubscription.SystemMsgParsed, footer);
            }
        }

        private static void Client_OnRaidNotification(object sender, OnRaidNotificationArgs e)
        {
            //Incase the Client Somehow connected to other sources
            if (e.Channel.Equals(Config.LoggingChannel))
            {
                //Making String for Author that Declares User Status if Higher then Viewer
                string author = ((!e.RaidNotification.UserType.ToString().Equals("Viewer")) ? "[" + e.RaidNotification.UserType + "] " : "") + e.RaidNotification.DisplayName;

                //Making Url to Author Page
                var authorUrl = "https://twitch.tv/" + e.RaidNotification.Login;

                //Chat Message Id for "/delete {msg-id}" command
                var footer = e.RaidNotification.Id;

                //Send Message to Discord Pipeline
                CreateAndSendEmbed(author, authorUrl, e.RaidNotification.SystemMsgParsed, footer);
            }
        }


        //Moderator Actions
        private static void Client_OnUserBanned(object sender, OnUserBannedArgs e)
        {
            //Incase the Client Somehow connected to other sources
            if (e.UserBan.Channel.Equals(Config.LoggingChannel))
            {

                //Making String for Author that Declares User Status if Higher then Viewer
                string author = e.UserBan.Username;

                //Set Author with Link to User Page
                var authorUrl = "https://twitch.tv/" + e.UserBan.Username;
                var Desc = "User Banned";
                var footer = "";

                //Send Message to Discord Pipeline
                CreateAndSendEmbed(author, authorUrl, Desc, footer);
            }
        }

        private static void Client_OnUserTimedout(object sender, OnUserTimedoutArgs e)
        {
            //Incase the Client Somehow connected to other sources
            if (e.UserTimeout.Channel.Equals(Config.LoggingChannel))
            {

                //Making String for Author that Declares User Status if Higher then Viewer
                string author = e.UserTimeout.Username;

                //Set Author with Link to User Page
                var authorUrl ="https://twitch.tv/" + e.UserTimeout.Username;
                var Desc = "Timed Out for " + e.UserTimeout.TimeoutDuration + " seconds";
                var footer = "";

                //Send Message to Discord Pipeline
                CreateAndSendEmbed(author, authorUrl, Desc, footer);
            }
        }

        //Client Issues
        private static void Client_OnError(object sender, OnErrorEventArgs e)
        {
            LogMessage(Client.TwitchUsername, e.Exception.Message + "/n" + e.Exception.StackTrace, "Exception");
        }

        private static async Task Client_OnDisconnected(object sender, OnDisconnectedEventArgs e)
        {
            LogMessage(Client.TwitchUsername, "The Client has Disconnected");
            while (!CheckForInternetConnection())
            {
                await Task.Delay(1000);
            }
            Client.Reconnect();
            Webhook = new DiscordWebhookClient(Config.DiscordWebhook);
        }

        private static async Task Client_OnConnectionError(object sender, OnConnectionErrorArgs e)
        {
            LogMessage(e.BotUsername, e.Error.Message, "Error");
            while (!CheckForInternetConnection())
            {
                await Task.Delay(1000);
            }
            Client.Connect();
            Webhook = new DiscordWebhookClient(Config.DiscordWebhook);

        }

        private static void Client_OnFailureToReceiveJoinConfirmation(object sender, OnFailureToReceiveJoinConfirmationArgs e)
        {
            LogMessage(Client.TwitchUsername,"<"+ e.Exception.Channel +"> " + e.Exception.Details,"Exception");
        }

        public static async Task Run()
        {
           await Task.Delay(-1);
        }

        private static void Client_OnConnected(object sender, OnConnectedArgs e)
        {
            //Log Connecting to Twitch
            LogMessage(e.BotUsername, "Connetion Opened to " + e.AutoJoinChannel);
        }

        private static void Client_OnMessageReceived(object sender, OnMessageReceivedArgs e)
        {

            //Incase the Client Somehow connected to other sources
            if (e.ChatMessage.Channel.Equals(Config.LoggingChannel)) {
                //Making String for Author that Declares User Status if Higher then Viewer
                string author = ((!e.ChatMessage.UserType.ToString().Equals("Viewer"))?"[" + e.ChatMessage.UserType + "] ":"") + e.ChatMessage.DisplayName;

                //Making Url to Author Page
                var authorUrl  = "https://twitch.tv/" + e.ChatMessage.Username;

                //Chat Message Id for "/delete {msg-id}" command
                var footer = e.ChatMessage.Id;

                //Send Message to Discord Pipeline
                CreateAndSendEmbed(author,authorUrl,e.ChatMessage.Message,footer);
            }

        }

        private static void CreateAndSendEmbed(string author,string authorUrl,string description, string footer)
        {
            var embed = new EmbedBuilder();

            //Set Author with Link to User Page
            embed.WithAuthor(author, url: Uri.EscapeUriString(authorUrl));
            embed.WithDescription(description);
            embed.WithCurrentTimestamp();

            //Chat Message Id for "/delete {msg-id}" command
            embed.WithFooter(footer);

            //Using a List because Webhooks can technically send up to 5 embeds
            var list = new List<Embed>
                {
                    embed.Build()
                };

            //Send Message to Discord Pipeline
            Webhook.SendMessageAsync(embeds: list, username: Config.LoggingChannel + " Chat");
        }

        private static void ForwardEvents(this TwitchClient client)
        {
            //Forwarding Events
            client.OnConnected += Client_OnConnected;
            client.OnDisconnected += ( sender,  e) => { Client_OnDisconnected(sender, e); };

            //Error
            client.OnError += Client_OnError;
            client.OnConnectionError += (sender, e) => { Client_OnConnectionError(sender, e); };
            client.OnFailureToReceiveJoinConfirmation += Client_OnFailureToReceiveJoinConfirmation;

            //Messages
            client.OnReSubscriber += Client_OnReSubscriber;
            client.OnNewSubscriber += Client_OnNewSubscriber;
            client.OnMessageReceived += Client_OnMessageReceived;
            client.OnRaidNotification += Client_OnRaidNotification;
            client.OnGiftedSubscription += Client_OnGiftedSubscription;
            client.OnCommunitySubscription += Client_OnCommunitySubscription;

            //Mod Action
            client.OnUserBanned += Client_OnUserBanned;
            client.OnUserTimedout += Client_OnUserTimedout;
            client.AutoReListenOnException = true;
        }

        private static void LogMessage(string bot, string message, string type = "Info")
        {
            Console.WriteLine($"[{type}] <{bot}> - {message}");
        }

        private static bool CheckForInternetConnection()
        {
            try
            {
                using (var client = new WebClient())
                using (client.OpenRead("http://google.com/generate_204"))
                    return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
