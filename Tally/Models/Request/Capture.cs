namespace Tally.Models.Request;

public class Capture
{
    public JsonCapture? Json { get; init; }
}

public class JsonCapture
{
    public required string Path { get; init; }
}