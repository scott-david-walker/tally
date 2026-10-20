namespace Tally.Results;

/// <summary>Outcome of executing a single HTTP request.</summary>
public sealed class RequestResult
{
    public required string Method { get; init; }
    public required string Url { get; init; }
    public string? Name { get; init; }

    public int StatusCode { get; init; }
    public string ResponseBody { get; set; } = ""; //Temp. Should really handle streams directly.
    public IReadOnlyDictionary<string, string> ResponseHeaders { get; init; } = new Dictionary<string, string>();
    public TimeSpan Elapsed { get; init; }

    /// <summary>Set when the request could not be executed at all (e.g. network error).</summary>
    public string? Error { get; init; }

    public bool IsTransportSuccess => Error is null;
}