namespace Beacon.API.Commands;

internal class InlineBeaconCommand : BeaconCommand
{
    public override string Keyword { get; }
    public override string Description { get; }

    private readonly Func<ICommandSender, string[], CancellationToken, ValueTask<bool>> _action;

    public InlineBeaconCommand(string keyword, string description, Func<ICommandSender, string[], CancellationToken, ValueTask<bool>> action)
    {
        this.Keyword = keyword;
        this.Description = description;
        this._action = action;
    }

    protected override ValueTask<bool> HandleAsync(ICommandSender sender, string[] args, CancellationToken cToken = default)
    {
        return _action(sender, args, cToken);
    }

}