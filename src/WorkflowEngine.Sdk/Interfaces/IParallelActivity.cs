using System;

namespace Nexus.Link.WorkflowEngine.Sdk.Interfaces;

/// <summary>
/// An activity that is the parent of underlying instances, such as <see cref="IActivityWhileDo"/> or <see cref="IActivityParallel"/>.
/// </summary>
public interface IParallelActivity
{
    /// <summary>
    /// The number of an individual job in the parallel activity.
    /// </summary>
    int JobNumber { get; }
}