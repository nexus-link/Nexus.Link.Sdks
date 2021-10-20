using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities.Administration;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;

namespace Nexus.Link.WorkflowEngine.Sdk.Services
{
    public class WorkflowAdministrationService : IWorkflowAdministrationService
    {
        private readonly IWorkflowService _workflowService;
        private readonly IWorkflowInstanceService _workflowInstanceService;
        private readonly IAsyncRequestMgmtCapability _requestMgmtCapability;

        public WorkflowAdministrationService(IWorkflowService workflowService, IWorkflowInstanceService workflowInstanceService, IAsyncRequestMgmtCapability requestMgmtCapability)
        {
            _workflowService = workflowService;
            _requestMgmtCapability = requestMgmtCapability;
            _workflowInstanceService = workflowInstanceService;
        }

        /// <inheritdoc />
        public async Task<Workflow> ReadAsync(string id, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNullOrWhiteSpace(id, nameof(id));

            var workflowRecord = await _workflowService.ReadAsync(id, cancellationToken);
            if (workflowRecord == null) return null;

            var workflow = new Workflow
            {
                Id = id,
                StartedAt = workflowRecord.Instance.StartedAt,
                FinishedAt = workflowRecord.Instance.FinishedAt,
                CancelledAt = workflowRecord.Instance.CancelledAt,
                Title = $"{workflowRecord.Form.Title} {workflowRecord.Version.MajorVersion}.{workflowRecord.Version.MinorVersion}: {workflowRecord.Instance.Title}",
                Activities = await BuildActivityTreeAsync(null, workflowRecord.Activities)
            };

            // TODO: state på hela workflow
            // TODO: Annan enum som sätts på WorkflowInstanceRecord när den ändras

            return workflow;
        }

        /// <inheritdoc />
        public async Task CancelWorkflowAsync(string workflowInstanceId, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNullOrWhiteSpace(workflowInstanceId, nameof(workflowInstanceId));

            var item = await _workflowInstanceService.ReadAsync(workflowInstanceId, cancellationToken);
            if (item == null) throw new FulcrumNotFoundException(workflowInstanceId);

            item.CancelledAt = DateTimeOffset.Now;

            await _workflowInstanceService.UpdateAsync(workflowInstanceId, item, cancellationToken);
            await _requestMgmtCapability.Execution.ReadyForExecutionAsync(workflowInstanceId, cancellationToken);
        }

        private async Task<List<Activity>> BuildActivityTreeAsync(Activity parent, List<Capabilities.WorkflowMgmt.Abstract.Entities.Runtime.Activity> workflowRecordActivities)
        {
            var activities = new List<Activity>();
            foreach (var activityRecord in workflowRecordActivities)
            {
                var activity = new Activity
                {
                    Id = activityRecord.Instance.Id,
                    StartedAt = activityRecord.Instance.StartedAt,
                    FinishedAt = activityRecord.Instance.FinishedAt,
                    Title = $"{activityRecord.Form.Title}",
                    Position = $"{(parent != null ? parent.Position + "." : "")}{activityRecord.Version.Position}",
                    State = activityRecord.Instance.State,
                    FriendlyErrorMessage = activityRecord.Instance.ExceptionFriendlyMessage,
                    TechnicalErrorMessage = activityRecord.Instance.ExceptionTechnicalMessage,
                    WaitingForWorkflow = await GetWaitingForWorkflowAsync(activityRecord)
                };
                activity.Children = await BuildActivityTreeAsync(activity, activityRecord.Children);
                activities.Add(activity);
            }
            return activities;
        }

        private Task<AnnotatedWorkflowId> GetWaitingForWorkflowAsync(Capabilities.WorkflowMgmt.Abstract.Entities.Runtime.Activity activityRecord)
        {
            AnnotatedWorkflowId waitingForWorkflow = null;
            // TODO: Enable when AM capability has support
            //if (string.IsNullOrWhiteSpace(activityRecord.Instance.AsyncRequestId))
            //{
            //    var requestResponse = await _requestResponseService.ReadResponseAsync(activityRecord.Instance.AsyncRequestId);
            //    if (!string.IsNullOrWhiteSpace(requestResponse.ExecutionId))
            //    {
            //        var idAsGuid = MapperHelper.MapToType<Guid, string>(requestResponse.ExecutionId);
            //        var workflowInstance = await _runtimeTables.WorkflowInstance.ReadAsync(idAsGuid);
            //        if (workflowInstance != null)
            //        {
            //            waitingForWorkflow = new AnnotatedWorkflowId
            //            {
            //                Id = requestResponse.ExecutionId,
            //                Title = $"{workflowInstance.Title}" // TODO: Good enough title?
            //            };
            //        }
            //    }
            //}

            return Task.FromResult(waitingForWorkflow);
        }
    }
}