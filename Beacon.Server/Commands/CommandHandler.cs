using Beacon.API.Commands;
using Beacon.Server.CLI;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.RegularExpressions;

namespace Beacon.Server.Commands;

internal class CommandHandler : ICommandHandler
{
    private readonly ILogger _logger;
    private readonly Dictionary<string, BeaconCommand> _commandLookup;
    private readonly AutoCompleteNode _autoComplete;
    public CommandHandler(ILogger logger) : this(logger, new())
    {
    }

    public CommandHandler(ILogger logger, List<BeaconCommand> commands)
    {
        _logger = logger;
        _commandLookup = new(commands.Count);
        RegisterBaseCommands();
        RegisterCommands(commands);
        _autoComplete = BuildAutocompletionTree();
    }


    private void RegisterBaseCommands()
    {
        var builtins = typeof(CommandHandler).Assembly.GetTypes()
            .Where(t => !t.IsAbstract && !t.IsInterface && typeof(BeaconCommand).IsAssignableTo(t))
            .Select(t => Activator.CreateInstance(t))
            .Cast<BeaconCommand>()
            .ToList();

        RegisterCommands(builtins);
    }

    private AutoCompleteNode BuildAutocompletionTree()
    {
        var root = new AutoCompleteNode();
        foreach (var kv in _commandLookup)
            root.Options.Add(kv.Key, BuildAutocompletionTree(kv.Value));
        
        return root; 
    }

    private AutoCompleteNode BuildAutocompletionTree(BeaconCommand cmd)
    {
        AutoCompleteNode root;
        var subKeywords = cmd.SubCommandKeywords;
        root = subKeywords.Length == 0 ? new AutoCompleteLeafNode() : new AutoCompleteNode();

        foreach (var sub in subKeywords)
        {
            var innerCmd = cmd.GetSubCommand(sub);
            if (innerCmd != null)
                root.Options.Add(sub, BuildAutocompletionTree(innerCmd));
        }
        return root;
    }
    private void RegisterCommands(List<BeaconCommand> commands)
    {
        foreach (var cmd in commands)
        {
            if (cmd.Keyword.Length < 1 || !Regex.IsMatch(cmd.Keyword, "^[a-z]+$"))
            {
                _logger.LogWarning("The command name '{name}' is not valid and will not be loaded", cmd.Keyword);
                _logger.LogWarning("Command names should have no whitespace and contain at least 1 lower case letter", cmd.Keyword);
                continue;
            }
            if (_commandLookup.TryAdd(cmd.Keyword, cmd))
                continue;

            _logger.LogWarning("Command '{cmd}' was already registered. Skipping this one", cmd.Keyword);
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
