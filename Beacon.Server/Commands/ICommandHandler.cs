using Beacon.API.Commands;
using Beacon.Server.CLI;

namespace Beacon.Server.Commands;

internal interface ICommandHandler
{
    public ValueTask<bool> HandleAsync(ICommandSender sender, string command, CancellationToken cToken = default);
    public AutoCompleteNode GetAutoCompleteTree();
}
