using System.Net;
using System.Text.Json.Nodes;
using Tally.Executors;
using Tally.Models.Request;
using Tally.Variables;
using UnitTests.Helpers;

namespace UnitTests;

public class HttpRequestExecutorTests
{
    private static Tally.Variables.VariableScope Scope(params (string Key, string Value)[] vars)
        => new(vars.ToDictionary(v => v.Key, v => v.Value));

    [Fact]
    public async Task CanRenderUrlAndBody()
    {
        var stub = new StubHttpMessageHandler(HttpStatusCode.Created, """{"id":1}""");
        var executor = new HttpRequestExecutor(new(stub));

        var request = new Request
        {
            Method = "post",
            Url = "{{baseUrl}}/orders",
            Headers = { ["Authorization"] = "Bearer {{authToken}}" },
            Body = new()
            {
                Location = TestFileReader.GetFileLocation("userIdAsVariable.json"),
            },
        };

        var result = await executor.ExecuteAsync("Create", request, Scope(
            ("baseUrl", "https://api.test"),
            ("authToken", "tok"),
            ("userId", "7")));

        Assert.Equal(201, result.StatusCode);
        Assert.Equal("POST", stub.LastRequest!.Method.Method);
        Assert.Equal("https://api.test/orders", stub.LastRequest.RequestUri!.ToString());
        Assert.Equal("Bearer tok", stub.LastRequest.Headers.GetValues("Authorization").Single());

        var sent = JsonNode.Parse(stub.LastRequestBody!)!;
        Assert.Equal("7", sent["userId"]!.GetValue<string>());
        Assert.Equal(42, sent["amount"]!.GetValue<int>());
    }

    [Fact]
    public async Task CanAcceptsContentTypeWithCharsetParameter()
    {
        var stub = new StubHttpMessageHandler();
        var executor = new HttpRequestExecutor(new(stub));

        var request = new Request
        {
            Method = "POST",
            Url = "https://api.test/x",
            Headers = { ["Content-Type"] = "application/json; charset=UTF-8" },
            Body = new()
            {       
                Location = TestFileReader.GetFileLocation("standard.json"),
            },
        };

        var result = await executor.ExecuteAsync("Create", request, Scope());

        Assert.True(result.IsTransportSuccess);
        Assert.Equal("application/json", stub.LastRequest!.Content!.Headers.ContentType!.MediaType);
        Assert.Equal("utf-8", stub.LastRequest.Content.Headers.ContentType.CharSet, ignoreCase: true);
    }

    [Fact]
    public async Task WhenTransportError_ShouldNotThrow()
    {
        var stub = new StubHttpMessageHandler(new HttpRequestException("connection refused"));
        var executor = new HttpRequestExecutor(new(stub));

        var request = new Request { Method = "GET", Url = "https://unreachable.test/" };
        var result = await executor.ExecuteAsync("Request", request, Scope());

        Assert.False(result.IsTransportSuccess);
        Assert.Contains("connection refused", result.Error);
    }

    [Fact]
    public async Task WhenVariableIsMissing_ShouldThrow()
    {
        var executor = new HttpRequestExecutor(new(new StubHttpMessageHandler()));
        var request = new Request { Method = "GET", Url = "{{missing}}/x" };
        await Assert.ThrowsAsync<VariableResolutionException>(() => executor.ExecuteAsync("", request, Scope()));
    }
}
