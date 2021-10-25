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
        public Workflow WorkflowHierarchy { get; private set; }
        private readonly NexusAsyncSemaphore _semaphore = new NexusAsyncSemaphore();

        public async Task<Workflow> LoadDataAsync()
        {
            if (WorkflowHierarchy != null) return WorkflowHierarchy;
            return await _semaphore.ExecuteAsync(async () => 
            {
                if (WorkflowHierarchy != null) return WorkflowHierarchy;
                WorkflowHierarchy = await _workflowCapability.Workflow.ReadAsync(_workflowInstanceId);
                return WorkflowHierarchy;
            });
        }

        public WorkflowCache(IWorkflowCapability workflowCapability, string workflowInstanceId)
        {
            _workflowCapability = workflowCapability;
            _workflowInstanceId = workflowInstanceId;
        }
    }
}