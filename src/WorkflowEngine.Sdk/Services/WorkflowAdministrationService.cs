using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract.Services;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities.Administration;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Crud.Helpers;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract;

namespace Nexus.Link.WorkflowEngine.Sdk.Services
{
    public class WorkflowAdministrationService : IWorkflowAdministrationService
    {
        private readonly IWorkflowService _workflowService;
        private readonly IRequestResponseService _requestResponseService;
        private readonly IRuntimeTables _runtimeTables;

        public WorkflowAdministrationService(IWorkflowService workflowService, IRequestResponseService requestResponseService, IRuntimeTables runtimeTables)
        {
            _workflowService = workflowService;
            _requestResponseService = requestResponseService;
            _runtimeTables = runtimeTables;
        }

        public async Task<Workflow> ReadAsync(string id, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNullOrWhiteSpace(id, nameof(id));

            var workflowRecord = await _workflowService.ReadAsync(id, cancellationToken);

            var workflow = new Workflow
            {
                Id = id,
                StartedAt = workflowRecord.Instance.StartedAt,
                FinishedAt = workflowRecord.Instance.FinishedAt,
                Title = $"{workflowRecord.Form.Title} {workflowRecord.Version.MajorVersion}.{workflowRecord.Version.MinorVersion}: {workflowRecord.Instance.Title}",
                Activities = await BuildActivityTreeAsync(null, workflowRecord.Activities)
            };

            // TODO: state

            return workflow;
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
                    // TODO: State = activityRecord.Instance.State
                    ErrorMessage = activityRecord.Instance.ExceptionMessage,
                    WaitingForWorkflow = await GetWaitingForWorkflowAsync(activityRecord)
                };
                activity.Children = await BuildActivityTreeAsync(activity, workflowRecordActivities);
                activities.Add(activity);
            }
            return activities;
        }

        private async Task<AnnotatedWorkflowId> GetWaitingForWorkflowAsync(Capabilities.WorkflowMgmt.Abstract.Entities.Runtime.Activity activityRecord)
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

            return waitingForWorkflow;
        }
    }
}