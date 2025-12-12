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
                            return new VectorDataAccess(
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
                            return new ChatCompletionGeneration(
                                settings.ChatCompletionApi.Protocol,
                                settings.ChatCompletionApi.Host,
                                settings.ChatCompletionApi.Port,
                                settings.ChatCompletionApi.Model);
                        });

                        _ = services.AddSingleton(provider =>
                        {
                            var settings = provider.GetRequiredService<AppSettings>();
                            return new ArchivalDataAccess(settings.DataDirectory);
                        });

                        _ = services.AddSingleton(provider =>
                        {
                            var settings = provider.GetRequiredService<AppSettings>();
                            var archivalDataAccess = provider.GetRequiredService<ArchivalDataAccess>();
                            var embeddingGeneration = provider.GetRequiredService<EmbeddingGeneration>();
                            var vectorStorage = provider.GetRequiredService<VectorDataAccess>();
                            
                            return new Embeddings(
                                archivalDataAccess,
                                embeddingGeneration,
                                vectorStorage,
                                settings.Contextualisation.ContextSectionChunkTitles);
                        });
                        
                        services.AddSingleton<Search>();
                        services.AddSingleton<ArchivalData>();

                        services.AddHostedService<ConsoleService>();
                    })
                    .Build();
    }
}