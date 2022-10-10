using System;
using TwitchLib.Client;
using TwitchLib.Client.Enums;

namespace TwitchLogger
{
    public static class Utility
    {

        public static void TrackChat(this TwitchClient client, ChatTracker tracker)
        {

            //Messages
            client.OnBeingHosted += tracker.Client_OnBeingHosted;
            client.OnReSubscriber += tracker.Client_OnReSubscriber;
            client.OnNewSubscriber += tracker.Client_OnNewSubscriber;
            client.OnMessageReceived += tracker.Client_OnMessageReceived;
            client.OnRaidNotification += tracker.Client_OnRaidNotification;
            client.OnGiftedSubscription += tracker.Client_OnGiftedSubscription;
            client.OnCommunitySubscription += tracker.Client_OnCommunitySubscription;

            //Mod Action
            client.OnUserBanned += tracker.Client_OnUserBanned;
            client.OnUserTimedout += tracker.Client_OnUserTimedout;
            
        }
        public static void TrackChat(this TwitchClient client, StatsTracker tracker)
        {

            //Messages
            client.OnBeingHosted += tracker.Client_OnBeingHosted;
            client.OnReSubscriber += tracker.Client_OnReSubscriber;
            client.OnNewSubscriber += tracker.Client_OnNewSubscriber;
            client.OnMessageReceived += tracker.Client_OnMessageReceived;
            client.OnRaidNotification += tracker.Client_OnRaidNotification;
            client.OnGiftedSubscription += tracker.Client_OnGiftedSubscription;
            client.OnCommunitySubscription += tracker.Client_OnCommunitySubscription;

            //Mod Action
            client.OnUserBanned += tracker.Client_OnUserBanned;
            client.OnUserTimedout += tracker.Client_OnUserTimedout;

        }
        public static string PrettyJson(this string val)
        {
            return val.Replace(",", ",\n").Replace("{", "{\n").Replace("}", "\n}");
        }
    }
    public struct Raid
    {
        public string User { get; set; }
        public string uId { get; set; }
        public string mId { get; set; }
        public DateTime Time { get; set; }
        public int Viewers { get; set; }
        public string Login { get; set; }
    }
    public struct Sub
    {
        public string User { get; set; }
        public string uId { get; set; }
        public string mId { get; set; }
        public SubscriptionPlan Tier { get; set; }
        public int Months { get; set; }
        public DateTime Time { get; set; }
        public int Times { get; set; }
        public string Login { get; set; }
    }
    public struct Gifter
    {
        public string User { get; set; }
        public string uId { get; set; }
        public string mId { get; set; }
        public int Times { get; set; }
        public int T1subs { get; set; }
        public int T2subs { get; set; }
        public int T3subs { get; set; }
        public string Login { get; set; }
        public DateTime Time { get; set; }
    }
    public struct Chatter
    {
        public string mId { get; set; }
        public string User { get; set; }
        public string uId { get; set; }
        public DateTime Time { get; set; }
        public int Times { get; set; }
        public int Bits { get; set; }
        public string Login { get; set; }
    }
    public struct Host
    {
        public string User { get; set; }
        public bool Auto { get; set; }
        public int Viewers { get; set; }
    }
    public struct Punishment
    {
        public string User { get; set; }
        public string uId { get; set; }
        public string mId { get; set; }
        public PunishType Type { get; set; }
        public DateTime Time { get; set; }
        public int Duration { get; set; }
    }

    public struct CrushedUser
    {
        public string User { get; set; }
        public string uId { get; set; }
        public int Messages { get; set; }
        public int Bits { get; set; }
        public SubscriptionPlan Tier { get; set; }
        public int Months { get; set; }
        public DateTime Time { get; set; }
        public int SubMonths { get; set; }
        public string Login { get; set; }
        public int GiftedSubs { get; set; }
        public int T1subs { get; set; }
        public int T2subs { get; set; }
        public int T3subs { get; set; }
        public int Raiders { get; set; }
    }
    public enum PunishType
    {
        Timeout,
        Ban
    }

    
}
