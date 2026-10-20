using System.Globalization;
using System.Text.RegularExpressions;

namespace Tally.Variables;

/// <summary>
/// Substitutes <c>{{ name }}</c> tokens in strings using a <see cref="VariableScope"/>.
/// </summary>
public static partial class TemplateEngine
{
    [GeneratedRegex(@"\{\{\s*([^}\s]+)\s*\}\}")]
    private static partial Regex TokenPattern();

    public static string Render(string template, VariableScope scope)
        => TokenPattern().Replace(template, match => scope.Get(match.Groups[1].Value));

    /// <summary>
    /// Converts a variable value (which may originate from typed YAML) into the
    /// string form used for substitution and environment fallback layers.
    /// </summary>
    public static string Stringify(object? value) => value switch
    {
        null => "",
        string s => s,
        bool b => b ? "true" : "false",
        IFormattable f => f.ToString(null, CultureInfo.InvariantCulture),
        _ => value.ToString() ?? "",
    };

    public static IReadOnlyDictionary<string, string> ToStringLayer(IReadOnlyDictionary<string, object?> source)
        => source.ToDictionary(kvp => kvp.Key, kvp => Stringify(kvp.Value));
}
