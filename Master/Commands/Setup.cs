using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RainBorg.Commands
{
    public partial class Commands : ModuleBase<SocketCommandContext>
    {
        [Command("addchannel")]
        public async Task AddChannelAsync(ulong Id, int Weight, [Remainder]string Remainder = null)
        {
            if (RainBorg.Operators.Contains(Context.Message.Author.Id))
            {
                try
                {
                    if (!RainBorg.UserPools.ContainsKey(Id) && Weight > 0)
                    {
                        RainBorg.UserPools.Add(Id, new List<ulong>());
                        for (int i = 0; i < Weight; i++) RainBorg.ChannelWeight.Add(Id);
                        await Config.Save();
                        if (RainBorg.entranceMessage != "")
                            await (Context.Client.GetChannel(Id) as SocketTextChannel).SendMessageAsync(RainBorg.entranceMessage);
                        try
                        {
                            // Add reaction to message
                            IEmote emote = Context.Guild.Emotes.First(e => e.Name == RainBorg.successReact);
                            await Context.Message.AddReactionAsync(emote);
                        }
                        catch { }
                    }
                }
                catch { }
            }
        }

        [Command("addchannel")]
        public async Task AddChannelMentionAsync(SocketChannel Channel, int Weight, [Remainder]string Remainder = null)
        {
            if (RainBorg.Operators.Contains(Context.Message.Author.Id))
            {
                try
                {
                    if (!RainBorg.UserPools.ContainsKey(Channel.Id) && Weight > 0)
                    {
                        RainBorg.UserPools.Add(Channel.Id, new List<ulong>());
                        for (int i = 0; i < Weight; i++) RainBorg.ChannelWeight.Add(Channel.Id);
                        await Config.Save();
                        if (RainBorg.entranceMessage != "")
                            await (Context.Client.GetChannel(Channel.Id) as SocketTextChannel).SendMessageAsync(RainBorg.entranceMessage);
                        try
                        {
                            // Add reaction to message
                            IEmote emote = Context.Guild.Emotes.First(e => e.Name == RainBorg.successReact);
                            await Context.Message.AddReactionAsync(emote);
                        }
                        catch { }
                    }
                }
                catch { }
            }
        }

        [Command("removechannel")]
        public async Task RemoveChannelAsync(params ulong[] Ids)
        {
            if (RainBorg.Operators.Contains(Context.Message.Author.Id))
            {
                foreach (ulong Id in Ids)
                    try
                    {
                        if (RainBorg.UserPools.ContainsKey(Id))
                        {
                            RainBorg.UserPools.Remove(Id);
                            RainBorg.ChannelWeight.RemoveAll(ChannelId => ChannelId == Id);
                            await Config.Save();
                        }
                    }
                    catch { }
                try
                {
                    // Add reaction to message
                    IEmote emote = Context.Guild.Emotes.First(e => e.Name == RainBorg.successReact);
                    await Context.Message.AddReactionAsync(emote);
                }
                catch { }
            }
        }

        [Command("removechannel")]
        public async Task RemoveChannelMentionAsync(params SocketChannel[] Channels)
        {
            if (RainBorg.Operators.Contains(Context.Message.Author.Id))
            {
                foreach (SocketChannel Channel in Channels)
                    try
                    {
                        if (RainBorg.UserPools.ContainsKey(Channel.Id))
                        {
                            RainBorg.UserPools.Remove(Channel.Id);
                            RainBorg.ChannelWeight.RemoveAll(ChannelId => ChannelId == Channel.Id);
                            await Config.Save();
                        }
                    }
                    catch { }
                try
                {
                    // Add reaction to message
                    IEmote emote = Context.Guild.Emotes.First(e => e.Name == RainBorg.successReact);
                    await Context.Message.AddReactionAsync(emote);
                }
                catch { }
            }
        }

        [Command("addstatuschannel")]
        public async Task AddStatusChannelMentionAsync(SocketChannel Channel, [Remainder]string Remainder = null)
        {
            if (RainBorg.Operators.Contains(Context.Message.Author.Id))
            {
                try
                {
                    if (!RainBorg.StatusChannel.Contains(Channel.Id))
                    {
                        RainBorg.StatusChannel.Add(Channel.Id);
                        await Config.Save();
                        try
                        {
                            // Add reaction to message
                            IEmote emote = Context.Guild.Emotes.First(e => e.Name == RainBorg.successReact);
                            await Context.Message.AddReactionAsync(emote);
                        }
                        catch { }
                    }
                }
                catch { }
            }
        }

        [Command("addstatuschannel")]
        public async Task AddStatusChannelAsync(ulong Id, [Remainder]string Remainder = null)
        {
            if (RainBorg.Operators.Contains(Context.Message.Author.Id))
            {
                try
                {
                    if (!RainBorg.StatusChannel.Contains(Id))
                    {
                        RainBorg.StatusChannel.Add(Id);
                        await Config.Save();
                        try
                        {
                            // Add reaction to message
                            IEmote emote = Context.Guild.Emotes.First(e => e.Name == RainBorg.successReact);
                            await Context.Message.AddReactionAsync(emote);
                        }
                        catch { }
                    }
                }
                catch { }
            }
        }

        [Command("removestatuschannel")]
        public async Task RemoveStatusChannelAsync(ulong Id, [Remainder]string Remainder = null)
        {
            if (RainBorg.Operators.Contains(Context.Message.Author.Id))
            {
                try
                {
                    if (RainBorg.StatusChannel.Contains(Id))
                    {
                        RainBorg.StatusChannel.Remove(Id);
                        await Config.Save();
                        try
                        {
                            // Add reaction to message
                            IEmote emote = Context.Guild.Emotes.First(e => e.Name == RainBorg.successReact);
                            await Context.Message.AddReactionAsync(emote);
                        }
                        catch { }
                    }
                }
                catch { }
            }
        }

        [Command("removestatuschannel")]
        public async Task RemoveStatusChannelMentionAsync(SocketChannel Channel, [Remainder]string Remainder = null)
        {
            if (RainBorg.Operators.Contains(Context.Message.Author.Id))
            {
                try
                {
                    if (RainBorg.StatusChannel.Contains(Channel.Id))
                    {
                        RainBorg.StatusChannel.Remove(Channel.Id);
                        await Config.Save();
                        try
                        {
                            // Add reaction to message
                            IEmote emote = Context.Guild.Emotes.First(e => e.Name == RainBorg.successReact);
                            await Context.Message.AddReactionAsync(emote);
                        }
                        catch { }
                    }
                }
                catch { }
            }
        }

        [Command("addoperator")]
        public async Task AddOperatorAsync([Remainder]string Remainder = null)
        {
            if (RainBorg.Operators.Contains(Context.Message.Author.Id))
            {
                foreach (SocketUser user in Context.Message.MentionedUsers)
                    if (!RainBorg.Operators.Contains(user.Id))
                    {
                        RainBorg.Operators.Add(user.Id);
                    }
                await Config.Save();
                try
                {
                    // Add reaction to message
                    IEmote emote = Context.Guild.Emotes.First(e => e.Name == RainBorg.successReact);
                    await Context.Message.AddReactionAsync(emote);
                }
                catch { }
            }
        }

        [Command("addoperator")]
        public async Task AddOperatorAsync(params ulong[] users)
        {
            if (RainBorg.Operators.Contains(Context.Message.Author.Id))
            {
                foreach (ulong user in users)
                    try
                    {
                        if (!RainBorg.Operators.Contains(Context.Client.GetUser(user).Id))
                        {
                            RainBorg.Operators.Add(Context.Client.GetUser(user).Id);
                        }
                    }
                    catch { }
                await Config.Save();
                try
                {
                    // Add reaction to message
                    IEmote emote = Context.Guild.Emotes.First(e => e.Name == RainBorg.successReact);
                    await Context.Message.AddReactionAsync(emote);
                }
                catch { }
            }
        }

        [Command("removeoperator")]
        public async Task RemoveOperatorAsync([Remainder]string Remainder = null)
        {
            if (RainBorg.Operators.Contains(Context.Message.Author.Id))
            {
                foreach (SocketUser user in Context.Message.MentionedUsers)
                    if (RainBorg.Operators.Contains(user.Id))
                    {
                        RainBorg.Operators.Remove(user.Id);
                    }
                await Config.Save();
                try
                {
                    // Add reaction to message
                    IEmote emote = Context.Guild.Emotes.First(e => e.Name == RainBorg.successReact);
                    await Context.Message.AddReactionAsync(emote);
                }
                catch { }
            }
        }

        [Command("removeoperator")]
        public async Task RemoveOperatorAsync(params ulong[] users)
        {
            if (RainBorg.Operators.Contains(Context.Message.Author.Id))
            {
                foreach (ulong user in users)
                    try
                    {
                        if (RainBorg.Operators.Contains(Context.Client.GetUser(user).Id))
                        {
                            RainBorg.Operators.Remove(Context.Client.GetUser(user).Id);
                        }
                    }
                    catch { }
                await Config.Save();
                try
                {
                    // Add reaction to message
                    IEmote emote = Context.Guild.Emotes.First(e => e.Name == RainBorg.successReact);
                    await Context.Message.AddReactionAsync(emote);
                }
                catch { }
            }
        }
    }
}
