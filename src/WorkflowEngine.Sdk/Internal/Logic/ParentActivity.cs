using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Interfaces;

namespace Nexus.Link.WorkflowEngine.Sdk.Internal.Logic;

/// <inheritdoc cref="IInternalActivity" />
internal abstract class ParentActivity : Activity, IParentActivity
{
    protected ParentActivity(IActivityInformation activityInformation)
        : base(activityInformation)
    {
        ChildCounter = 0;
    }

    /// <inheritdoc />
    public int ChildCounter
    {
        get => InternalIteration ?? 0;
        set => InternalIteration = value;
    }
}

/// <inheritdoc/>
internal abstract class ParentActivity<TActivityReturns> : ParentActivity
{

    protected ActivityDefaultValueMethodAsync<TActivityReturns> DefaultValueMethodAsync { get; }

    protected ParentActivity(IActivityInformation activityInformation,
        ActivityDefaultValueMethodAsync<TActivityReturns> defaultValueMethodAsync)
        : base(activityInformation)
    {
        DefaultValueMethodAsync = defaultValueMethodAsync;
    }
}