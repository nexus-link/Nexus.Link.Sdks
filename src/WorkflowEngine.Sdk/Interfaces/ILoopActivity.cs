using System;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Interfaces;

namespace Nexus.Link.WorkflowEngine.Sdk.Interfaces;

/// <summary>
/// An activity that is the parent of underlying instances, such as <see cref="IActivityWhileDo"/> or <see cref="IActivityParallel"/>.
/// </summary>
public interface ILoopActivity : IActivity
{
    /// <summary>
    /// The iteration in the loop. The first iteration is number 1, the second one is number 2, etc.
    /// </summary>
    [Obsolete($"Please use {nameof(LoopIteration)}. Obsolete since 2022-05-08.")]
    int ChildCounter { get; }

    /// <summary>
    /// The iteration in the loop. The first iteration is number 1, the second one is number 2, etc.
    /// </summary>
    int LoopIteration { get; }
}