using Discord;
using Discord.Webhook;
using System;
using System.Collections.Generic;
using TwitchLib.Client;
using TwitchLib.Client.Events;

namespace TwitchLogger
{
    public class ChatTracker
    {
        private DiscordWebhookClient ChatFeed { get; set; }
        private string LoggingChannel { get; set; }
        public ChatTracker(TwitchClient client, string Webhook, string logChannel)
        {
            ChatFeed = new DiscordWebhookClient(Webhook);
            LoggingChannel = logChannel;
            client.TrackChat(this);
        }

        private void CreateAndSendEmbed(string author, string authorUrl, string description, string footer, Color color)
        {
            var embed = new EmbedBuilder();

            //Set Author with Link to User Page
            embed.WithAuthor(author, url: Uri.EscapeUriString(authorUrl));
            embed.WithDescription(description);
            embed.WithCurrentTimestamp();

            //Chat Message Id for "/delete {msg-id}" command
            embed.WithFooter(footer);

            embed.WithColor(color);

            //Using a List because Webhooks can technically send up to 5 embeds
            var list = new List<Embed>
                {
                    embed.Build()
                };

            //Send Message to Discord Pipeline
            ChatFeed.SendMessageAsync(embeds: list, username: LoggingChannel + " Chat");
        }

        public void Client_OnMessageReceived(object sender, OnMessageReceivedArgs e)
        {
            //Incase the Client Somehow connected to other sources
            if (e.ChatMessage.Channel.Equals(LoggingChannel))
            {
                //Making String for Author that Declares User Status if Higher then Viewer
                string author = ((!e.ChatMessage.UserType.ToString().Equals("Viewer")) ? "[" + e.ChatMessage.UserType + "] " : "") + e.ChatMessage.DisplayName;

                //Making Url to Author Page
                var authorUrl = "https://twitch.tv/" + e.ChatMessage.Username;

                //Chat Message Id for "/delete {msg-id}" command
                var footer = e.ChatMessage.Id;

                //Send Message to Discord Pipeline
                CreateAndSendEmbed(author, authorUrl, e.ChatMessage.Message, footer, Color.DarkBlue);
            }

        }

        //Subscriber Events
        public void Client_OnReSubscriber(object sender, OnReSubscriberArgs e)
        {
            //Incase the Client Somehow connected to other sources
            if (e.Channel.Equals(LoggingChannel))
            {
                //Making String for Author that Declares User Status if Higher then Viewer
                string author = ((!e.ReSubscriber.UserType.ToString().Equals("Viewer")) ? "[" + e.ReSubscriber.UserType + "] " : "") + e.ReSubscriber.DisplayName;

                //Making Url to Author Page
                var authorUrl = "https://twitch.tv/" + e.ReSubscriber.Login;

                //Chat Message Id for "/delete {msg-id}" command
                var footer = e.ReSubscriber.Id;
                var desc = e.ReSubscriber.SystemMessageParsed + ((e.ReSubscriber.ResubMessage.Length > 0) ? "```" + e.ReSubscriber.ResubMessage + "```" : "");
                //Send Message to Discord Pipeline
                CreateAndSendEmbed(author, authorUrl, desc, footer, Color.DarkGreen);
            }
        }

        public void Client_OnNewSubscriber(object sender, OnNewSubscriberArgs e)
        {
            //Incase the Client Somehow connected to other sources
            if (e.Channel.Equals(LoggingChannel))
            {
                //Making String for Author that Declares User Status if Higher then Viewer
                string author = ((!e.Subscriber.UserType.ToString().Equals("Viewer")) ? "[" + e.Subscriber.UserType + "] " : "") + e.Subscriber.DisplayName;

                //Making Url to Author Page
                var authorUrl = "https://twitch.tv/" + e.Subscriber.Login;

                //Chat Message Id for "/delete {msg-id}" command
                var footer = e.Subscriber.Id;
                var desc = e.Subscriber.SystemMessageParsed + ((e.Subscriber.ResubMessage.Length > 0) ? "```" + e.Subscriber.ResubMessage + "```" : "");
                //Send Message to Discord Pipeline
                CreateAndSendEmbed(author, authorUrl, desc, footer, Color.Green);
            }
        }

        public void Client_OnGiftedSubscription(object sender, OnGiftedSubscriptionArgs e)
        {
            //Incase the Client Somehow connected to other sources
            if (e.Channel.Equals(LoggingChannel))
            {
                //Making String for Author that Declares User Status if Higher then Viewer
                string author = ((!e.GiftedSubscription.UserType.ToString().Equals("Viewer")) ? "[" + e.GiftedSubscription.UserType + "] " : "") + e.GiftedSubscription.DisplayName;

                //Making Url to Author Page
                var authorUrl = "https://twitch.tv/" + e.GiftedSubscription.Login;

                //Chat Message Id for "/delete {msg-id}" command
                var footer = e.GiftedSubscription.Id;

                //Send Message to Discord Pipeline
                CreateAndSendEmbed(author, authorUrl, e.GiftedSubscription.SystemMsgParsed, footer, Color.Teal);
            }
        }

        public void Client_OnCommunitySubscription(object sender, OnCommunitySubscriptionArgs e)
        {
            //Incase the Client Somehow connected to other sources
            if (e.Channel.Equals(LoggingChannel))
            {
                //Making String for Author that Declares User Status if Higher then Viewer
                string author = ((!e.GiftedSubscription.UserType.ToString().Equals("Viewer")) ? "[" + e.GiftedSubscription.UserType + "] " : "") + e.GiftedSubscription.DisplayName;

                //Making Url to Author Page
                var authorUrl = "https://twitch.tv/" + e.GiftedSubscription.Login;

                //Chat Message Id for "/delete {msg-id}" command
                var footer = e.GiftedSubscription.Id;

                //Send Message to Discord Pipeline
                CreateAndSendEmbed(author, authorUrl, e.GiftedSubscription.SystemMsgParsed, footer, Color.Teal);
            }
        }

        public void Client_OnBeingHosted(object sender, OnBeingHostedArgs e)
        {
            //Incase the Client Somehow connected to other sources
            if (e.BeingHostedNotification.Channel.Equals(LoggingChannel))
            {
                //Making String for Author that Declares User Status if Higher then Viewer
                string author = e.BeingHostedNotification.HostedByChannel;

                //Making Url to Author Page
                var authorUrl = "https://twitch.tv/" + e.BeingHostedNotification.HostedByChannel;

                //Constructing Description Body
                var desc = $"{e.BeingHostedNotification.HostedByChannel} hosted the channel with {e.BeingHostedNotification.Viewers} viewers!";

                var footer = e.BeingHostedNotification.IsAutoHosted ? "Auto-hosted" : "Manual/Raid Host";

                //Send Message to Discord Pipeline
                CreateAndSendEmbed(author, authorUrl, desc, footer, Color.Magenta);
            }
        }

        public void Client_OnRaidNotification(object sender, OnRaidNotificationArgs e)
        {
            //Incase the Client Somehow connected to other sources
            if (e.Channel.Equals(LoggingChannel))
            {
                //Making String for Author that Declares User Status if Higher then Viewer
                string author = ((!e.RaidNotification.UserType.ToString().Equals("Viewer")) ? "[" + e.RaidNotification.UserType + "] " : "") + e.RaidNotification.DisplayName;

                //Making Url to Author Page
                var authorUrl = "https://twitch.tv/" + e.RaidNotification.Login;

                //Constructing Description Body
                var desc = $"{e.RaidNotification.DisplayName} raided the channel with {e.RaidNotification.MsgParamViewerCount} viewers!";

                //Chat Message Id for "/delete {msg-id}" command
                var footer = e.RaidNotification.Id;

                //Send Message to Discord Pipeline
                CreateAndSendEmbed(author, authorUrl, desc, footer, Color.Magenta);
            }
        }

        //Moderator Actions
        public void Client_OnUserBanned(object sender, OnUserBannedArgs e)
        {
            //Incase the Client Somehow connected to other sources
            if (e.UserBan.Channel.Equals(LoggingChannel))
            {

                //Making String for Author that Declares User Status if Higher then Viewer
                string author = e.UserBan.Username;

                //Set Author with Link to User Page
                var authorUrl = "https://twitch.tv/" + e.UserBan.Username;
                var Desc = "User Banned";
                var footer = "";

                //Send Message to Discord Pipeline
                CreateAndSendEmbed(author, authorUrl, Desc, footer, Color.Red);
            }
        }

        public void Client_OnUserTimedout(object sender, OnUserTimedoutArgs e)
        {
            //Incase the Client Somehow connected to other sources
            if (e.UserTimeout.Channel.Equals(LoggingChannel))
            {

                //Making String for Author that Declares User Status if Higher then Viewer
                string author = e.UserTimeout.Username;

                //Set Author with Link to User Page
                var authorUrl = "https://twitch.tv/" + e.UserTimeout.Username;
                var Desc = "Timed Out for " + e.UserTimeout.TimeoutDuration + " seconds";
                var footer = "";

                //Send Message to Discord Pipeline
                CreateAndSendEmbed(author, authorUrl, Desc, footer, Color.Orange);
            }
        }

    } 
}
