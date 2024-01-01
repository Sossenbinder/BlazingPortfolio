using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Telegram.Bot;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services => {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
        
        services.AddSingleton(sp => new CosmosClient(sp.GetRequiredService<IConfiguration>()["CosmosConnectionString"]));
        services.AddSingleton(sp => new TelegramBotClient(sp.GetRequiredService<IConfiguration>()["TelegramToken"]));
    })
    .Build();

host.Run();