using OmniSharp.Extensions.LanguageServer.Server;
using Microsoft.Extensions.DependencyInjection;
using Fifth.LanguageServer.Parsing;

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
            options.WithServices(services =>
            {
                services.AddSingleton<ParsingService>();
                services.AddSingleton<DocumentService>();
                services.AddSingleton<DocumentStore>();
            });
        });

        await server.Initialize(cts.Token);
        await server.WaitForExit;
    }
}
