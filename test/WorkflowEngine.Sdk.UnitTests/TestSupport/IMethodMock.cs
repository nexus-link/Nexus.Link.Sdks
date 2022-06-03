using Nexus.Link.Capabilities.WorkflowState.Abstract.Entities;

namespace WorkflowEngine.Sdk.UnitTests.TestSupport;

public interface IMethodMock
{
    ActivityInstance GetActivityInstance(string activityInstanceId);
}