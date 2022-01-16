using Beacon.API.Commands;
using Microsoft.Extensions.Logging;

namespace Beacon.DemoPlugin.Commands;

public class HelloCommand : ICommand
{
    public string Name => "hello there";

    public string Description => "Say hello to your server!";

    public string HelpText => @"¯\_(ツ)_/¯";

    public ValueTask<bool> ExecuteAsync(ICommandSender sender, string[] args, CancellationToken cToken = default)
    {
        sender.SendMessageAsync("Hi there!");
        return ValueTask.FromResult(true);
    }
}




