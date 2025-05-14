using Discord;
using Discord.Interactions;
using Victoria;
using Victoria.Rest.Search;
using Victoria.Enums;
using System;
using Discord.WebSocket;
using DiscorBotLogService;
using Discord.Rest;
using System.Threading.Channels;
using DiscordBotChannelRegistry;

namespace DiscorBotMusicService
{
    public class MusicService : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly LavaNode<LavaPlayer<LavaTrack>, LavaTrack> lavaNode;
        private readonly DiscordSocketClient client;
        private readonly Registry registry;
        private readonly LogService logService;

        public MusicService(LavaNode<LavaPlayer<LavaTrack>, LavaTrack> lavaNode, DiscordSocketClient client, Registry registry, LogService logService)
        {
            this.lavaNode = lavaNode;
            this.client = client;
            this.registry = registry;
            this.logService = logService;
        }

        public async Task NextTrackAsync(Victoria.WebSocket.EventArgs.TrackEndEventArg arg)
        {
            LavaPlayer<LavaTrack> player = await lavaNode.TryGetPlayerAsync(arg.GuildId);
            if (player == null)
                return;

            var channelId = registry.GetTextChannel(arg.GuildId);
            if (channelId == null)
                return;

            var channel = client.GetChannel(channelId.Value) as ITextChannel;
            if (channel == null)
                return;

            var voiceChannel = registry.GetVoiceChannel(arg.GuildId);
            //if (voiceChannel == null)
            //    return;

            if (player.GetQueue().Count > 0)
            {
                if (player.GetQueue().TryDequeue(out var nextTrack))
                {
                    await player.PlayAsync(lavaNode, nextTrack);
                    await channel.SendMessageAsync($"Now playing: **{nextTrack.Title}**");
                }
            }
            else
            {
                await channel.SendMessageAsync("Queue is empty, the tracks are over.");
                await lavaNode.LeaveAsync(voiceChannel);
            }
        }

        [SlashCommand("play", "Play music in voice channel")]
        public async Task PlayAsync([Summary("query", "YouTube link or search term")] string searchQuery)
        {
            await DeferAsync(); // longer timeout 

            var voiceState = Context.User as IVoiceState;
            if (voiceState?.VoiceChannel == null)
            {
                await FollowupAsync("You must be connected to a voice channel!");
                return;
            }

            SearchResponse searchResponse = null;
            try
            {
                searchResponse = await lavaNode.LoadTrackAsync(searchQuery);
            }
            catch (Exception)
            {
                await FollowupAsync($"I wasn't able to find anything for `{searchQuery}`.");
                return;
            }

            try
            {
                var voiceChannel = Context.Guild.CurrentUser.VoiceChannel;

                LavaPlayer<LavaTrack> player = await lavaNode.TryGetPlayerAsync(Context.Guild.Id);
                if (player == null || voiceChannel == null)
                {
                    player = await lavaNode.JoinAsync(voiceState.VoiceChannel);
                    await FollowupAsync($"Joined {voiceState.VoiceChannel.Name}!");
                }

                var track = searchResponse.Tracks.FirstOrDefault();

                if (player?.Track == null)
                {
                    await player.PlayAsync(lavaNode, track);
                    await FollowupAsync($"Now playing: \n Title: **{track.Title}** \n Duration: ({track.Duration}) \n {searchQuery}");
                }
                else
                {
                    player.GetQueue().Enqueue(track);
                    await FollowupAsync($"Added to queue: \n Title: **{track.Title}** \n Duration: ({track.Duration}) \n {searchQuery}");
                }

                voiceChannel = Context.Guild.CurrentUser.VoiceChannel;
                registry.SetTextChannel(Context.Guild.Id, Context.Channel.Id);
                registry.SetVoiceChannel(Context.Guild.Id, voiceChannel);
            }
            catch (Exception ex)
            {
                await logService.LogFuncAsync(new(LogSeverity.Error, "SlashCommand play ", ex.Message));
            }
        }

        [SlashCommand("pause", "I can pause the track")]
        public async Task PauseAsync()
        {
            await DeferAsync(); 

            var voiceChannel = Context.Guild.CurrentUser.VoiceChannel;
            var player = await lavaNode.TryGetPlayerAsync(Context.Guild.Id);
            if (voiceChannel == null)
            {
                await FollowupAsync("I cannot pause when I'm not playing anything!");
                return;
            }

            try
            {
                await player.PauseAsync(lavaNode);
                await FollowupAsync($"Paused: **{player.Track.Title}**");
            }
            catch (Exception ex)
            {
                await logService.LogFuncAsync(new(LogSeverity.Error, "SlashCommand pause ", ex.Message));
            }
        }

        [SlashCommand("resume", "I can resume playing the track")]
        public async Task ResumeAsync()
        {
            await DeferAsync(); 

            var voiceChannel = Context.Guild.CurrentUser.VoiceChannel;
            var player = await lavaNode.TryGetPlayerAsync(Context.Guild.Id);
            if (voiceChannel == null)
            {
                await FollowupAsync("I cannot resume when I'm not playing anything!");
                return;
            }

            try
            {
                await player.ResumeAsync(lavaNode, player.Track);
                await FollowupAsync($"Resumed: **{player.Track.Title}**");
            }
            catch (Exception ex)
            {
                await logService.LogFuncAsync(new(LogSeverity.Error, "SlashCommand resume ", ex.Message)); ;
            }
        }

        [SlashCommand("stop", "leave")]
        public async Task StopAsync()
        {
            await DeferAsync();

            var voiceChannel = Context.Guild.CurrentUser.VoiceChannel;
            if (voiceChannel == null)
            {
                await FollowupAsync("Woah, can't stop won't stop.");
                return;
            }
            try
            {
                await lavaNode.LeaveAsync(voiceChannel);
                await FollowupAsync("No longer playing anything.");
            }
            catch (Exception ex)
            {
                await logService.LogFuncAsync(new (LogSeverity.Error, "SlashCommand stop ", ex.Message));
            }
        }

        [SlashCommand("skip", "I can skip tracks")]
        public async Task SkipAsync()
        {
            await DeferAsync();

            var voiceChannel = Context.Guild.CurrentUser.VoiceChannel;
            var player = await lavaNode.TryGetPlayerAsync(Context.Guild.Id);
            if (voiceChannel == null)
            {
                await FollowupAsync("Woaaah there, I can't skip when nothing is playing.");
                return;
            }

            if (player.GetQueue().Count == 0)
            {
                await FollowupAsync("There's nothing to skip!");
                return;
            }

            try
            {
                if (player.GetQueue().TryDequeue(out var nextTrack))
                {
                    await player.ResumeAsync(lavaNode, nextTrack);
                    await FollowupAsync($"Skipped to the next track: **{nextTrack.Title}**");
                }
            }
            catch (Exception ex)
            {
                await logService.LogFuncAsync(new(LogSeverity.Error, "SlashCommand skip ", ex.Message));
            }
        }
    }
}
