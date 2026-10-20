using System.Net;

namespace UnitTests.Helpers;

internal sealed class StubHttpMessageHandler : HttpMessageHandler
{
    private readonly HttpStatusCode _status;
    private readonly string _responseBody = "{}";
    private readonly Exception? _throw;

    public StubHttpMessageHandler(HttpStatusCode status = HttpStatusCode.OK, string responseBody = "{}")
    {
        _status = status;
        _responseBody = responseBody;
    }

    public StubHttpMessageHandler(Exception toThrow) => _throw = toThrow;

    public HttpRequestMessage? LastRequest { get; private set; }
    public string? LastRequestBody { get; private set; }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        LastRequest = request;
        if (request.Content is not null)
            LastRequestBody = await request.Content.ReadAsStringAsync(cancellationToken);

        if (_throw is not null)
            throw _throw;

        return new(_status)
        {
            Content = new StringContent(_responseBody),
        };
    }
}