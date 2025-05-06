using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBotRegistrar
{
    public class SlashCommandRegistrar
    {
        private readonly DiscordSocketClient client;

        public SlashCommandRegistrar(DiscordSocketClient client)
        {
            this.client = client;
        }
        public async Task SlashCommandsRegister()
        {
            var globalCommand = new SlashCommandBuilder();
            globalCommand.WithName("what-is-your-duty");
            globalCommand.WithDescription("Send information about bot Duty");

            await client.CreateGlobalApplicationCommandAsync(globalCommand.Build());
        }

        public async Task SlashCommandHandler(SocketSlashCommand command)
        {
            await command.RespondAsync($"To serve {command.User.GlobalName}s Will.");
        }
    }
}
