namespace Nexus.Link.WorkflowEngine.Sdk.Interfaces;

/// <summary>
/// An activity that is the parent of underlying instances, such as <see cref="IActivityWhileDo"/> or <see cref="IActivityParallel"/>.
/// </summary>
public interface IParentActivity
{
    /// <summary>
    /// The iteration in the loop. The first iteration is number 1, the second one is number 2, etc.
    /// </summary>
    int ChildCounter { get; }
}