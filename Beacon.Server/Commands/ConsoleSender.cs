using Beacon.API;
using Beacon.API.Commands;
using Microsoft.Extensions.Logging;

namespace Beacon.Server.Commands;

internal class ConsoleSender : ICommandSender
{
    public IServer Server { get; }

    public ConsoleSender(IServer server)
    {
        this.Server = server;
    }

    public Task SendMessageAsync(string message)
    {
        Server.Logger.LogInformation("{m}", message);
        return Task.CompletedTask;
    }
}
