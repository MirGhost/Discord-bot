using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Discord.Interactions;
using DiscordBotRegistrar;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Victoria;
using Microsoft.Extensions.Logging;
using DiscorBotLogService;
using DiscorBotMusicService;
using Victoria.Enums;

namespace DiscordBot
{
    internal class Program
    {
        private readonly DiscordSocketClient client;
        private readonly string token;
        private readonly IConfiguration config;
        private static IServiceProvider services;

        public Program()
        {
            this.client = new DiscordSocketClient(new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.GuildVoiceStates | GatewayIntents.GuildMessages
            });

            config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            token = config["Token"]; //my token for bot
        }

        static void Main(string[] commands) =>
            new Program().StartBotAsync().GetAwaiter().GetResult();

        public async Task StartBotAsync()
        {
            SetServices();
            var logService = services.GetRequiredService<LogService>();
            var interactionService = services.GetRequiredService<InteractionService>();
            var lavaNode = services.GetRequiredService<LavaNode<LavaPlayer<LavaTrack>, LavaTrack>>();

            this.client.Ready += ClientReady;
            
            client.InteractionCreated += async interaction =>
            {
                var ctx = new SocketInteractionContext(client, interaction); // вилучення контексту для подальшого використання 
                await interactionService.ExecuteCommandAsync(ctx, services); // очікування на виконання команд
            };

            lavaNode.OnTrackEnd += async arg =>
            {
                if (arg.Reason == TrackEndReason.Finished)
                {
                    var musicService = services.GetRequiredService<MusicService>();
                    await musicService.NextTrackAsync(arg);
                }
            };

            this.client.Log += logService.LogFuncAsync;
            
            await this.client.LoginAsync(TokenType.Bot, token);
            await this.client.StartAsync();
            await Task.Delay(-1);
        }

        private void SetServices()
        {
            var collection = new ServiceCollection()
                .AddSingleton(this.config)
                .AddSingleton(this.client)
                .AddSingleton(new CommandService())
                .AddSingleton<DiscordBotChannelRegistry.Registry>()
                .AddSingleton<InteractionService>(provider => // додання до колекції сервіса бота InteractionService для подальшої взаємодії
                {
                    var client = provider.GetRequiredService<DiscordSocketClient>(); //встановлення клієнта через DI
                    return new InteractionService(client);
                })
                .AddSingleton<DiscorBotLogService.LogService>()
                .AddSingleton<DiscorBotMusicService.MusicService>()
                .AddLogging(builder => builder.AddConsole())
                .AddSingleton(provider =>
                {
                    var discord = provider.GetRequiredService<DiscordSocketClient>();

                    var lavaConfig = new Configuration
                    {
                        SelfDeaf = false,
                        Hostname = config["Lavalink:Hostname"],
                        Port = ushort.Parse(config["Lavalink:Port"]),
                        Authorization = config["Lavalink:Authorization"],
                        IsSecure = bool.Parse(config["Lavalink:IsSsl"])
                    };

                    var logger = provider.GetRequiredService<ILogger<LavaNode<LavaPlayer<LavaTrack>, LavaTrack>>>();

                    return new LavaNode<LavaPlayer<LavaTrack>, LavaTrack>(discord, lavaConfig, logger);
                });

            services = collection.BuildServiceProvider();
        }

        private async Task ClientReady()
        {
            var interactionService = services.GetRequiredService<InteractionService>(); //отримання з колекції сервіса бота InteractionService
            await interactionService.AddModulesAsync(typeof(Program).Assembly, services); //пошук всіх класів для вилучиння і реєстрації команд

            await interactionService.RegisterCommandsGloballyAsync();

            // Дістаємо Lavalink Node з DI контейнера
            var lavaNode = services.GetRequiredService<LavaNode<LavaPlayer<LavaTrack>, LavaTrack>>();
            var logService = services.GetRequiredService<LogService>();

            // Перевірка підключення
            if (!lavaNode.IsConnected)
            {
                try
                {
                    await lavaNode.ConnectAsync();
                    await logService.LogFuncAsync(new LogMessage(LogSeverity.Info, "Services", "Connected to Lavalink server."));
                }
                catch (Exception ex)
                {
                    await logService.LogFuncAsync(new LogMessage(LogSeverity.Error, "Services", $"Failed to connect: {ex.Message}"));
                }
            }
            else
            {
                await logService.LogFuncAsync(new LogMessage(LogSeverity.Info, "Services", "Lavalink already connected."));
            }
        }
    }
}