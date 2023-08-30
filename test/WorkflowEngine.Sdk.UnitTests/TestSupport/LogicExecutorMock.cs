using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Activities;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Interfaces;

namespace WorkflowEngine.Sdk.UnitTests.TestSupport;

internal class LogicExecutorMock : ILogicExecutor
{
    /// <inheritdoc />
    public IInternalActivity Activity { get; set; }

    public ConcurrentDictionary<string, int> ExecuteWithoutReturnValueCounter { get; } = new();

    public ConcurrentDictionary<string, int> ExecuteWithReturnValueCounter { get; } = new();

    /// <inheritdoc />
    public async Task ExecuteWithoutReturnValueAsync(InternalActivityMethodAsync methodAsync, string methodName,
        CancellationToken cancellationToken = default)
    {
        ExecuteWithoutReturnValueCounter.TryAdd(methodName, 0);
        ExecuteWithoutReturnValueCounter[methodName]++;
        await methodAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<TMethodReturns> ExecuteWithReturnValueAsync<TMethodReturns>(InternalActivityMethodAsync<TMethodReturns> methodAsync, string methodName,
        CancellationToken cancellationToken = default)
    {
        ExecuteWithReturnValueCounter.TryAdd(methodName, 0);
        ExecuteWithReturnValueCounter[methodName]++;
        var result = await methodAsync(cancellationToken);
        return result;
    }
}