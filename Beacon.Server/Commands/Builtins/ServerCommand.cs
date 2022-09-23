using Beacon.API;
using Beacon.API.Commands;

namespace Beacon.Server.Commands.Builtins;

internal class ServerCommand : BeaconCommand
{
    private readonly IServer _server;

    public ServerCommand(IServer server)
    {
        _server = server;
        AddSubCommand("version", HandleServerStop);
    }

    public override string Keyword => "server";
    public override string Description => "Base command for server related stuff.";

    private async ValueTask<bool> HandleServerStop(ICommandSender sender, string[] args,
        CancellationToken cToken = default)
    {
        await sender.SendMessageAsync($"Beacon Server v{BeaconServer.Version} for Minecraft {BeaconServer.McVersion}");
        return true;
    }
}