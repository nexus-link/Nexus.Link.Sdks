using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract;
using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract;
using Nexus.Link.Capabilities.WorkflowState.Abstract;
using Nexus.Link.Capabilities.WorkflowState.Abstract.Entities;
using Nexus.Link.Components.WorkflowMgmt.Abstract.Entities;
using Nexus.Link.Components.WorkflowMgmt.Abstract.Services;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Misc.Models;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract;

namespace Nexus.Link.WorkflowEngine.Sdk.Services.Administration
{
    public class WorkflowService : IWorkflowService
    {
        private readonly IWorkflowStateCapability _stateCapability;
        private readonly IWorkflowConfigurationCapability _configurationCapability;
        private readonly IRuntimeTables _runtimeTables;
        private readonly IAsyncRequestMgmtCapability _requestMgmtCapability;

        public WorkflowService(IWorkflowStateCapability stateCapability, IWorkflowConfigurationCapability configurationCapability, IAsyncRequestMgmtCapability requestMgmtCapability, IRuntimeTables runtimeTables)
        {
            _stateCapability = stateCapability;
            _requestMgmtCapability = requestMgmtCapability;
            _runtimeTables = runtimeTables;
            _configurationCapability = configurationCapability;
        }

        /// <inheritdoc />
        public async Task<Workflow> ReadAsync(string id, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNullOrWhiteSpace(id, nameof(id));

            var workflowSummary = await _stateCapability.WorkflowSummary.GetSummaryAsync(id, cancellationToken);
            if (workflowSummary == null) return null;

            var workflow = new Workflow
            {
                Id = id,
                WorkflowFormId = workflowSummary.Form.Id,
                WorkflowVersionId = workflowSummary.Version.Id,
                State = workflowSummary.Instance.State,
                StartedAt = workflowSummary.Instance.StartedAt,
                FinishedAt = workflowSummary.Instance.FinishedAt,
                CancelledAt = workflowSummary.Instance.CancelledAt,
                Title = $"{workflowSummary.Form.Title} {workflowSummary.Version.MajorVersion}.{workflowSummary.Version.MinorVersion}: {workflowSummary.Instance.Title}",
                Activities = await BuildActivityTreeAsync(null, workflowSummary.ActivityTree)
            };

            return workflow;
        }

        /// <inheritdoc />
        public async Task CancelAsync(string workflowInstanceId, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNullOrWhiteSpace(workflowInstanceId, nameof(workflowInstanceId));

            var item = await _stateCapability.WorkflowInstance.ReadAsync(workflowInstanceId, cancellationToken);
            if (item == null) throw new FulcrumNotFoundException(workflowInstanceId);

            item.CancelledAt = DateTimeOffset.Now;

            await _stateCapability.WorkflowInstance.UpdateAsync(workflowInstanceId, item, cancellationToken);
            await _requestMgmtCapability.Request.RetryAsync(workflowInstanceId, cancellationToken);
        }

        private async Task<List<Activity>> BuildActivityTreeAsync(Activity parent, IReadOnlyList<ActivitySummary> workflowRecordActivities)
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
                    Type = activityRecord.Form.Type,
                    Position = $"{(parent != null ? parent.Position + "." : "")}{activityRecord.Version.Position}",
                    State = activityRecord.Instance.State,
                    ResultAsJson = activityRecord.Instance.ResultAsJson,
                    FriendlyErrorMessage = activityRecord.Instance.ExceptionFriendlyMessage,
                    TechnicalErrorMessage = activityRecord.Instance.ExceptionTechnicalMessage,
                    WaitingForWorkflow = await GetWaitingForWorkflowAsync(activityRecord)
                };
                activity.Children = await BuildActivityTreeAsync(activity, activityRecord.Children);
                activities.Add(activity);
            }
            return activities;
        }

        private Task<AnnotatedId<string>> GetWaitingForWorkflowAsync(ActivitySummary activityRecord)
        {
            AnnotatedId<string> waitingForWorkflow = null;
            // TODO: Enable when AM capability has support
            //if (string.IsNullOrWhiteSpace(activityRecord.Instance.AsyncRequestId))
            //{
            //    var requestResponse = await _requestResponseService.ReadResponseAsync(activityRecord.Instance.AsyncRequestId);
            //    if (!string.IsNullOrWhiteSpace(requestResponse.ExecutionId))
            //    {
            //        var idAsGuid = requestResponse.ExecutionId.ToGuid();
            //        var workflowInstance = await _runtimeTables.WorkflowInstance.ReadAsync(idAsGuid);
            //        if (workflowInstance != null)
            //        {
            //            waitingForWorkflow = new AnnotatedId<string>
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