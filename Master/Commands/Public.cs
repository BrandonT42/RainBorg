using Discord;
using Discord.Commands;
using Newtonsoft.Json.Linq;
using System;
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
            m += "$donate - Learn how you can donate to the tip pool";

            if (RainBorg.Operators.Contains(Context.Message.Author.Id))
            {
                m += "\r\n\r\nOperator Only Commands:\r\n";
                m += "$info - Display current bot values\r\n";
                m += "$addoperator - Adds users as new bot operators (Usage: $addoperator @user1 @user2)\r\n";
                m += "$removeoperator - Removes users as bot operators (Usage: $removeoperator @user1 @user2)\r\n";
                m += "$operators - List all bot operators\r\n";
                m += "$exile - Blacklist users from receiving tips (Usage: $exile @user1 @user2)\r\n";
                m += "$unexile - Remove users from blacklist (Usage: $unexile @user1 @user2)\r\n";
                m += "$blacklist - Lists users that have been blacklisted\r\n";
                m += "$waitmin - Sets the minimum wait time between tipping cycles (Usage: $waitmin milliseconds)\r\n";
                m += "$waitmax - Sets the maximum wait time between tipping cycles (Usage: $waitmax milliseconds)\r\n";
                m += "$usermin - Set the minimum number of users the bot needs to see in the tip pool before tipping (Usage: $usermin amount)\r\n";
                m += "$usermax - Set the maximum number of users the bot will tip (Usage: $usermax amount)\r\n";
                m += "$tipmin - Set the minimum tip (Usage: $tipmin amount)\r\n";
                m += "$tipmax - Set the maximum tip (Usage: $tipmax amount)\r\n";
                m += "$addchannel - Add a tippable channel (Usage: $addchannel #channel channelweight)\r\n";
                m += "$removechannel - Removes a tippable channel (Usage: $removechannel #channel)\r\n";
                m += "$addstatuschannel - Adds a status channel (Usage: $addstatuschannel #channel)\r\n";
                m += "$removestatuschannel - Removes a status channel (Usage: $removestatuschannel #channel)\r\n";
                m += "$userpools - Views all userpools in the current tip cycle\r\n";
                m += "$channels - Lists all tippable channels\r\n";
                m += "$warn - Issues a spam warning and greylists users from the current tip pool (Usage: $warn @user1 @user2)\r\n";
                m += "$adduser - Manually add users to the tip pool in the current channel (Usage: $adduser @user1 @user2)\r\n";
                m += "$dotip - Manually trigger the current tip cycle\r\n";
                m += "$reset - Resets user pools and greylist\r\n";
                m += "$pause - Pauses the bot\r\n";
                m += "$resume - Resumes the bot if paused\r\n";
                m += "$exit - Exit the bot```";
            }
            else m += "```";

            if (!RainBorg.Operators.Contains(Context.Message.Author.Id))
                m += "Need more help? Check the wiki link below to learn how to be a part of the rain:\r\n" + RainBorg.wikiURL;


            await Context.Message.Author.SendMessageAsync(m);
        }
    }
}