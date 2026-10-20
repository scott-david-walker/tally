using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using Tally.Models.Request;
using Tally.Results;
using Tally.Variables;

namespace Tally.Executors;

public sealed class HttpRequestExecutor(HttpClient httpClient) : IExecutor
{
    private const string ContentTypeHeader = "Content-Type";

    public async Task<RequestResult> ExecuteAsync(
        string requestName,
        Request request,
        VariableScope scope,
        CancellationToken cancellationToken = default)
    {
        var method = new HttpMethod(request.Method.ToUpperInvariant());
        var url = TemplateEngine.Render(request.Url, scope);

        using var message = new HttpRequestMessage(method, url);
        var contentType = "application/json";

        foreach (var (key, rawValue) in request.Headers)
        {
            var value = TemplateEngine.Render(rawValue, scope);
            if (string.Equals(key, ContentTypeHeader, StringComparison.OrdinalIgnoreCase))
                contentType = value;
            else
                message.Headers.TryAddWithoutValidation(key, value);
        }

        if (request.Body is { } body)
            message.Content = await BuildContent(body, scope, contentType);

        var stopwatch = Stopwatch.StartNew();
        try
        {
            using var response = await httpClient.SendAsync(message, cancellationToken);
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
            stopwatch.Stop();

            return new()
            {
                Name = requestName,
                Method = method.Method,
                Url = url,
                StatusCode = (int)response.StatusCode,
                ResponseBody = responseBody,
                ResponseHeaders = response.Headers
                    .Concat(response.Content.Headers)
                    .ToDictionary(h => h.Key, h => string.Join(", ", h.Value)),
                Elapsed = stopwatch.Elapsed,
            };
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            stopwatch.Stop();
            return new()
            {
                Name = requestName,
                Method = method.Method,
                Url = url,
                Elapsed = stopwatch.Elapsed,
                Error = ex.Message,
            };
        }
    }

    private static async Task<StringContent> BuildContent(RequestBody body, VariableScope scope, string contentType)
    {
        var data = await File.ReadAllTextAsync(body.Location!);
        var resolved = TemplateEngine.Render(data, scope);

        var content = new StringContent(resolved, Encoding.UTF8);
        content.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);
        return content;
    }
}
