namespace Beacon.Server.CLI;

internal class AutoCompleteLeafNode : AutoCompleteNode
{
    public bool AcceptsArgs { get; }
    public AutoCompleteLeafNode(bool acceptArgs = false, params string[] options) 
    {
        Options = options.ToDictionary(o => o, o => new AutoCompleteNode());
        AcceptsArgs = acceptArgs;
    }

    public override bool Hint(string input, out string? hint)
    {
        hint = null;

        if (string.IsNullOrEmpty(input) && !Options.Any()) return true;
        if (Options.ContainsKey(input)) return true;

        hint = Options.Keys
            .FirstOrDefault(k => k.Length > input.Length && k[..input.Length] == input)
            ?[input.Length..];

        return AcceptsArgs;
    }

    protected override bool Hint(string[] parts, int depth, out string? hint)
    {
        hint = null;
        if (depth == parts.Length && !Options.Any()) return true;
        if (depth != parts.Length - 1) return AcceptsArgs;
        var input = parts[depth];
        if (input.Length == 0)
        {
            hint = Options.Keys.FirstOrDefault();
            return false;
        }
        return Hint(input, out hint);
    }


}
