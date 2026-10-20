using Tally.Models.Request;
using Tally.Results;
using Tally.Variables;

namespace Tally.Executors;

public interface IExecutor
{
    Task<RequestResult> ExecuteAsync(string requestName, Request request, VariableScope scope, CancellationToken cancellationToken = default);
}