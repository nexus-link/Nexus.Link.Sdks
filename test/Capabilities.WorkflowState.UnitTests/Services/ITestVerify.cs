using System.Collections.Generic;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Web.Error.Logic;

namespace Nexus.Link.Capabilities.WorkflowState.UnitTests.Services;

/// <summary>
/// Support for verifying during testing
/// </summary>
public interface ITestVerify
{
    Task<ICollection<string>> GetRaisedWorkflowInstanceIdsAsync();
}