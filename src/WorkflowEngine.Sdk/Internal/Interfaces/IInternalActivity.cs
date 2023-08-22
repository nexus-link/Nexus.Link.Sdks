using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Configuration.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.State.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;

namespace Nexus.Link.WorkflowEngine.Sdk.Internal.Interfaces;

internal interface IInternalActivity : IInternalActivityBase, IActivity
{
    ActivityInstance Instance { get; }
    ActivityVersion Version { get; }
    void PromoteOrPurgeLogs();
    Task SafeAlertExceptionAsync(CancellationToken cancellationToken); 
    ILogicExecutor LogicExecutor { get; }
}

internal interface IInternalActivity<TActivityReturns> : IInternalActivity
{
    ActivityDefaultValueMethodAsync<TActivityReturns> DefaultValueMethodAsync { get; }

    TActivityReturns GetResult();
}