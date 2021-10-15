using Nexus.Link.Capabilities.WorkflowMgmt.Abstract;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services;
using Nexus.Link.Libraries.Web.RestClientHelper;

namespace Nexus.Link.WorkflowEngine.Sdk.RestClients
{
    public class WorkflowRestClients : IWorkflowCapability
    {
        public WorkflowRestClients(IHttpSender httpSender)
        {
            WorkflowForm = new WorkflowFormRestClient(httpSender);
            WorkflowVersion = new WorkflowVersionRestClient(httpSender);
            WorkflowParameter = new WorkflowParameterRestClient(httpSender);
            ActivityForm= new ActivityFormRestClient(httpSender);
            ActivityVersion= new ActivityVersionRestClient(httpSender);
            ActivityInstance= new ActivityInstanceRestClient(httpSender);
            Transition= new TransitionRestClient(httpSender);
            ActivityParameter = new ActivityParameterRestClient(httpSender);
            WorkflowInstance = new WorkflowInstanceRestClient(httpSender);
        }

        /// <inheritdoc />
        public IWorkflowFormService WorkflowForm { get; }

        /// <inheritdoc />
        public IWorkflowVersionService WorkflowVersion { get; }

        /// <inheritdoc />
        public IWorkflowParameterService WorkflowParameter { get; }

        /// <inheritdoc />
        public IActivityFormService ActivityForm { get; }

        /// <inheritdoc />
        public IActivityVersionService ActivityVersion { get; }

        /// <inheritdoc />
        public IActivityInstanceService ActivityInstance { get; }

        /// <inheritdoc />
        public ITransitionService Transition { get; }

        /// <inheritdoc />
        public IActivityParameterService ActivityParameter { get; }

        /// <inheritdoc />
        public IWorkflowInstanceService WorkflowInstance { get; }

        /// <inheritdoc />
        public IWorkflowService Workflow { get; }
    }
}