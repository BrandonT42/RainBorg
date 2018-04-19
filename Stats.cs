using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace RainBorg
{
    // Utility class to hold tip information
    class Tip
    {
        public DateTime Date { get; }
        public double Amount { get; }
        public ulong Channel { get; }
        public Tip (DateTime date, ulong channel, double amount)
        {
            Date = date;
            Amount = amount;
            Channel = channel;
        }
    }

    // Utility class to hold channel information
    class StatTracker
    {
        public int TotalTips;
        public double TotalAmount;
        public double TipAverage;
        public List<Tip> Tips;
        public StatTracker()
        {
            TotalTips = 0;
            TotalAmount = 0;
            TipAverage = 0;
            Tips = new List<Tip>();
        }
    }

    class Stats
    {
        // Global stats
        public static StatTracker GlobalStats = new StatTracker();

        // Channel stats
        public static Dictionary<ulong, StatTracker> ChannelStats = new Dictionary<ulong, StatTracker>();

        // User stats
        public static Dictionary<ulong, StatTracker> UserStats = new Dictionary<ulong, StatTracker>();

        // Update user tips
        public static Task Tip(DateTime Date, ulong Channel, ulong Id, double Amount)
        {
            // Update user stats
            if (!UserStats.ContainsKey(Id))
                UserStats.Add(Id, new StatTracker());
            UserStats[Id].Tips.Add(new Tip(Date, Channel, Amount));
            if (UserStats[Id].TipAverage == 0)
                UserStats[Id].TipAverage = Amount;
            else UserStats[Id].TipAverage = (UserStats[Id].TipAverage + Amount) / 2;
            UserStats[Id].TotalTips++;
            UserStats[Id].TotalAmount += Amount;

            // Update channel stats
            if (!ChannelStats.ContainsKey(Channel))
                ChannelStats.Add(Channel, new StatTracker());
            ChannelStats[Channel].Tips.Add(new Tip(Date, Channel, Amount));
            if (ChannelStats[Channel].TipAverage == 0)
                ChannelStats[Channel].TipAverage = Amount;
            else ChannelStats[Channel].TipAverage = (ChannelStats[Channel].TipAverage + Amount) / 2;
            ChannelStats[Channel].TotalTips++;
            ChannelStats[Channel].TotalAmount += Amount;

            // Update global stats
            if (GlobalStats.TipAverage == 0)
                GlobalStats.TipAverage = Amount;
            else GlobalStats.TipAverage = (GlobalStats.TipAverage + Amount) / 2;
            GlobalStats.TotalTips++;
            GlobalStats.TotalAmount += Amount;

            // Completed
            return Task.CompletedTask;
        }

        // Loads stats from stat sheet
        public static async Task Load()
        {
            // Create stat sheet if it doesn't exist
            if (!File.Exists(Constants.StatSheet))
            {
                File.Create(Constants.StatSheet).Close();
                await Update();
            }
            else
            {
                // Load values
                JObject StatSheet = JObject.Parse(File.ReadAllText(Constants.StatSheet));
                GlobalStats = StatSheet["globalStats"].ToObject<StatTracker>();
                ChannelStats = StatSheet["channelStats"].ToObject<Dictionary<ulong, StatTracker>>();
                UserStats = StatSheet["userStats"].ToObject<Dictionary<ulong, StatTracker>>();
            }
        }

        // Flush updates to stat sheet
        public static Task Update()
        {
            // Store values
            JObject StatSheet = new JObject
            {
                ["globalStats"] = JToken.FromObject(GlobalStats),
                ["channelStats"] = JToken.FromObject(ChannelStats),
                ["userStats"] = JToken.FromObject(UserStats)
            };

            // Flush to file
            File.WriteAllText(Constants.StatSheet, StatSheet.ToString());

            // Completed
            return Task.CompletedTask;
        }
    }
}
