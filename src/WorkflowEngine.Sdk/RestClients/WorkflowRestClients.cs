using Nexus.Link.Capabilities.WorkflowMgmt.Abstract;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services;
using Nexus.Link.Libraries.Web.RestClientHelper;
using Nexus.Link.WorkflowEngine.Sdk.Services;

namespace Nexus.Link.WorkflowEngine.Sdk.RestClients
{
    public class WorkflowRestClients : IWorkflowCapability
    {
        public WorkflowRestClients(IHttpSender httpSender)
        {
            WorkflowForm = new WorkflowFormRestClient(httpSender.CreateHttpSender("Configuration"));
            WorkflowVersion = new WorkflowVersionRestClient(httpSender.CreateHttpSender("Configuration"));
            WorkflowParameter = new WorkflowParameterRestClient(httpSender.CreateHttpSender("Configuration"));
            ActivityForm= new ActivityFormRestClient(httpSender.CreateHttpSender("Configuration"));
            ActivityVersion= new ActivityVersionRestClient(httpSender.CreateHttpSender("Configuration"));
            ActivityInstance= new ActivityInstanceRestClient(httpSender.CreateHttpSender("Runtime"));
            Transition= new TransitionRestClient(httpSender.CreateHttpSender("Configuration"));
            ActivityParameter = new ActivityParameterRestClient(httpSender.CreateHttpSender("Configuration"));
            WorkflowInstance = new WorkflowInstanceRestClient(httpSender.CreateHttpSender("Runtime"));
            Workflow = new WorkflowRestClient(httpSender.CreateHttpSender("Runtime"));
            WorkflowAdministrationService = new WorkflowAdministrationRestClient(httpSender.CreateHttpSender("Administration"));
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

        /// <inheritdoc />
        public IWorkflowAdministrationService WorkflowAdministrationService { get; }
    }
}