using System.Threading.Tasks;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract;

public interface IDeleteAll
{
    /// <summary>
    /// Delete all objects. Typically used for testing purposes, e.g. delete all data in database tables before starting the test
    /// </summary>
    Task DeleteAllAsync();
}