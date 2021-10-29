using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities.State;
using Nexus.Link.Libraries.Core.Threads;

namespace Nexus.Link.WorkflowEngine.Sdk.WorkflowLogic
{
    public class WorkflowCache
    {
        private readonly IWorkflowMgmtCapability _workflowCapability;
        private readonly string _workflowInstanceId;
        public WorkflowSummary WorkflowSummary { get; private set; }
        private readonly NexusAsyncSemaphore _semaphore = new NexusAsyncSemaphore();

        public async Task<WorkflowSummary> LoadDataAsync()
        {
            if (WorkflowSummary != null) return WorkflowSummary;
            return await _semaphore.ExecuteAsync(async () => 
            {
                if (WorkflowSummary != null) return WorkflowSummary;
                WorkflowSummary = await _workflowCapability.WorkflowSummary.ReadAsync(_workflowInstanceId);
                return WorkflowSummary;
            });
        }

        public WorkflowCache(IWorkflowMgmtCapability workflowCapability, string workflowInstanceId)
        {
            _workflowCapability = workflowCapability;
            _workflowInstanceId = workflowInstanceId;
        }
    }
}