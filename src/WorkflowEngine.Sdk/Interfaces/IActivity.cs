namespace Nexus.Link.WorkflowEngine.Sdk.Interfaces
{
    public interface IActivity
    {
        string InstanceId { get; }
        string Title { get; }
        int? Iteration { get; }

        TParameter GetArgument<TParameter>(string parameterName);
    }
}