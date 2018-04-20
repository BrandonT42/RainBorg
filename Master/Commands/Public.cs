﻿using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace RainBorg.Commands
{
    public partial class Commands : ModuleBase<SocketCommandContext>
    {
        [Command("balance")]
        public async Task BalanceAsync([Remainder]string Remainder = null)
        {
            // Get balance
            using (WebClient client = new WebClient())
            {
                string dl = client.DownloadString(Constants.BalanceURL);
                JObject j = JObject.Parse(dl);
                RainBorg.tipBalance = (double)j["balance"] / 100;
            }

            double i = RainBorg.tipMin - RainBorg.tipBalance;
            if (i < 0) i = 0;

            string m = "Current tip balance: " + String.Format("{0:n}", RainBorg.tipBalance);
            await ReplyAsync(m);
        }

        [Command("donate")]
        public async Task DonateAsync([Remainder]string Remainder = null)
        {
            string m = "Want to donate to keep the rain a-pouring? What a great turtle you are! :)\r\n\r\n";
            m += "To donate, simply send some TRTL to the following address, REMEMBER to use the provided payment ID, or else your funds will NOT reach the tip pool.\r\n";
            m += "```Address:\r\n" + RainBorg.botAddress + "\r\n";
            m += "Payment ID (INCLUDE THIS):\r\n" + RainBorg.botPaymentId + "```";
            await Context.Message.Author.SendMessageAsync(m);
        }

        [Command("help")]
        public async Task HelpAsync([Remainder]string Remainder = null)
        {
            string m = "```List of Commands:\r\n";
            m += "$balance - Check the bot's tip balance\r\n";
            m += "$donate - Learn how you can donate to the tip pool\r\n";
            m += "$optout - Opt out of receiving tips from the bot\r\n";
            m += "$optin - Opt back into receiving tips from the bot```";
            if (RainBorg.Operators.Contains(Context.Message.Author.Id))
            {
                m += "Op-only message:\r\nOperator-only command documentation can be found at:\r\n";
                m += "https://github.com/BrandonT42/RainBorg/wiki/Operator-Commands\r\n";
            }
            m += "Need more help? Check the wiki link below to learn how to be a part of the rain:\r\n" + RainBorg.wikiURL;
            await Context.Message.Author.SendMessageAsync(m);
        }

        [Command("optout")]
        public async Task OutOutAsync([Remainder]string Remainder = null)
        {
            if (!RainBorg.OptedOut.Contains(Context.Message.Author.Id))
            {
                RainBorg.OptedOut.Add(Context.Message.Author.Id);
                await RainBorg.RemoveUserAsync(Context.Message.Author, 0);
                await Config.Save();
                try
                {
                    
                    IEmote emote = Context.Guild.Emotes.First(e => e.Name == RainBorg.successReact);
                    await Context.Message.AddReactionAsync(emote);
                }
                catch { }
                await Context.Message.Author.SendMessageAsync("You have opted out from receiving future tips.");
            }
            else await Context.Message.Author.SendMessageAsync("You have already opted out, use $optin to opt back into receiving tips.");
        }

        [Command("optin")]
        public async Task OptInAsync([Remainder]string Remainder = null)
        {
            if (RainBorg.OptedOut.Contains(Context.Message.Author.Id))
            {
                RainBorg.OptedOut.Remove(Context.Message.Author.Id);
                await Config.Save();
                try
                {
                    
                    IEmote emote = Context.Guild.Emotes.First(e => e.Name == RainBorg.successReact);
                    await Context.Message.AddReactionAsync(emote);
                }
                catch { }
                await Context.Message.Author.SendMessageAsync("You have opted back in, and will receive tips once again.");
            }
            else await Context.Message.Author.SendMessageAsync("You have not opted out, you are already able to receive tips.");
        }
    }
}