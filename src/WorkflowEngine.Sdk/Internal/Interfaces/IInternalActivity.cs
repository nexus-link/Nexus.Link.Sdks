using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract.Entities;
using Nexus.Link.Capabilities.WorkflowState.Abstract.Entities;
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