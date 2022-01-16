using Beacon.API.Commands;
using Beacon.Server.CLI;
using Microsoft.Extensions.Logging;
using System.Text;

namespace Beacon.Server.Commands;

internal class CommandHandler : ICommandHandler
{
    private readonly ILogger _logger;
    private readonly Dictionary<string, ICommand> _commandLookup;
    private readonly AutoCompleteNode _autoComplete;
    public CommandHandler(ILogger logger, List<ICommand> commands)
    {
        _logger = logger;
        _commandLookup = new(commands.Count);
        RegisterCommands(commands);
        _autoComplete = BuildAutocompletionTree();
    }

    private AutoCompleteNode BuildAutocompletionTree()
    {
        var root = new AutoCompleteNode();
        foreach (var kv in _commandLookup)
        {
            root.Options.Add(kv.Key, new AutoCompleteLeafNode(true));
        }
        return root; 
    }
    private void RegisterCommands(List<ICommand> commands)
    {
        foreach (var cmd in commands)
        {
            if (_commandLookup.TryAdd(cmd.Name, cmd))
                continue;

            _logger.LogWarning("Command '{cmd}' was already registered. Skipping this one", cmd.Name);
        }
    }

    public ValueTask<bool> HandleAsync(ICommandSender sender, string command, CancellationToken cToken = default)
    {
        if (string.IsNullOrWhiteSpace(command))
            return ValueTask.FromResult(false);

        var parts = new Queue<string>(command.Split(' '));
        var bob = new StringBuilder();

        for (var i = 0; i < parts.Count; i++)
        {
            if (i != 0) 
                bob.Append(' ');
            bob.Append(parts.Dequeue());

            if (!_commandLookup.TryGetValue(bob.ToString(), out var cmd))
                continue;

            return cmd.ExecuteAsync(sender, parts.ToArray(), cToken);
        }

        return ValueTask.FromResult(false);

    }
    public AutoCompleteNode GetAutoCompleteTree() => _autoComplete;
}
