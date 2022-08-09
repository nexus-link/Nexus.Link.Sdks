using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nexus.Link.Capabilities.WorkflowState.UnitTests.Services;

/// <summary>
/// Support for verifying during testing
/// </summary>
public interface ITestVerify
{
    Task<ICollection<string>> GetRaisedWorkflowInstanceIdsAsync();
}