using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Activities;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Interfaces;
using WorkflowEngine.Sdk.UnitTests.TestSupport;

namespace WorkflowEngine.Sdk.UnitTests.Internal.Activities;

public abstract class ActivityTestsBase
{
    internal readonly WorkflowInformationMock _workflowInformationMock;
    internal Mock<IActivityExecutor> _activityExecutorMock { get; }
    internal ActivityInformationMock _activityInformationMock { get; }
    internal Mock<ILogicExecutor> _logicExecutorMock { get; }

    protected ActivityTestsBase(string testClassName)
    {
        FulcrumApplicationHelper.UnitTestSetup(testClassName);
        _activityExecutorMock = new Mock<IActivityExecutor>();
        _logicExecutorMock = new Mock<ILogicExecutor>();
        _workflowInformationMock = new WorkflowInformationMock(_activityExecutorMock.Object, _logicExecutorMock.Object);
        _activityInformationMock = new ActivityInformationMock(_workflowInformationMock);
        _activityExecutorMock.Setup(executor =>
                executor.ExecuteWithoutReturnValueAsync(It.IsAny<InternalActivityMethodAsync>(),
                    It.IsAny<CancellationToken>()))
            .Callback((InternalActivityMethodAsync m, CancellationToken ct) => m.Invoke(ct));
        _activityExecutorMock.Setup(executor =>
                executor.DoExtraAdminAsync(It.IsAny<Func<CancellationToken, Task>>(),
                    It.IsAny<CancellationToken>()))
            .Callback((Func<CancellationToken, Task> m, CancellationToken ct) => m.Invoke(ct));
    }
}