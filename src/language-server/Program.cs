using OmniSharp.Extensions.LanguageServer.Server;

namespace Fifth.LanguageServer;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var server = await OmniSharp.Extensions.LanguageServer.Server.LanguageServer.From(options =>
        {
            options.WithInput(Console.OpenStandardInput());
            options.WithOutput(Console.OpenStandardOutput());
        });

        await server.Initialize(default);
        await server.WaitForExit;
    }
}
