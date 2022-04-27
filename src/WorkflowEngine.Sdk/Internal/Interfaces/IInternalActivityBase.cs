using Nexus.Link.WorkflowEngine.Sdk.Exceptions;

namespace Nexus.Link.WorkflowEngine.Sdk.Internal.Interfaces;

internal interface IInternalActivityBase
{
    IActivityInformation ActivityInformation { get; }
    void MarkAsSuccess();
#pragma warning disable CS0618
    void MarkAsFailed(ActivityException exception);
#pragma warning restore CS0618
}