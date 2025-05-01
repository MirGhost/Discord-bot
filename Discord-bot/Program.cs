using System;
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
        private const string token = "MTM2NzEwNjUxMjU4MTAzODExMQ.G_nSH-.V609OVxhLlbMPPSEkQlaaQ5KQ7hjIz-vGn3pSw"; //my token for bot

        public Program()
        {
            var config = new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent
            };
            this.client = new DiscordSocketClient(config);
            this.client.MessageReceived += MessageHandler;
            this.client.Ready += Client_Ready;
            this.client.SlashCommandExecuted += SlashCommandHandler;
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

        public async Task Client_Ready()
        {
            var globalCommand = new SlashCommandBuilder();
            globalCommand.WithName("what-is-your-duty");
            globalCommand.WithDescription("Send information about bot Duty");

            await client.CreateGlobalApplicationCommandAsync(globalCommand.Build());

        }

        private async Task SlashCommandHandler(SocketSlashCommand command)
        {
            await command.RespondAsync($"To serve {command.User.GlobalName}s Will.");
        }

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