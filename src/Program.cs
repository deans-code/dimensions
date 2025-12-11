using Dimensions.Business;
using Dimensions.Config;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Dimensions.Infrastructure;
using Dimensions.Interface;

namespace Dimensions;

public sealed class Program
{
    private static async Task Main(string[] args)
    {
        IHost host = BuildHost(args);

        await host.RunAsync();
    }

    private static IHost BuildHost(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
                    .ConfigureServices((context, services) =>
                    {
                        var appSettings = new AppSettings();
                        context.Configuration.Bind(appSettings);
                        services.AddSingleton(appSettings);

                        _ = services.AddSingleton(provider =>
                        {
                            var settings = provider.GetRequiredService<AppSettings>();
                            return new VectorStorage(
                                settings.VectorDatabaseApi.Host,
                                settings.VectorDatabaseApi.Port,
                                settings.VectorDatabaseApi.CollectionName);
                        });

                        _ = services.AddSingleton(provider =>
                        {
                            var settings = provider.GetRequiredService<AppSettings>();
                            return new EmbeddingGeneration(
                                settings.EmbeddingApi.Protocol,
                                settings.EmbeddingApi.Host,
                                settings.EmbeddingApi.Port,
                                settings.EmbeddingApi.Model);
                        });

                        _ = services.AddSingleton(provider =>
                        {
                            var settings = provider.GetRequiredService<AppSettings>();
                            return new RawDataLoading(settings.DataDirectory);
                        });

                        services.AddSingleton<Embeddings>();

                        services.AddHostedService<ConsoleService>();
                    })
                    .Build();
    }
}