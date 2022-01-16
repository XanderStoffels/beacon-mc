namespace Beacon.Server.CLI;

internal class AutoCompleteNode
{
    public Dictionary<string, AutoCompleteNode> Options { get; protected set; }

    public AutoCompleteNode()
    {
        Options = new();
    }

    /// <summary>
    /// Check if the given input has any autocomplete options.
    /// </summary>
    /// <param name="input">The string you want to autocomplete.</param>
    /// <param name="hint">The rest of the given input string which can be used for autocompletion.</param>
    /// <returns>Boolean indicating if the string can be autocompleted.</returns>
    public virtual bool Hint(string input, out string? hint)
    {
        var parts = input.Split(' ');
        return Hint(parts, 0, out hint);
    }

    protected virtual bool Hint(string[] parts, int depth, out string? hint)
    {
        hint = null;
        if (depth >= parts.Length) return false;

        var input = parts[depth];
        if (input.Length == 0)
        {
            hint = Options.Keys.FirstOrDefault();
            return hint != null;
        }

        if (Options.ContainsKey(input))
            return Options[input].Hint(parts, depth + 1, out hint);

        if (depth != parts.Length - 1) return false;

        hint = Options.Keys.FirstOrDefault(k => k.Length > input.Length && k[..input.Length] == input);
        if (hint is null) return false;

        hint = hint.Substring(input.Length);
        return false;
    }
}
