using Beacon.API;
using Beacon.API.Commands;

namespace Beacon.Server.Commands;

internal class ConsoleSender : ICommandSender
{
    public ConsoleSender(IServer server)
    {
        Server = server;
    }

    public IServer Server { get; }

    public Task SendMessageAsync(string message)
    {
        return Task.CompletedTask;
    }
}