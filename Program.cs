using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using VGI.Helpers.Osu;
using VGI.Helpers.Twitter;
using VGI.Services;

using IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(config =>
    {
#if DEBUG
        config.AddJsonFile("appSettings.development.json", false);
#else
        config.AddJsonFile("appSettings.json", false);
#endif
    })
    .ConfigureServices(services =>
    {
        services.AddSingleton<DiscordSocketClient>();       // Add the discord client to services
        services.AddSingleton<InteractionService>();        // Add the interaction service to services
        services.AddHostedService<InteractionHandlingService>();    // Add the slash command handler
        services.AddHostedService<DiscordStartupService>();         // Add the discord startup service
        services.AddSingleton<OsuMapper>();
        services.AddSingleton<TwitterClient>();
        services.AddSingleton<TwitterMapper>();
    })
    .Build();

await host.RunAsync();