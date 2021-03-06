using Beacon.API;
using Beacon.API.Commands;

namespace Beacon.Server.Commands.Builtins;

internal class ServerCommand : BeaconCommand
{
    public override string Keyword => "server";
    public override string Description => "Base command for server related stuff.";
    private readonly IServer _server;
    public ServerCommand(IServer server)
    {
        _server = server;
    }

    protected override void RegisterSubCommands()
    {
        AddSubCommand("version", HandleServerStop);
    }

    private async ValueTask<bool> HandleServerStop(ICommandSender sender, string[] args, CancellationToken cToken = default)
    {
        await sender.SendMessageAsync($"Beacon Server v{_server.Version.ToString()}");
        return true;
    }
}
