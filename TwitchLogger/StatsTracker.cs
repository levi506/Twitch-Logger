using Discord;
using Discord.Webhook;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using TwitchLib.Client;
using TwitchLib.Client.Enums;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;

namespace TwitchLogger
{
    public class StatsTracker
    {
        //Stats Vars
        private Dictionary<string, Chatter> Users { get; set; }
        private Dictionary<string, Raid> Raids { get; set; }
        private Dictionary<string, Sub> Subs { get; set; }
        private Dictionary<string, Gifter> GiftedSubs { get; set; }
        private Dictionary<string, Host> Hosts { get; set; }
        private Dictionary<string, Punishment> Punishments { get; set; }

        //Critical Var
        private DiscordWebhookClient StatsFeed { get; set; }
        private bool IsOpen { get; set; }

        public StatsTracker(TwitchClient client, string webhook)
        {
            StatsFeed = new DiscordWebhookClient(webhook);
            client.TrackChat(this);
            RegenDics();
            
        }
        private void RegenDics()
        {
            Users = new Dictionary<string, Chatter>();
            Raids = new Dictionary<string, Raid>();
            Subs = new Dictionary<string, Sub>();
            GiftedSubs = new Dictionary<string, Gifter>();
            Hosts = new Dictionary<string, Host>();
            Punishments = new Dictionary<string, Punishment>();
        }
        internal void Compile()
        {
            var ChattersSorted = Users.OrderByDescending( x => (x.Value.Times *(x.Value.Login.Equals("nightbot") || x.Value.Login.Equals("streamelements") || x.Value.Login.Equals("lita426t") || x.Value.Login.Equals("henukei") ? -1:1)));
            var CheersSorted = Users.OrderByDescending(x => (x.Value.Bits * (x.Value.Login.Equals("henukei") || x.Value.Login.Equals("lita426t") ? -1 : 1)));
            var RaidsSorted = Raids.OrderBy(x => x.Value.Time);
            var SubsSortedInverse = Subs.OrderBy(x => x.Value.Months);
            var SubsSortedLength = Subs.OrderByDescending(x => x.Value.Months);
            var Discipline = Punishments.OrderBy(x => x.Value.Time);
            var Gifts = GiftedSubs.OrderByDescending(x => (x.Value.Times * (x.Value.Login.Equals("henukei") || x.Value.Login.Equals("lita426t") ? -1 : 1)));
            var HostsByViewer = Hosts.OrderBy(x => x.Value.Viewers);
            int i;
            List<Embed> list;

            //Stream Stats
            var mainStats = new EmbedBuilder();
            mainStats.WithColor(Color.Teal);
            mainStats.WithAuthor("Chat Stats");
            var max = 0;
            max = Math.Min(10, Users.Count);
            string Chat = "", Cheer = "", HostList = "", NewSub = "", OldSub= "", Gifter = "";
            i = 0;
            foreach(var z in ChattersSorted)
            {

                Chat += $"[{z.Value.User}](https://twitch.tv/{z.Value.Login}): {z.Value.Times}\n";
                i++;
                if (i > max)
                {
                    break;
                }
            }
            i = 0;
            foreach (var z in CheersSorted)
            {
                if (z.Value.Bits < 1)
                    break;
                Cheer += $"[{z.Value.User}](https://twitch.tv/{z.Value.Login}): {z.Value.Bits}\n";
                i++;
                if (i > max)
                {
                    break;
                }
            }
            max = Math.Min(10, Subs.Count);
            i = 0;
            foreach (var z in SubsSortedLength)
            {
                if (z.Value.Months < 2)
                    break;
                OldSub += $"[{z.Value.User}](https://twitch.tv/{z.Value.Login}) resubbed for {z.Value.Months} months!\n";
                i++;
                if (i > max)
                {
                    break;
                }
            }
            i = 0;
            foreach (var z in SubsSortedInverse)
            {
                if (z.Value.Months > 1)
                    break;
                NewSub += $"[{z.Value.User}](https://twitch.tv/{z.Value.Login}) subbed for the first time!\n";
                i++;
                if (i > max)
                {
                    break;
                }
            }
            max = Math.Min(10, GiftedSubs.Count);
            i = 0;
            foreach (var z in Gifts)
            {
                Gifter += $"[{z.Value.User}](https://twitch.tv/{z.Value.Login}): {z.Value.Times}\n";
                i++;
                if (i > max)
                {
                    break;
                }
            }
            max = Math.Min(10, Hosts.Count);
            i = 0;
            foreach (var z in HostsByViewer)
            {
                if (z.Value.Auto || z.Value.Viewers < 2)
                    break;
                HostList += $"{z.Value.User}: {z.Value.Viewers}\n";
                i++;
                if (i > max)
                {
                    break;
                }
            }
            Chat.Trim();
            Cheer.Trim();
            HostList.Trim();
            NewSub.Trim();
            OldSub.Trim();
            Gifter.Trim();

            mainStats.WithCurrentTimestamp();
            mainStats.WithDescription($"Stats for the stream of {DateTime.UtcNow.ToLongDateString()}");

            if(!Chat.Equals(""))
                mainStats.AddField("Messages", Chat, true);
            if(!Cheer.Equals(""))
                mainStats.AddField("Cheer", Cheer, true);
            if(!Gifter.Equals(""))
                mainStats.AddField("Gifted Sub Points", Gifter, true);
            if(!NewSub.Equals(""))
                mainStats.AddField("New Subscribers", NewSub, true);
            if(!OldSub.Equals(""))
                mainStats.AddField("Longest Subs", OldSub, true);
            if(!HostList.Equals(""))
                mainStats.AddField("Hosts", HostList, true);

            list = new List<Embed>
                {
                    mainStats.Build()
                };
            StatsFeed.SendMessageAsync(embeds: list);


            //Raiders
            var RaidersEm = new EmbedBuilder();
            RaidersEm.WithColor(Color.DarkPurple);
            RaidersEm.WithAuthor("Raid Stats");
            var raiderDesc = "";
            foreach (var raider in RaidsSorted)
            {
                raiderDesc += $"[{raider.Value.User}](https://twitch.tv/{raider.Value.Login}) raided with {raider.Value.Viewers} at {raider.Value.Time.ToUniversalTime().ToShortTimeString()}\n";
            }
            raiderDesc.Trim();
            RaidersEm.WithCurrentTimestamp();
            RaidersEm.WithDescription(raiderDesc);
            //Using a List because Webhooks can technically send up to 5 embeds
            if (!raiderDesc.Equals(""))
            {
                list = new List<Embed>
                {
                    RaidersEm.Build()
                };
                StatsFeed.SendMessageAsync(embeds: list);
            }

            //Punishment Stats
            var PunEm = new EmbedBuilder();
            PunEm.WithColor(Color.DarkRed);
            PunEm.WithCurrentTimestamp();
            PunEm.WithAuthor("Issued Punishments");
            i = 0;
            foreach(var punn in Discipline)
            {
                i++;
                var desc = "";
                if (punn.Value.Type == PunishType.Ban) {
                    desc = $"Banned";
                } else
                {
                    desc = $"Timed Out for {punn.Value.Duration} seconds";
                }
                if (punn.Value.User == null)
                    continue;
                PunEm.AddField(punn.Value.User ?? "ERR: Username not logged", desc, true);
                    if(i> 23)
                {
                    PunEm.WithFooter("More Punishments in File");
                    break;
                }
            }
            if (PunEm.Fields.Count > 0)
            {
                list = new List<Embed>
                {
                    PunEm.Build()
                };
                StatsFeed.SendMessageAsync(embeds: list);
            }

            Crush();
        }

        private void Crush()
        {
            var UDb = new List<CrushedUser>();
            var RaidDb = new List<Raid>();
            var PunDb = new List<Punishment>();


            var Rkeys = Raids.Keys;
            foreach (var key in Rkeys)
            {
                Raids.TryGetValue(key, out var r);
                RaidDb.Add(r);
            }

            var Ukeys = Users.Keys;
            foreach (var key in Ukeys)
            {

                Users.TryGetValue(key, out var User);
                var RGot = Raids.TryGetValue(key, out var Raid);
                var SGot = Subs.TryGetValue(key, out var Sub);
                var GGot = GiftedSubs.TryGetValue(key, out var Gifter);

                UDb.Add(new CrushedUser
                {
                    Bits = User.Bits,
                    Messages = User.Times,
                    Raiders = RGot ? Raid.Viewers : 0,
                    uId = key,
                    SubMonths = (SGot ? Sub.Times : 0),
                    Months = SGot ? Sub.Months : 0,
                    T1subs = GGot ? Gifter.T1subs : 0,
                    T2subs = GGot ? Gifter.T2subs : 0,
                    T3subs = GGot ? Gifter.T3subs : 0,
                    GiftedSubs = GGot ? Gifter.Times : 0,
                    User = User.User,
                    Login = User.Login,
                    Tier = SGot ? Sub.Tier : SubscriptionPlan.NotSet,
                    Time = User.Time
                });
                Users.Remove(key);
                Raids.Remove(key);
                Subs.Remove(key);
                GiftedSubs.Remove(key);
            }

            Rkeys = Raids.Keys;
            if (Rkeys.Count > 0) {
                foreach (var key in Rkeys)
                {
                    Raids.TryGetValue(key, out var Raid);
                    var SGot = Subs.TryGetValue(key, out var Sub);
                    var GGot = GiftedSubs.TryGetValue(key, out var Gifter);

                    UDb.Add(new CrushedUser
                    {
                        Bits = 0,
                        Messages = 0,
                        Raiders = Raid.Viewers,
                        uId = key,
                        SubMonths = (SGot ? Sub.Times : 0),
                        Months = SGot ? Sub.Months : 0,
                        T1subs = GGot ? Gifter.T1subs : 0,
                        T2subs = GGot ? Gifter.T2subs : 0,
                        T3subs = GGot ? Gifter.T3subs : 0,
                        GiftedSubs = GGot ? Gifter.Times : 0,
                        User = Raid.User,
                        Login = Raid.Login,
                        Tier = SGot ? Sub.Tier : SubscriptionPlan.NotSet,
                        Time = Raid.Time
                    });
                    Raids.Remove(key);
                    Subs.Remove(key);
                    GiftedSubs.Remove(key);
                }
            }

            var Skeys = Subs.Keys;
            if (Skeys.Count > 0) {
                foreach (var key in Skeys)
                {
                    Subs.TryGetValue(key, out var Sub);
                    var GGot = GiftedSubs.TryGetValue(key, out var Gifter);

                    UDb.Add(new CrushedUser
                    {
                        Bits = 0,
                        Messages = 0,
                        Raiders = 0,
                        uId = key,
                        SubMonths = Sub.Times,
                        Months = Sub.Months,
                        T1subs = GGot ? Gifter.T1subs : 0,
                        T2subs = GGot ? Gifter.T2subs : 0,
                        T3subs = GGot ? Gifter.T3subs : 0,
                        GiftedSubs = GGot ? Gifter.Times : 0,
                        User = Sub.User,
                        Login = Sub.Login,
                        Tier = Sub.Tier,
                        Time = Sub.Time
                    });
                    Subs.Remove(key);
                    GiftedSubs.Remove(key);
                }
            }

            var Gkeys = GiftedSubs.Keys;
            if (Gkeys.Count > 0) {
                foreach (var key in Skeys)
                {
                    GiftedSubs.TryGetValue(key, out var Gifter);

                    UDb.Add(new CrushedUser
                    {
                        Bits = 0,
                        Messages = 0,
                        Raiders = 0,
                        uId = key,
                        SubMonths = 0,
                        Months = 0,
                        T1subs = Gifter.T1subs,
                        T2subs = Gifter.T2subs,
                        T3subs = Gifter.T3subs,
                        GiftedSubs = Gifter.Times,
                        User = Gifter.User,
                        Login = Gifter.Login,
                        Tier = SubscriptionPlan.NotSet,
                        Time = Gifter.Time
                    });
                    GiftedSubs.Remove(key);
                }
            }
            var PKeys = Punishments.Keys;
            foreach(var key in PKeys)
            {
                Punishments.TryGetValue(key, out var p);
                PunDb.Add(p);
            }
            var now = DateTime.UtcNow;

            Task.Delay(1000).GetAwaiter().GetResult();

            if (UDb.Count > 0)
            {
                using var Stream = new MemoryStream();
                using var StreamWriter = new StreamWriter(Stream);
                string jsonStringU = JsonSerializer.Serialize(UDb);
                jsonStringU = jsonStringU.PrettyJson();
                StreamWriter.Write(jsonStringU);
                StreamWriter.Flush();
                Stream.Position = 0;
                StatsFeed.SendFileAsync(Stream, $"{now.Year}_{now.Month}_{now.Day}_Users.json", $"Stats for {now.Year} - {now.Month} - {now.Day}").GetAwaiter().GetResult();
            }

            if (RaidDb.Count > 0)
            {
                using var StreamR = new MemoryStream();
                using var StreamWriterR = new StreamWriter(StreamR);
                string jsonStringR = JsonSerializer.Serialize(RaidDb);
                jsonStringR = jsonStringR.PrettyJson();
                StreamWriterR.WriteLine(jsonStringR);
                StreamWriterR.Flush();
                StreamR.Position = 0;
                StatsFeed.SendFileAsync(StreamR, $"{now.Year}_{now.Month}_{now.Day}_Raids.json", $"Raids for {now.Year} - {now.Month} - {now.Day}").GetAwaiter().GetResult();
            }

            if (PunDb.Count > 0)
            {
                using var StreamP = new MemoryStream();
                using var StreamWriterP = new StreamWriter(StreamP);
                string jsonStringP = JsonSerializer.Serialize(PunDb);
                jsonStringP = jsonStringP.PrettyJson();
                StreamWriterP.Write(jsonStringP);
                StreamWriterP.Flush();
                StreamP.Position = 0;
                StatsFeed.SendFileAsync(StreamP, $"{now.Year}_{now.Month}_{now.Day}_Punishments.json", $"Punishments for {now.Year} - {now.Month} - {now.Day}").GetAwaiter().GetResult();
            }

        }

        internal void Open()
        {
            if (!IsOpen)
            {
                IsOpen = true;
                RegenDics();
            }
        }
        public void Close()
        {
            IsOpen = false;
            Compile();
        }

        //Event Methods
        internal void Client_OnNewSubscriber(object sender, OnNewSubscriberArgs e)
        {
            AddSub(e.Subscriber);
        }
        internal void Client_OnRaidNotification(object sender, OnRaidNotificationArgs e)
        {
            AddRaider(e.RaidNotification);
        }
        internal void Client_OnCommunitySubscription(object sender, OnCommunitySubscriptionArgs e)
        {
            AddGifter(e.GiftedSubscription);
        }
        internal void Client_OnUserTimedout(object sender, OnUserTimedoutArgs e)
        {
            AddPunishment(e.UserTimeout);
        }
        internal void Client_OnUserBanned(object sender, OnUserBannedArgs e)
        {
            AddPunishment(e.UserBan);
        }
        internal void Client_OnGiftedSubscription(object sender, OnGiftedSubscriptionArgs e)
        {
            AddGifter(e.GiftedSubscription);
        }
        internal void Client_OnMessageReceived(object sender, OnMessageReceivedArgs e)
        {
            AddChatter(e.ChatMessage);
        }
        internal void Client_OnReSubscriber(object sender, OnReSubscriberArgs e)
        {
            AddSub(e.ReSubscriber);
        }
        internal void Client_OnBeingHosted(object sender, OnBeingHostedArgs e)
        {
            AddHost(e.BeingHostedNotification);
        }

        //Stat Congregators
        private void AddHost(BeingHostedNotification e)
        {
            var r = new Host()
            {
                User = e.HostedByChannel,
                Auto = e.IsAutoHosted,
                Viewers = e.Viewers
            };
            Hosts.Add(e.HostedByChannel, r);
        }
        private void AddRaider(RaidNotification e)
        {
            if (!Raids.ContainsKey(e.UserId))
            {

                var u = e.DisplayName;
                if (!e.DisplayName.ToLower().Equals(e.Login))
                {
                    u = u + " (" + e.Login + ")";
                }
                var r = new Raid()
                {
                    User = u,
                    uId = e.UserId,
                    Login = e.Login,
                    Time = DateTime.UtcNow,
                    Viewers = int.Parse(e.MsgParamViewerCount)
                };
                Raids.Add(e.UserId, r);
            }
        }
        private void AddGifter(GiftedSubscription e)
        {
            Gifter r;
            
            if (!GiftedSubs.TryGetValue(e.Login, out r))
            {
                var u = e.DisplayName;
                if (!e.DisplayName.ToLower().Equals(e.Login))
                {
                    u = u + " (" + e.Login + ")";
                }
                r = new Gifter()
                {
                    User = u,
                    uId = e.UserId,
                    Login = e.Login,
                    mId = e.Id,
                    Time = DateTime.UtcNow
                };
            }
            var Addative = Math.Max(int.Parse(e.MsgParamMonths), 1);
            switch (e.MsgParamSubPlan)
            {
                case SubscriptionPlan.Tier1:
                    r.T1subs += Addative;
                    r.Times += Addative;
                    break;
                case SubscriptionPlan.Tier2:
                    r.T2subs = Addative;
                    r.Times += Addative * 2;
                    break;
                case SubscriptionPlan.Tier3:
                    r.T3subs = Addative;
                    r.Times += Addative * 5;
                    break;
                default:
                    break;
            }
            GiftedSubs.Remove(e.UserId);
            GiftedSubs.Add(e.UserId, r);
        }
        private void AddGifter(CommunitySubscription e)
        {
            Gifter r;

            if (!GiftedSubs.TryGetValue(e.Login, out r))
            {
                var u = e.DisplayName;
                if (!e.DisplayName.ToLower().Equals(e.Login))
                {
                    u = u + " (" + e.Login + ")";
                }
                r = new Gifter()
                {
                    User = u,
                    uId = e.UserId,
                    Login = e.Login,
                    mId = e.Id,
                    Time = DateTime.UtcNow
                };
            }
            var Addative = (e.MsgParamMultiMonthGiftDuration != null)?int.Parse(e.MsgParamMultiMonthGiftDuration) * e.MsgParamMassGiftCount: e.MsgParamMassGiftCount;
            switch (e.MsgParamSubPlan)
            {
                case SubscriptionPlan.Tier1:
                    
                    r.T1subs += Addative;
                    r.Times += Addative;
                    break;
                case SubscriptionPlan.Tier2:
                    r.T2subs += Addative;
                    r.Times += Addative * 2;
                    break;
                case SubscriptionPlan.Tier3:
                    r.T3subs += Addative;
                    r.Times += Addative * 5;
                    break;
                default:
                    break;
            }
            GiftedSubs.Remove(e.UserId);
            GiftedSubs.Add(e.UserId, r);
        }
        private void AddSub(Subscriber e)
        {
            if (!Subs.ContainsKey(e.UserId))
            {
                var u = e.DisplayName;
                if (!e.DisplayName.ToLower().Equals(e.Login))
                {
                    u = u + " (" + e.Login + ")";
                }
                var streak = e.MsgParamStreakMonths == null ? 0 : int.Parse(e.MsgParamStreakMonths);
                var r = new Sub()
                {
                    User = u,
                    uId = e.UserId,
                    Login = e.Login,
                    mId = e.Id,
                    Time = DateTime.UtcNow,
                    Months = int.Parse(e.MsgParamCumulativeMonths),
                    Tier = e.SubscriptionPlan,
                    Times = streak
                };
                Subs.Add(e.UserId, r);
            }
        }
        private void AddSub(ReSubscriber e)
        {
            if (!Subs.ContainsKey(e.UserId))
            {
                var u = e.DisplayName;
                if (!e.DisplayName.ToLower().Equals(e.Login))
                {
                    u = u + " (" + e.Login + ")";
                }
                var streak = e.MsgParamStreakMonths == null ? 0 : int.Parse(e.MsgParamStreakMonths);
                var r = new Sub()
                {
                    User = u,
                    uId = e.UserId,
                    Login = e.Login,
                    mId = e.Id,
                    Time = DateTime.UtcNow,
                    Months = int.Parse(e.MsgParamCumulativeMonths),
                    Tier = e.SubscriptionPlan,
                    Times = streak
                };
                Subs.Add(e.UserId, r);
            }
        }
        private void AddChatter(ChatMessage e)
        {
            if (!Users.ContainsKey(e.UserId))
            {
                var u = e.DisplayName;
                if (!e.DisplayName.ToLower().Equals(e.Username))
                {
                    u = u + " (" + e.Username + ")";
                }
                var r = new Chatter()
                {

                    User = u,
                    Time = DateTime.UtcNow,
                    uId = e.UserId,
                    Login = e.Username,
                    mId = e.Id,
                    Times = 1,
                    Bits = e.Bits
                };
                Users.Add(e.UserId, r);
            }
            else
            {
                if (Users.TryGetValue(e.UserId, out var u))
                {
                    u.Times++;
                    u.Bits += e.Bits;
                    Users.Remove(e.UserId);
                    Users.Add(e.UserId, u);
                }
            }
        }
        private void AddPunishment(UserTimeout e)
        {
            var key = SnowflakeUtils.ToSnowflake(DateTimeOffset.Now).ToString();
            if (!Punishments.ContainsKey(key))
            {
                var r = new Punishment()
                {
                    User = e.Username,
                    Type = PunishType.Timeout,
                    Time = DateTime.UtcNow,
                    Duration = e.TimeoutDuration
                };
                Punishments.Add(key, r);
            }
        }
        private void AddPunishment(UserBan e)
        {
            var key = SnowflakeUtils.ToSnowflake(DateTimeOffset.Now).ToString();
            if (!Punishments.ContainsKey(key))
            {
                var r = new Punishment()
                {
                User = e.Username,
                uId = e.TargetUserId,
                Type = PunishType.Ban,
                Time = DateTime.UtcNow
                };
                Punishments.Add(key, r);
            }
        }
    }
}
