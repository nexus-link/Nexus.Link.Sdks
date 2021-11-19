using System.Threading;
using System.Threading.Tasks;

namespace Nexus.Link.WorkflowEngine.Sdk.Interfaces
{ 
    public interface IWorkflowImplementationBase : IWorkflowLogger
    {
        int MajorVersion { get; }
        int MinorVersion { get; }

        string GetInstanceTitle();
        IWorkflowVersions WorkflowVersions { get; }
    }

    public interface IWorkflowImplementation<TWorkflowResult> : IWorkflowImplementationBase
    {
        IWorkflowImplementation<TWorkflowResult> CreateWorkflowInstance();
        Task<TWorkflowResult> ExecuteAsync(CancellationToken cancellationToken);
        IWorkflowImplementation<TWorkflowResult> SetParameter<TParameter>(string name, TParameter value);
    }

    public interface IWorkflowImplementation : IWorkflowImplementationBase
    {
        IWorkflowImplementation CreateWorkflowInstance();
        Task ExecuteAsync(CancellationToken cancellationToken);
        IWorkflowImplementation SetParameter<TParameter>(string name, TParameter value);
    }
}