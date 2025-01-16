using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Core.Misc.Models;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Component.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Component.Services;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Configuration;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Configuration.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.State;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.State.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Extensions.State;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract;

namespace Nexus.Link.WorkflowEngine.Sdk.Services.Administration
{
    /// <inheritdoc />
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
                    Type =  activityRecord.Form.Type,
                    Position = $"{(parent != null ? parent.Position + "." : "")}{activityRecord.Version.Position}",
                    State = activityRecord.Instance.State,
                    ResultAsJson = activityRecord.Instance.ResultAsJson,
                    FriendlyErrorMessage = activityRecord.Instance.ExceptionFriendlyMessage,
                    TechnicalErrorMessage = activityRecord.Instance.ExceptionTechnicalMessage,
                    IterationTitle = activityRecord.Instance.IterationTitle,
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

        /// <inheritdoc />
        public async Task RetryHaltedAsync(string workflowInstanceId, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNullOrWhiteSpace(workflowInstanceId, nameof(workflowInstanceId));

            // Workflow Instances
            var workflowInstanceService = _stateCapability.WorkflowInstance;
            FulcrumAssert.IsNotNull((workflowInstanceService, CodeLocation.AsString()));

            var workflowDistributedLock = await workflowInstanceService.ClaimDistributedLockAsync(
                workflowInstanceId, null, null, cancellationToken);

            try
            {
                var workflowInstance = await workflowInstanceService.ReadAsync(workflowInstanceId, cancellationToken);
                if (workflowInstance == null) throw new FulcrumNotFoundException(workflowInstanceId);
                if (workflowInstance.State != WorkflowStateEnum.Halted && workflowInstance.State != WorkflowStateEnum.Halting)
                {
                    return;
                }

                var workflowSummary = await _stateCapability.WorkflowSummary.GetSummaryAsync(workflowInstanceId, cancellationToken);
                if (workflowSummary == null) throw new FulcrumNotFoundException(workflowInstanceId);

                var activityInstanceService = _stateCapability.ActivityInstance;
                FulcrumAssert.IsNotNull(activityInstanceService, CodeLocation.AsString());

                foreach (var activityInstance in workflowSummary.ActivityInstances.Values)
                {
                    if (activityInstance.State == ActivityStateEnum.Failed)
                    {
                        var activityVersion = workflowSummary.ActivityVersions.Values.FirstOrDefault(x => x.Id == activityInstance.ActivityVersionId);
                        FulcrumAssert.IsNotNull( activityVersion , CodeLocation.AsString());

                        var activityFailUrgency = activityVersion.FailUrgency;
                        if (activityFailUrgency != ActivityFailUrgencyEnum.Stopping)
                        {
                            continue;
                        }

                        activityInstance.Reset();
                        await activityInstanceService.UpdateAndReturnAsync(activityInstance.Id, activityInstance, cancellationToken);
                    }
                }

                workflowInstance.State = WorkflowStateEnum.Waiting;
                await workflowInstanceService.UpdateAsync(workflowInstanceId, workflowInstance, cancellationToken);
            }
            finally
            {
                await workflowInstanceService.ReleaseDistributedLockAsync(
                    workflowInstanceId, workflowDistributedLock.LockId, cancellationToken);
            }

            await _requestMgmtCapability.Request.RetryAsync(workflowInstanceId, cancellationToken);
        }
    }
}