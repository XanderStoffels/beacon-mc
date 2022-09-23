using Beacon.API.Commands;

namespace Beacon.DemoPlugin.Commands;

internal class ClearCommand : BeaconCommand
{
    public override string Keyword => "clear";
    public override string Description => "Clears the screen.";

    protected override ValueTask<bool> HandleAsync(ICommandSender sender, string[] args,
        CancellationToken cToken = default)
    {
        Console.Clear();
        return ValueTask.FromResult(true);
    }
}