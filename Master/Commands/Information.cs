using Discord;
using Discord.Commands;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace RainBorg.Commands
{
    public partial class Commands : ModuleBase<SocketCommandContext>
    {
        [Command("info")]
        public async Task InfoAsync([Remainder]string Remainder = null)
        {
            if (RainBorg.Operators.Contains(Context.Message.Author.Id))
            {
                using (WebClient client = new WebClient())
                {
                    string dl = client.DownloadString(Constants.BalanceURL);
                    JObject j = JObject.Parse(dl);
                    RainBorg.tipBalance = (double)j["balance"] / 100;
                }

                double i = RainBorg.tipMin + RainBorg.tipFee - RainBorg.tipBalance;
                if (i < 0) i = 0;

                string m = "```Current tip balance: " + String.Format("{0:n}", RainBorg.tipBalance) + " TRTL\r\n" +
                    "Total tipped: " + Stats.GlobalStats.TotalAmount + " TRTL (" + Stats.GlobalStats.TotalTips + " tips)\r\n" +
                    "Average tip: " + String.Format("{0:n}", Stats.GlobalStats.TipAverage) + " TRTL\r\n" +
                    "Amount until next tip: " + String.Format("{0:n}", i) + " TRTL\r\n" +
                    "Tip minimum: " + String.Format("{0:n}", RainBorg.tipMin) + "\r\n" +
                    "Tip maximum: " + String.Format("{0:n}", RainBorg.tipMax) + "\r\n" +
                    "Minimum users: " + RainBorg.userMin + "\r\n" +
                    "Maximum users: " + RainBorg.userMax + "\r\n" +
                    "Minimum wait time: " + String.Format("{0:n0}", RainBorg.waitMin) + "ms (" + TimeSpan.FromMilliseconds(RainBorg.waitMin).ToString() + ")\r\n" +
                    "Maximum wait time: " + String.Format("{0:n0}", RainBorg.waitMax) + "ms (" + TimeSpan.FromMilliseconds(RainBorg.waitMax).ToString() + ")\r\n" +
                    "Current wait time: " + String.Format("{0:n0}", RainBorg.waitTime) + "ms (" + RainBorg.waitNext + ")\r\n" +
                    "Operators: " + RainBorg.Operators.Count + "\r\n" +
                    "Blacklisted: " + RainBorg.Blacklist.Count + "\r\n" +
                    "Greylisted: " + RainBorg.Greylist.Count + "\r\n" +
                    "Channel Count: " + RainBorg.UserPools.Keys.Count + "\r\n" +
                    "Address: " + RainBorg.botAddress + "\r\n" +
                    "Payment ID: " + RainBorg.botPaymentId + "\r\n" +
                    "```";
                await Context.Message.Author.SendMessageAsync(m);
            }
        }

        [Command("operators")]
        public async Task OperatorsAsync([Remainder]string Remainder = null)
        {
            if (RainBorg.Operators.Contains(Context.Message.Author.Id))
            {
                string m = "```Operators:\r\n";
                foreach (ulong i in RainBorg.Operators)
                {
                    try
                    {
                        m += Context.Client.GetUser(i).Username + "\r\n";
                    }
                    catch { }
                }
                m += "```";
                await Context.Message.Author.SendMessageAsync(m);
            }
        }

        [Command("blacklist")]
        public async Task BlacklistAsync([Remainder]string Remainder = null)
        {
            if (RainBorg.Operators.Contains(Context.Message.Author.Id))
            {
                string m = "```Blacklisted Users:\r\n";
                foreach (ulong i in RainBorg.Blacklist)
                    try
                    {
                        m += Context.Client.GetUser(i).Username + " (" + i + ")\r\n";
                    }
                    catch { }
                m += "\r\n\r\nGreylisted Users:\r\n";
                foreach (ulong i in RainBorg.Greylist)
                    try
                    {
                        m += Context.Client.GetUser(i).Username + " (" + i + ")\r\n";
                    }
                    catch { }
                m += "```";
                await Context.Message.Author.SendMessageAsync(m);
            }
        }

        [Command("userpools")]
        public async Task UserPoolsAsync([Remainder]string Remainder = null)
        {
            if (RainBorg.Operators.Contains(Context.Message.Author.Id))
            {
                string m = "```Current User Pools:\r\n";

                foreach (KeyValuePair<ulong, List<ulong>> entry in RainBorg.UserPools)
                {
                    try
                    {
                        m += "#" + Context.Client.GetChannel(entry.Key) + " (" + entry.Key + ") :\r\n";

                        List<ulong> v = entry.Value;
                        foreach (ulong s in v) m += Context.Client.GetUser(s).Username + " (" + s + ")\r\n";
                        m += "\r\n\r\n";
                    }
                    catch { }
                }

                m += "```";

                await Context.Message.Author.SendMessageAsync(m);
            }
        }

        [Command("channels")]
        public async Task ChannelsAsync([Remainder]string Remainder = null)
        {
            if (RainBorg.Operators.Contains(Context.Message.Author.Id))
            {
                string m = "```Tippable Channels:\r\n";
                foreach (KeyValuePair<ulong, List<ulong>> entry in RainBorg.UserPools)
                {
                    try
                    {
                        m += "#" + Context.Client.GetChannel(entry.Key) + ", weight of ";

                        var x = RainBorg.ChannelWeight.GroupBy(i => i);
                        foreach (var channel in x)
                            if (channel.Key == entry.Key) m += channel.Count();

                        m += "\r\n";
                    }
                    catch { }
                }
                m += "```";
                await Context.Message.Author.SendMessageAsync(m);
            }
        }
    }
}
