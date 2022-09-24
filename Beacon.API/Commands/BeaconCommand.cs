namespace Beacon.API.Commands;

public abstract class BeaconCommand
{
    private readonly Dictionary<string, BeaconCommand> _subcommands;

    public abstract string Keyword { get; }
    public abstract string Description { get; }
    public string[] SubCommandKeywords => _subcommands.Keys.ToArray();

    protected BeaconCommand()
    {
        _subcommands = new Dictionary<string, BeaconCommand>();
    }

    public ValueTask<bool> ExecuteAsync(ICommandSender sender, string[] args, CancellationToken cToken = default)
    {
        if (args.Length == 0) return HandleAsync(sender, args, cToken);
        if (_subcommands.ContainsKey(args[0])) return _subcommands[args[0]].ExecuteAsync(sender, args[1..], cToken);

        // The arguments passed are to be used by this command because it is not a sub-command.
        return HandleAsync(sender, args, cToken);
    }

    public BeaconCommand? GetSubCommand(string keyword)
    {
        return _subcommands.GetValueOrDefault(keyword);
    }

    /// <summary>
    ///     Add a sub command.
    /// </summary>
    /// <param name="keyword">The keyword for the subcommand.</param>
    /// <param name="command"></param>
    /// <returns>The sub-command that has been added.</returns>
    protected BeaconCommand AddSubCommand(BeaconCommand command)
    {
        if (_subcommands.ContainsKey(command.Keyword))
        {
            _subcommands[command.Keyword] = command;
            return this;
        }

        _subcommands.Add(command.Keyword, command);
        return command;
    }

    /// <summary>
    ///     Add a sub command.
    /// </summary>
    /// <param name="keyword">The keyword for the subcommand.</param>
    /// <param name="command"></param>
    /// A function to be called once the sub-commands gets called.
    /// <returns>The sub-command that has been added.</returns>
    protected BeaconCommand AddSubCommand(string keyword,
        Func<ICommandSender, string[], CancellationToken, ValueTask<bool>> action)
    {
        var inline = new InlineBeaconCommand(keyword, string.Empty, action);
        if (_subcommands.ContainsKey(keyword))
        {
            _subcommands[keyword] = inline;
            return this;
        }

        _subcommands.Add(keyword, inline);
        return inline;
    }

    protected virtual async ValueTask<bool> HandleAsync(ICommandSender sender, string[] args,
        CancellationToken cToken = default)
    {
        await sender.SendMessageAsync("Invalid command!");
        await sender.SendMessageAsync($"Available sub-command: {string.Join(", ", _subcommands.Keys)}");
        return false;
    }
}