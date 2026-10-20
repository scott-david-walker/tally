namespace Tally.Variables;

/// <summary>
/// Resolves variable names against a chain of layers. Layers are consulted in the
/// order they are supplied (first = highest priority). If no layer contains the
/// name, the process environment is used as the final fallback.
///
/// A name prefixed with <c>env.</c> (e.g. <c>env.API_TOKEN</c>) bypasses the
/// layer chain entirely and reads directly from the environment.
/// </summary>
public sealed class VariableScope
{
    private const string EnvPrefix = "env.";

    private readonly List<IReadOnlyDictionary<string, string>> _layers;

    /// <param name="layers">Layers in priority order (highest priority first).</param>
    public VariableScope(params IReadOnlyDictionary<string, string>?[] layers)
        => _layers = layers.Where(l => l is not null).Cast<IReadOnlyDictionary<string, string>>().ToList();

    public bool TryGet(string name, out string value)
    {
        if (name.StartsWith(EnvPrefix, StringComparison.Ordinal))
        {
            var envValue = Environment.GetEnvironmentVariable(name[EnvPrefix.Length..]);
            value = envValue ?? "";
            return envValue is not null;
        }

        foreach (var layer in _layers)
        {
            if (layer.TryGetValue(name, out var v))
            {
                value = v;
                return true;
            }
        }

        var fallback = Environment.GetEnvironmentVariable(name);
        value = fallback ?? "";
        return fallback is not null;
    }

    public string Get(string name)
        => TryGet(name, out var value) ? value : throw new VariableResolutionException(name);
}

public sealed class VariableResolutionException(string variableName)
    : Exception($"Unable to resolve variable '{variableName}'.")
{
    public string VariableName { get; } = variableName;
}
