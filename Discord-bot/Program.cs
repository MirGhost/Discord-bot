using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBotRegistrar;
using Microsoft.Extensions.Configuration;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace DiscordBot
{
    internal class Program
    {
        private readonly DiscordSocketClient client;
        private readonly string token;
        private readonly IConfiguration config;
        private SlashCommandRegistrar registrar;

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
            registrar = new SlashCommandRegistrar(client);

            this.client.Ready += ClientReady;
            this.client.SlashCommandExecuted += registrar.SlashCommandHandler;
            this.client.Log += LogFuncAsync;

            await this.client.LoginAsync(TokenType.Bot, token);
            await this.client.StartAsync();
            await Task.Delay(-1);

            async Task LogFuncAsync(LogMessage message) =>
                Console.Write(message.ToString());
        }
        private async Task ClientReady()
        {
            await registrar.SlashCommandsRegister();
        }
    }
}