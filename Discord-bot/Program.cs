using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace DiscordBot
{
    internal class Program
    {
        private readonly DiscordSocketClient client;
        private readonly string token;
        private IConfiguration config;
        
        public Program()
        {
            this.client = new DiscordSocketClient();

            config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            token = config["Token"]; //my token for bot
        }

        static void Main(string[] commands) =>
            new Program().StartBotAsync().GetAwaiter().GetResult();

        public async Task StartBotAsync()
        {
            this.client.Ready += Client_Ready;
            this.client.Log += LogFuncAsync;
            this.client.SlashCommandExecuted += SlashCommandHandler;
            
            await this.client.LoginAsync(TokenType.Bot, token);
            await this.client.StartAsync();
            await Task.Delay(-1);

            async Task LogFuncAsync(LogMessage message) =>
                Console.Write(message.ToString());
        }

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
    }
}