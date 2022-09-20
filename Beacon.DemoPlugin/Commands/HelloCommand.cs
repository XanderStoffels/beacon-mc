using Beacon.API.Commands;

namespace Beacon.DemoPlugin.Commands;

public class HelloCommand : BeaconCommand
{
    public override string Keyword => "hello";
    public override string Description => "Say hello to your server!";

    protected override ValueTask<bool> HandleAsync(ICommandSender sender, string[] args, CancellationToken cToken = default)
    {
        if (args.Length == 0)
        {
            sender.SendMessageAsync("Hi there!");
        }
        else
        {
            sender.SendMessageAsync($"Hi there, {string.Join(" ", args)}");
        }
        return ValueTask.FromResult(true);
    }
}




