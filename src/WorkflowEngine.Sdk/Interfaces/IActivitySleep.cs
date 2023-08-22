using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Configuration.Entities;

namespace Nexus.Link.WorkflowEngine.Sdk.Interfaces;

/// <summary>
/// An activity of type <see cref="ActivityTypeEnum.Action"/>.
/// </summary>
public interface IActivitySleep : IActivity
{
    /// <summary>
    /// Make the workflow sleep
    /// </summary>
    Task ExecuteAsync(CancellationToken cancellationToken = default);
}