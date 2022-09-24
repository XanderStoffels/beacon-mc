namespace Beacon.API.Commands;

internal class InlineBeaconCommand : BeaconCommand
{
    private readonly Func<ICommandSender, string[], CancellationToken, ValueTask<bool>> _action;

    public override string Keyword { get; }
    public override string Description { get; }

    public InlineBeaconCommand(string keyword, string description,
        Func<ICommandSender, string[], CancellationToken, ValueTask<bool>> action)
    {
        Keyword = keyword;
        Description = description;
        _action = action;
    }

    protected override ValueTask<bool> HandleAsync(ICommandSender sender, string[] args,
        CancellationToken cToken = default)
    {
        return _action(sender, args, cToken);
    }
}