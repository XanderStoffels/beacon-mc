using System.Text.RegularExpressions;

namespace Beacon.Server.Commands;

/// <summary>
/// A parser that can extract variable values from a string template and instance.
/// </summary>
/// <remarks>Credits to wcharczuk: https://gist.github.com/wcharczuk/2284226</remarks>
internal static class RouteParser
{
    private const string _routePattern = @"[{0}].+?[{1}]";
    private const string _tokenPattern = "(?<{0}>[^,]*)";
    private const char _variableStartChar = '{';
    private const char _variableEndChar = '}';

    public static HashSet<string> ParseVariableNames(string routeTemplate)
    {
        var variableList = new List<string>();
        var matchCollection = Regex.Matches
            (
                routeTemplate
                , string.Format(_routePattern, _variableStartChar, _variableEndChar)
                , RegexOptions.IgnoreCase
            );

        foreach (var match in matchCollection) 
            if (match.ToString() is string m)
                variableList.Add(RemoteVariableChars(m));
       
        return new(variableList);
    }

    public static Dictionary<string, string> Parse(string routeTemplate, string routeInstance)
    {
        var inputValues = new Dictionary<string, string>();
        var variables = ParseVariableNames(routeTemplate);

        foreach (var variable in variables)
            routeTemplate = routeTemplate.Replace(WrapWithVariableChars(variable), string.Format(_tokenPattern, variable));
        
        var regex = new Regex(routeTemplate, RegexOptions.IgnoreCase);
        var matchCollection = regex.Match(routeInstance);

        foreach (var variable in variables)
        {
            var value = matchCollection.Groups[variable].Value;
            inputValues.Add(variable, value);
        }

        return inputValues;
    }

    private static string RemoteVariableChars(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return input;

        var result = new string(input.ToArray());
        result = result.Replace(_variableStartChar.ToString(), string.Empty).Replace(_variableEndChar.ToString(), string.Empty);
        return result;
    }

    private static string WrapWithVariableChars(string input) 
        => string.Format("{0}{1}{2}", _variableStartChar, input, _variableEndChar);

}
