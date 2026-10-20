using Tally.Readers;
using UnitTests.Helpers;

namespace UnitTests.Readers;

public class YamlLoaderTests
{
    private readonly YamlLoader _loader = new();

    [Fact]
    public void CanLoadRequestDefinition()
    {
        using var ws = new TempWorkspace();
        var path = ws.Write("req.yaml", """
            name: Create Order
            vars:
              test:
                required: true
                default: "test"
              testagain:
                required: false
                default: "default"
            request:
              method: POST
              url: "{{baseUrl}}/orders"
              headers:
                Authorization: "Bearer {{authToken}}"
              body:
                type: file
                location: "somelocation"
            capture:
              id: 
                json:
                  path: abc
            """);

        var requestDefinition = _loader.LoadRequest(path);

        Assert.Equal("Create Order", requestDefinition.Name);
        Assert.Equal("POST", requestDefinition.Request.Method);
        Assert.True(requestDefinition.Vars!.ContainsKey("test"));
        Assert.Equal("test", requestDefinition.Vars["test"].Default);
        Assert.True(requestDefinition.Vars["test"].Required);
        Assert.True(requestDefinition.Vars!.ContainsKey("testagain"));
        Assert.Equal("default", requestDefinition.Vars["testagain"].Default);
        Assert.False(requestDefinition.Vars["testagain"].Required);
        Assert.Equal("POST", requestDefinition.Request.Method);
        Assert.Equal("{{baseUrl}}/orders", requestDefinition.Request.Url);
        Assert.Equal("Bearer {{authToken}}", requestDefinition.Request.Headers!["Authorization"]);
        Assert.NotNull(requestDefinition.Request.Body);
        Assert.Equal("file", requestDefinition.Request.Body!.Type);
        Assert.Equal("somelocation", requestDefinition.Request.Body.Location);
        Assert.True(requestDefinition.Capture!.ContainsKey("id"));
        Assert.Equal("abc", requestDefinition.Capture["id"].Json!.Path);
    }
    [Fact]
    public void WhenFileIsMissing_ThrowsException()
        => Assert.Throws<FileNotFoundException>(() => _loader.LoadRequest("/no/such/file.yaml"));
}
