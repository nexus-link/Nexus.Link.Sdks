using System.Threading;
using System.Threading.Tasks;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract;

/// <summary>
/// A way to delete all objects of a storage
/// </summary>
public interface IDeleteAll
{
    /// <summary>
    /// Delete all objects. Typically used for testing purposes, e.g. delete all data in database tables before starting the test
    /// </summary>
    Task DeleteAllAsync(CancellationToken cancellationToken = default);
}