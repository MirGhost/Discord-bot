using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace DiscordBotChannelRegistry
{
    public class Registry
    {
        private readonly Dictionary<ulong, ulong> guildTextChannels = new();
        private readonly Dictionary<ulong, Discord.WebSocket.SocketVoiceChannel> guildVoiceChannels = new();
        public void SetVoiceChannel(ulong guildId, Discord.WebSocket.SocketVoiceChannel voiceChannel) =>
            guildVoiceChannels[guildId] = voiceChannel;
        public Discord.WebSocket.SocketVoiceChannel? GetVoiceChannel(ulong guildId) =>
            guildVoiceChannels.TryGetValue(guildId, out var channel) ? channel : null;

        public void SetTextChannel(ulong guildId, ulong channelId) =>
            guildTextChannels[guildId] = channelId;

        public ulong? GetTextChannel(ulong guildId) =>
            guildTextChannels.TryGetValue(guildId, out var channelId) ? channelId : null;
    }
}
