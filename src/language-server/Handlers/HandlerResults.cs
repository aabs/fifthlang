using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace Fifth.LanguageServer.Handlers;

public static class HandlerResults
{
    public static Task<LocationOrLocationLinks?> EmptyDefinitionAsync() =>
        Task.FromResult<LocationOrLocationLinks?>(new LocationOrLocationLinks());
}
