using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Linq;
using System.Threading.Tasks;

namespace RainBorg.Commands
{
    public partial class Commands : ModuleBase<SocketCommandContext>
    {
        [Command("exile")]
        public async Task ExileAsync(params SocketUser[] users)
        {
            if (RainBorg.Operators.Contains(Context.Message.Author.Id))
            {
                foreach (SocketUser user in users)
                    if (!RainBorg.Blacklist.Contains(user.Id))
                    {
                        RainBorg.Blacklist.Add(user.Id);
                        await RainBorg.RemoveUserAsync(user, 0);
                    }
                await Config.Save();
                await ReplyAsync("Blacklisted users, they will receive no tips.");
                try
                {
                    SocketGuildChannel guild = Context.Message.Channel as SocketGuildChannel;
                    IEmote emote = Context.Guild.Emotes.First(e => e.Name == RainBorg.successReact);
                    await Context.Message.AddReactionAsync(emote);
                }
                catch { }
            }
        }

        [Command("exile")]
        public async Task ExileAsync(params ulong[] users)
        {
            if (RainBorg.Operators.Contains(Context.Message.Author.Id))
            {
                foreach (ulong user in users)
                    try
                    {
                        if (!RainBorg.Blacklist.Contains(Context.Client.GetUser(user).Id))
                        {
                            RainBorg.Blacklist.Add(Context.Client.GetUser(user).Id);
                            await RainBorg.RemoveUserAsync(Context.Client.GetUser(user), 0);
                        }
                    }
                    catch { }
                await Config.Save();
                await ReplyAsync("Blacklisted users, they will receive no tips.");
                try
                {
                    SocketGuildChannel guild = Context.Message.Channel as SocketGuildChannel;
                    IEmote emote = Context.Guild.Emotes.First(e => e.Name == RainBorg.successReact);
                    await Context.Message.AddReactionAsync(emote);
                }
                catch { }
            }
        }

        [Command("unexile")]
        public async Task UnExileAsync(params SocketUser[] users)
        {
            if (RainBorg.Operators.Contains(Context.Message.Author.Id))
            {
                foreach (SocketUser user in users)
                    if (RainBorg.Blacklist.Contains(user.Id))
                    {
                        RainBorg.Blacklist.Remove(user.Id);
                    }
                await Config.Save();
                await ReplyAsync("Removed users from blacklist, they may receive tips again.");
                try
                {
                    SocketGuildChannel guild = Context.Message.Channel as SocketGuildChannel;
                    IEmote emote = Context.Guild.Emotes.First(e => e.Name == RainBorg.successReact);
                    await Context.Message.AddReactionAsync(emote);
                }
                catch { }
            }
        }

        [Command("unexile")]
        public async Task UnExileAsync(params ulong[] users)
        {
            if (RainBorg.Operators.Contains(Context.Message.Author.Id))
            {
                foreach (ulong user in users)
                    try
                    {
                        if (RainBorg.Blacklist.Contains(Context.Client.GetUser(user).Id))
                        {
                            RainBorg.Blacklist.Remove(Context.Client.GetUser(user).Id);
                        }
                    }
                    catch { }
                await Config.Save();
                await ReplyAsync("Removed users from blacklist, they may receive tips again.");
                try
                {
                    SocketGuildChannel guild = Context.Message.Channel as SocketGuildChannel;
                    IEmote emote = Context.Guild.Emotes.First(e => e.Name == RainBorg.successReact);
                    await Context.Message.AddReactionAsync(emote);
                }
                catch { }
            }
        }

        [Command("warn")]
        public async Task WarnAsync(params SocketUser[] users)
        {
            if (RainBorg.Operators.Contains(Context.Message.Author.Id))
            {
                foreach (SocketUser user in users)
                    try
                    {
                        if (user != null && !RainBorg.Greylist.Contains(user.Id))
                        {
                            EmbedBuilder builder = new EmbedBuilder();
                            builder.WithColor(Color.Green);
                            builder.WithTitle("SPAM WARNING");
                            builder.Description = RainBorg.spamWarning;

                            RainBorg.Greylist.Add(user.Id);
                            await RainBorg.RemoveUserAsync(user, 0);
                            await user.SendMessageAsync("", false, builder);
                        }
                    }
                    catch { }
                try
                {
                    SocketGuildChannel guild = Context.Message.Channel as SocketGuildChannel;
                    IEmote emote = Context.Guild.Emotes.First(e => e.Name == RainBorg.successReact);
                    await Context.Message.AddReactionAsync(emote);
                }
                catch { }
            }
        }
    }
}
