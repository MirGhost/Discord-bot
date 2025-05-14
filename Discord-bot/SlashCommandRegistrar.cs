using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBotRegistrar
{
    public class SlashCommandRegistrar : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("what-is-your-duty", "Send information about bot Duty")]
        public async Task DutyCommand()
        {
            await RespondAsync($"To serve {Context.User.GlobalName}s Will.");
        }
    }
}
