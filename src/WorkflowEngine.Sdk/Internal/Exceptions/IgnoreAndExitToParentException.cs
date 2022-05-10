using Nexus.Link.Libraries.Web.Error.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Exceptions;

namespace Nexus.Link.WorkflowEngine.Sdk.Internal.Exceptions;

internal class IgnoreAndExitToParentException : RequestPostponedException
{
    public ActivityFailedException ActivityFailedException { get; }

    public IgnoreAndExitToParentException(ActivityFailedException activityFailedException)
    {
        ActivityFailedException = activityFailedException;
    }
}