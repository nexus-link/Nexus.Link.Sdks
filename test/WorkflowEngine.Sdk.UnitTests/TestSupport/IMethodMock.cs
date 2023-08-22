using Nexus.Link.WorkflowEngine.Sdk.Abstract.State.Entities;

namespace WorkflowEngine.Sdk.UnitTests.TestSupport;

public interface IMethodMock
{
    ActivityInstance GetActivityInstance(string activityInstanceId);
}