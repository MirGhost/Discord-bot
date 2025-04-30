using System.Net.NetworkInformation;
using System.Reactive.Subjects;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DiscordBot
{
    internal class Program
    {
        private readonly DiscordSocketClient client;
        private static CommandService commands;
        private const string token = "MTM2NzEwNjUxMjU4MTAzODExMQ.GVUQgP.M0LKbxow76jYQllxkVIZUvNLFxXZeZ2Y2K0nGY"; //my token for bot

        public Program()
        {
            var config = new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent
            };
            this.client = new DiscordSocketClient(config);
            this.client.MessageReceived += MessageHandler;
        }

        public async Task StartBotAsync()
        {

            this.client.Log += LogFuncAsync;
            await this.client.LoginAsync(TokenType.Bot, token);
            await this.client.StartAsync();
            await Task.Delay(-1);

            async Task LogFuncAsync(LogMessage message) =>
                Console.Write(message.ToString());
        }

        private async Task MessageHandler(SocketMessage message)
        {
            if (message.Author.IsBot) return;

            if (message.Content.Equals("коли закінчується служба?"))
            {
                await ReplyAsync(message, "служба вічна і закінчується зі смертю");
            }
        }

        private async Task ReplyAsync(SocketMessage message, string response) =>
            await message.Channel.SendMessageAsync(response);

        //private async Task CommandsHandler(SocketUserMessage command)
        //{
        //    if (command.Author.IsBot || command == null) return;

        //    int position = 0;
        //    bool messageIsCommand = command.HasCharPrefix('/', ref position);
        //    if (messageIsCommand)
        //    {
        //        await 
        //    }
        //}

        static void Main(string[] commands) =>
            new Program().StartBotAsync().GetAwaiter().GetResult();
    }
}