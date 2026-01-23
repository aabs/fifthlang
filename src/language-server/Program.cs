using OmniSharp.Extensions.LanguageServer.Server;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Fifth.LanguageServer.Parsing;
using Fifth.LanguageServer.Logging;

namespace Fifth.LanguageServer;

public static class Program
{
    public static async Task Main(string[] args)
    {
        using var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
        };

        var server = await OmniSharp.Extensions.LanguageServer.Server.LanguageServer.From(options =>
        {
            options.WithInput(Console.OpenStandardInput());
            options.WithOutput(Console.OpenStandardOutput());
            options.WithHandler<Handlers.DocumentSyncHandler>();
            options.WithHandler<Handlers.HoverHandler>();
            options.WithHandler<Handlers.CompletionHandler>();
            options.WithHandler<Handlers.DefinitionHandler>();
            options.WithServices(services =>
            {
                services.AddLogging(builder =>
                {
                    builder.ClearProviders();
                    builder.AddProvider(new StderrLoggerProvider());
                    builder.SetMinimumLevel(LogLevel.Information);
                });
                services.AddSingleton<ParsingService>();
                services.AddSingleton<DocumentService>();
                services.AddSingleton<DocumentStore>();
                services.AddSingleton<SymbolService>();
            });
        });

        await server.Initialize(cts.Token);
        await server.WaitForExit;
    }
}
