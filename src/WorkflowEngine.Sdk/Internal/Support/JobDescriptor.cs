using System;

namespace Nexus.Link.WorkflowEngine.Sdk.Internal.Support;

internal class JobDescriptor
{
    public int Index { get; set; }

    public object Job { get; set; }

    public Type Type { get; set; }

    public string IterationTitle { get; set; }
}