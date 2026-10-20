namespace Tally.Models.Request;

public class Request
{
    public string? Id { get; init; }
    public required string Method { get; init; }
    public required string Url { get; init; }
    
    public Dictionary<string, string> Headers { get; init; } = [];
    
    public Dictionary<string, string>? QueryParameters { get; init; } = [];
    public RequestBody? Body { get; init; }
}

public sealed class RequestBody
{
    public string Type { get; init; } = "file";
    public string? Location { get; init; }
}