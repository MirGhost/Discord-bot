using Discord;
using DiscordBot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Victoria.WebSocket.EventArgs;

namespace DiscorBotLogService
{
    public class LogService
    {
        async public Task LogFuncAsync(LogMessage message) =>
                Console.Write(message.ToString());
    }
}
