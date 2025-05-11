using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Victoria.WebSocket.EventArgs;

namespace DiscorBotLogService
{
    internal class LogService
    {
        async public Task LogFuncAsync(LogMessage message) =>
                Console.Write(message.ToString());

        async public Task LavaNodeOnConnected() =>
            Console.WriteLine(" " + DateTime.Now + " connected ||||||||||||||||||||");
    }
}
