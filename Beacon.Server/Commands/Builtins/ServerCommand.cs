using Beacon.API.Commands;

namespace Beacon.Server.Commands.Builtins;

internal class ServerCommand : BeaconCommand
{
    public override string Keyword => "server";
    public override string Description => "Base command for server related stuff.";

    protected override void RegisterSubCommands()
    {
        AddSubCommand("stop", HandleServerStop);
    }

    private async ValueTask<bool> HandleServerStop(ICommandSender sender, string[] args, CancellationToken cToken = default)
    {
        await sender.SendMessageAsync("Great, this works!");
        return true;
    }
}
