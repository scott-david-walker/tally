namespace Tally.Models.Request;

public class RequestDefinition
{  
    public required string Name { get; set; }
    public Dictionary<string, Input>? Vars { get; set; }
    public required Request Request { get; set; }
    public Dictionary<string, Capture>? Capture { get; set; }
}