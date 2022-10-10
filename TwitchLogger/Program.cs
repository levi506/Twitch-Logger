using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Timers;
using TwitchLib.Api;
using TwitchLib.Api.Services;
using TwitchLib.Api.Services.Events.LiveStreamMonitor;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Events;

namespace TwitchLogger
{
    public static class Program
    {
        //Core Info
        private static TwitchClient Client;
        private static TwitchAPI Api;
        private static Config Config;
        private static ChatTracker Chat;
        private static StatsTracker Stats;

        //Stats Reporting Vars
        static LiveStreamMonitorService LiveMonitor;
        static bool StreamUp;
        static Timer CountDownStats;

        static void Main(string[] args)
        {
            //Pulling Config from directory
            Config = Config.MakeConfig("./config.json");

            //Initializing Clients
            Build();
            
            //Run for Indef 
            Run().GetAwaiter().GetResult();
        }

        public static void Clean()
        {

            LiveMonitor.Stop();
            CountDownStats.Stop();

            Build();
        }

        public static void Build()
        {
            //Chat Client
            Client = new TwitchClient();
            Client.Initialize(new ConnectionCredentials(Config.BotUsername, Config.BotOAuth), Config.LoggingChannel);
            Client.ClientSetup();
            Client.AutoReListenOnException = true;

            //Api Client
            Api = new TwitchAPI();
            Api.Settings.ClientId = Config.APIClientId;
            Api.Settings.Secret = Config.APISecret;

            //Core Stream State
            StreamUp = false;

            //Live Monitor
            LiveMonitor = new LiveStreamMonitorService(Api, 180);
            List<string> ChannelMonitorList = new List<string>();
            ChannelMonitorList.Add(Config.LoggingChannel);
            LiveMonitor.SetChannelsByName(ChannelMonitorList);
            LiveMonitor.OnStreamOnline += LiveMonitor_OnStreamOnline;
            LiveMonitor.OnStreamOffline += LiveMonitor_OnStreamOffline;
            LiveMonitor.Start();

            //Notifying Stats Tracker
            CountDownStats = new Timer(900000);
            CountDownStats.AutoReset = false;
            CountDownStats.Elapsed += CountDownStats_Elapsed;

            //Creating Trackers
            if (Chat == null)
            {
                Chat = new ChatTracker(Client, Config.ChatWebhook, Config.LoggingChannel);
                Stats = new StatsTracker(Client, Config.StatsWebhook);
            } else
            {
                Client.TrackChat(Chat);
                Client.TrackChat(Stats);
            }

            //Connect Client to Twitch IRC
            Client.Connect();
            Client.JoinChannel(Config.LoggingChannel);
        }

        private static void CountDownStats_Elapsed(object sender, ElapsedEventArgs e)
        {
            LogMessage(Config.BotUsername, "Stream Has been down 15 minutes attempting to calc stats");
            Stats.Close();
        }

        private static void LiveMonitor_OnStreamOffline(object sender, OnStreamOfflineArgs e)
        {
            if (e.Channel == Config.LoggingChannel)
            {
                LogMessage(Config.BotUsername, "Logged Stream detected as down");
                StreamUp = false;
                CountDownStats.Start();
            }
        }

        private static void LiveMonitor_OnStreamOnline(object sender, OnStreamOnlineArgs e)
        {
            if(e.Channel == Config.LoggingChannel)
            {
                LogMessage(Config.BotUsername, "Logged Stream detected as up");
                StreamUp = true;
                CountDownStats.Stop();
                Stats.Open();
            }
        }

        //Standard Events
        public static void Client_OnConnected(object sender, OnConnectedArgs e)
        {
            //Log Connecting to Twitch
            LogMessage(e.BotUsername, "Connetion Opened to " + e.AutoJoinChannel);
        }


        //Client Issues
        public static void Client_OnError(object sender, OnErrorEventArgs e)
        {
            LogMessage(Client.TwitchUsername, e.Exception.Message + "/n" + e.Exception.StackTrace, "Exception");
        }

        public static async Task Client_OnDisconnected(object sender, OnDisconnectedEventArgs e)
        {
            LogMessage(Client.TwitchUsername, "The Client has Disconnected");
            while (!CheckForInternetConnection())
            {
                await Task.Delay(10000);
            }
            Clean();

        }

        public static async Task Client_OnConnectionError(object sender, OnConnectionErrorArgs e)
        {
            LogMessage(e.BotUsername, e.Error.Message, "Error");
            while (!CheckForInternetConnection())
            {
                await Task.Delay(1000);
            }
            Client.Disconnect();

        }

        public static void Client_OnFailureToReceiveJoinConfirmation(object sender, OnFailureToReceiveJoinConfirmationArgs e)
        {
            LogMessage(Client.TwitchUsername,"<"+ e.Exception.Channel +"> " + e.Exception.Details,"Exception");
        }


        //Helper Methods
        public static async Task Run()
        {
           await Task.Delay(-1);
        }

        
        public static void ClientSetup(this TwitchClient client)
        {
            //Forwarding Events
            client.OnConnected += Client_OnConnected;
            client.OnDisconnected += (sender, e) => { _ = Client_OnDisconnected(sender, e); };
            //client.OnLog += Client_OnLog;
            //Error
            client.OnError += Program.Client_OnError;
            client.OnConnectionError += (sender, e) => { _ = Client_OnConnectionError(sender, e); };
            client.OnFailureToReceiveJoinConfirmation += Client_OnFailureToReceiveJoinConfirmation;
        }

        private static void Client_OnLog(object sender, OnLogArgs e)
        {
            LogMessage(Client.TwitchUsername, e.Data);
        }

        //Internal Loggers (For Client Issues)
        public static void LogMessage(string bot, string message, string type = "Info")
        {
            Console.WriteLine($"[{type}] [{DateTime.UtcNow.ToLongTimeString()}] <{bot}> - {message}");
        }

        private static bool CheckForInternetConnection()
        {
            try
            {
                var request = (HttpWebRequest)WebRequest.Create("http://www.gstatic.com/generate_204");
                request.KeepAlive = false;
                request.Timeout = 10000;
                using var response = (HttpWebResponse)request.GetResponse();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    
}
