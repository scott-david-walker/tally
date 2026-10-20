using Tally.Models.Request;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Tally.Readers;

/// <summary>
/// Loads request and scenario definitions from YAML files on disk.
/// </summary>
public sealed class YamlLoader
{
    private readonly IDeserializer _deserializer = new DeserializerBuilder()
        .WithNamingConvention(LowerCaseNamingConvention.Instance)
        .IgnoreUnmatchedProperties()
        .Build();

    public RequestDefinition LoadRequest(string path)
        => Load<RequestDefinition>(path);

    private T Load<T>(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException($"YAML file not found: {path}", path);

        var text = File.ReadAllText(path);
        return _deserializer.Deserialize<T>(text)
               ?? throw new InvalidDataException($"YAML file is empty or invalid: {path}");
    }
}