using System.Collections.Generic;
using System.Threading.Tasks;

namespace WorkflowEngine.Sdk.UnitTests.Services.State;

/// <summary>
/// Support for verifying during testing
/// </summary>
public interface ITestVerify
{
    Task<ICollection<string>> GetRaisedWorkflowInstanceIdsAsync();
}