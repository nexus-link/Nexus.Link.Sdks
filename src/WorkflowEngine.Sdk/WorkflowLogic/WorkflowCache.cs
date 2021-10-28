using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities.Runtime;
using Nexus.Link.Libraries.Core.Threads;

namespace Nexus.Link.WorkflowEngine.Sdk.WorkflowLogic
{
    public class WorkflowCache
    {
        private readonly IWorkflowCapability _workflowCapability;
        private readonly string _workflowInstanceId;
        public WorkflowSummary WorkflowSummary { get; private set; }
        private readonly NexusAsyncSemaphore _semaphore = new NexusAsyncSemaphore();

        public async Task<WorkflowSummary> LoadDataAsync()
        {
            if (WorkflowSummary != null) return WorkflowSummary;
            return await _semaphore.ExecuteAsync(async () => 
            {
                if (WorkflowSummary != null) return WorkflowSummary;
                WorkflowSummary = await _workflowCapability.Workflow.ReadAsync(_workflowInstanceId);
                return WorkflowSummary;
            });
        }

        public WorkflowCache(IWorkflowCapability workflowCapability, string workflowInstanceId)
        {
            _workflowCapability = workflowCapability;
            _workflowInstanceId = workflowInstanceId;
        }
    }
}