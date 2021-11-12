using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities.State;

namespace Nexus.Link.WorkflowEngine.Sdk.Interfaces
{
    public interface IActivity
    {
        string WorkflowInstanceId { get; }
        string ActivityInstanceId { get; }
        string ActivityTitle { get; }
        int? Iteration { get; }
        ActivityFailUrgencyEnum FailUrgency { get; }

        TParameter GetArgument<TParameter>(string parameterName);
    }
}