using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Activities;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Exceptions;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.State.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Logic;

namespace WorkflowEngine.Sdk.UnitTests.TestSupport;

internal class ActivityMock : ActivityBase, IInternalActivity
{
    /// <inheritdoc />
    public ActivityMock(IActivityInformation activityInformation) : base(activityInformation)
    {
    }

    public int MaybePurgeLogsCalled { get; private set; }
    public int SafeAlertExceptionCalled { get; private set; }

    /// <inheritdoc />
    public Task LogAtLevelAsync(LogSeverityLevel severityLevel, string message, object data = null,
        CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public T GetArgument<T>(string parameterName)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public T GetActivityArgument<T>(string parameterName)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public ActivityFailedException GetException()
    {
        if (Instance.State != ActivityStateEnum.Failed) return null;
        FulcrumAssert.IsNotNull(Instance.ExceptionCategory, CodeLocation.AsString());
        return new ActivityFailedException(Instance.ExceptionCategory!.Value, Instance.ExceptionTechnicalMessage,
            Instance.ExceptionFriendlyMessage);
    }

    /// <inheritdoc />
    public void PromoteOrPurgeLogs()
    {
        MaybePurgeLogsCalled++;
    }

    /// <inheritdoc />
    public Task SafeAlertExceptionAsync(CancellationToken cancellationToken)
    {
        SafeAlertExceptionCalled++;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public ILogicExecutor LogicExecutor => ActivityInformation.Workflow.GetLogicExecutor(this);
}

/// <inheritdoc cref="ActivityMock{TActivity}" />
internal class ActivityMock<TActivityReturns> : ActivityMock, IInternalActivity<TActivityReturns>
{
    public ActivityDefaultValueMethodAsync<TActivityReturns> DefaultValueMethodAsync { get; }

    public ActivityMock(IActivityInformation activityInformation,
        ActivityDefaultValueMethodAsync<TActivityReturns> defaultValueMethodAsync)
        : base(activityInformation)
    {
        DefaultValueMethodAsync = defaultValueMethodAsync;
    }

    public TActivityReturns GetResult()
    {
        return ActivityInformation.Workflow.GetActivityResult<TActivityReturns>(ActivityInstanceId);
    }
}