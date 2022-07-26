using System;
using Nexus.Link.Libraries.Web.Error.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Exceptions;

namespace Nexus.Link.WorkflowEngine.Sdk.Internal.Exceptions;

[Obsolete("This will not be supported. Please use Action+Catch. Obsolete since 2022-06-15.")]
internal class IgnoreAndExitToParentException : RequestPostponedException
{
    public ActivityFailedException ActivityFailedException { get; }

    public IgnoreAndExitToParentException(ActivityFailedException activityFailedException)
    {
        ActivityFailedException = activityFailedException;
    }
}