using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RainBorg.Commands
{
    public partial class Commands : ModuleBase<SocketCommandContext>
    {
        [Command("pause")]
        public async Task PauseAsync(params SocketUser[] users)
        {
            if (RainBorg.Operators.Contains(Context.Message.Author.Id))
            {
                RainBorg.Paused = true;
                await Context.Message.Author.SendMessageAsync("Bot paused.");
                try
                {
                    SocketGuildChannel guild = Context.Message.Channel as SocketGuildChannel;
                    IEmote emote = Context.Guild.Emotes.First(e => e.Name == RainBorg.successReact);
                    await Context.Message.AddReactionAsync(emote);
                }
                catch { }
            }
        }

        [Command("resume")]
        public async Task StartAsync(params SocketUser[] users)
        {
            if (RainBorg.Operators.Contains(Context.Message.Author.Id))
            {
                RainBorg.Paused = false;
                await Context.Message.Author.SendMessageAsync("Bot resumed.");
                try
                {
                    SocketGuildChannel guild = Context.Message.Channel as SocketGuildChannel;
                    IEmote emote = Context.Guild.Emotes.First(e => e.Name == RainBorg.successReact);
                    await Context.Message.AddReactionAsync(emote);
                }
                catch { }
            }
        }

        [Command("adduser")]
        public async Task AddUserAsync(params SocketUser[] users)
        {
            if (RainBorg.Operators.Contains(Context.Message.Author.Id))
            {
                foreach (SocketUser user in users)
                    try
                    {
                        if (user != null && RainBorg.UserPools.ContainsKey(Context.Channel.Id) && !RainBorg.UserPools[Context.Channel.Id].Contains(user.Id))
                        {
                            RainBorg.UserPools[Context.Channel.Id].Add(user.Id);
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

        [Command("removeuser")]
        public async Task RemoveUserAsync(params SocketUser[] users)
        {
            if (RainBorg.Operators.Contains(Context.Message.Author.Id))
            {
                foreach (SocketUser user in users)
                    try
                    {
                        if (user != null && RainBorg.UserPools.ContainsKey(Context.Channel.Id) && RainBorg.UserPools[Context.Channel.Id].Contains(user.Id))
                        {
                            RainBorg.UserPools[Context.Channel.Id].Remove(user.Id);
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

        [Command("reset")]
        public async Task ResetAsync([Remainder]string Remainder = null)
        {
            if (RainBorg.Operators.Contains(Context.Message.Author.Id))
            {
                foreach (KeyValuePair<ulong, List<ulong>> Entry in RainBorg.UserPools)
                    Entry.Value.Clear();
                RainBorg.Greylist.Clear();
                await Context.Message.Author.SendMessageAsync("User pools and greylist cleared.");
                try
                {
                    SocketGuildChannel guild = Context.Message.Channel as SocketGuildChannel;
                    IEmote emote = Context.Guild.Emotes.First(e => e.Name == RainBorg.successReact);
                    await Context.Message.AddReactionAsync(emote);
                }
                catch { }
            }
        }

        [Command("dotip")]
        public async Task DoTipAsync([Remainder]string Remainder = null)
        {
            if (RainBorg.Operators.Contains(Context.Message.Author.Id))
            {
                RainBorg.Waiting = RainBorg.waitTime;
                try
                {
                    SocketGuildChannel guild = Context.Message.Channel as SocketGuildChannel;
                    IEmote emote = Context.Guild.Emotes.First(e => e.Name == RainBorg.successReact);
                    await Context.Message.AddReactionAsync(emote);
                }
                catch { }
            }
        }

        [Command("exit")]
        public async Task ExitAsync([Remainder]string Remainder = null)
        {
            if (RainBorg.Operators.Contains(Context.Message.Author.Id))
            {
                await Config.Save();
                RainBorg.ConsoleEventCallback(2);
                Environment.Exit(0);
            }
        }
    }
}
