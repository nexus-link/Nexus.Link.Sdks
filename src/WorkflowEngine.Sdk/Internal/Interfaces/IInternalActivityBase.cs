using System.Collections.Generic;
using Nexus.Link.WorkflowEngine.Sdk.Exceptions;

namespace Nexus.Link.WorkflowEngine.Sdk.Internal.Interfaces;

internal interface IInternalActivityBase
{
    IActivityInformation ActivityInformation { get; }
    void MarkAsSuccess();
#pragma warning disable CS0618
    void MarkAsFailed(ActivityException exception);
#pragma warning restore CS0618
    List<int> NestedIterations { get; }
    string NestedPosition { get; }

    /// <summary>
    /// If the activity is part of a loop, this is the iteration count for that loop
    /// </summary>
    ///
    int? InternalIteration { get; set; }
}