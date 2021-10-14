using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Json;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.WorkflowEngine.Sdk.MethodSupport;

namespace Nexus.Link.WorkflowEngine.Sdk.Model
{
    public enum WorkflowActivityTypeEnum
    {
        Action, Condition, LoopUntilTrue,
        ForEachParallel
    }

    public class ActivityInformation
    {
        public IWorkflowCapability WorkflowCapability { get; }
        private readonly WorkflowInformation _workflowInformation;
        public MethodHandler MethodHandler { get; }
        public WorkflowActivityTypeEnum ActivityType { get; }
        public ActivityInformation PreviousActivity { get; }
        public ActivityInformation ParentActivity { get; }
        public int? Iteration { get; set; }
        public string FormId { get; set; }
        public string VersionId { get; set; }
        public string InstanceId { get; set; }
        public int Position { get; set; }
        public string FormTitle { get; set; }
        public string NestedPosition { get; set; }
        public string NestedPositionAndTitle => $"{NestedPosition} {FormTitle}";
        public ActivityResult Result { get; set; } = new();
        public string AsyncRequestId { get; set; }

        public ActivityInformation(IWorkflowCapability workflowCapability,
            WorkflowInformation workflowInformation, MethodHandler methodHandler, int position,
            WorkflowActivityTypeEnum activityType,
            ActivityInformation previousActivity, ActivityInformation parentActivity)
        {
            WorkflowCapability = workflowCapability;
            _workflowInformation = workflowInformation;
            MethodHandler = methodHandler;
            ActivityType = activityType;
            PreviousActivity = previousActivity;
            Position = position;
            ParentActivity = parentActivity;
            NestedPosition = parentActivity == null ? Position.ToString() : $"{parentActivity.NestedPosition}.{Position}";
        }

        public bool HasCompleted;

        /// <inheritdoc />
        public override string ToString() => $"{_workflowInformation.VersionTitle}: {ActivityType} {NestedPositionAndTitle} ({FormId})";

        protected internal async Task PersistAsync(CancellationToken cancellationToken)
        {
            await PersistActivityFormAsync(cancellationToken);
            VersionId = await PersistActivityVersion(cancellationToken);
            await PersistTransitionAsync(cancellationToken);
            InstanceId = await PersistActivityInstance(cancellationToken);
            await MethodHandler.PersistActivityParametersAsync(WorkflowCapability, VersionId, cancellationToken);
        }

        private async Task PersistTransitionAsync(CancellationToken cancellationToken)
        {
            var searchItem = new TransitionUnique
            {
                WorkflowVersionId = _workflowInformation.VersionId,
                FromActivityVersionId = PreviousActivity?.VersionId,
                ToActivityVersionId = VersionId
            };
            var transition = await WorkflowCapability.Transition.FindUniqueAsync(searchItem, cancellationToken);
            if (transition == null)
            {
                var createItem = new TransitionCreate
                {
                    WorkflowVersionId = _workflowInformation.VersionId,
                    FromActivityVersionId = PreviousActivity?.VersionId,
                    ToActivityVersionId = VersionId
                };
                try
                {
                    await WorkflowCapability.Transition.CreateChildAsync(_workflowInformation.VersionId, createItem, cancellationToken);
                }
                catch (FulcrumConflictException)
                {
                    // This is OK. Most likely another thread has created the same item after we did the uniqueness test above.
                }
            }
        }

        private async Task<string> PersistActivityInstance(CancellationToken cancellationToken)
        {
            var findUnique = new ActivityInstanceUnique
            {
                WorkflowInstanceId = _workflowInformation.InstanceId,
                ActivityVersionId = VersionId,
                ParentActivityInstanceId = ParentActivity?.InstanceId,
                ParentIteration = ParentActivity?.Iteration
            };
            var activityInstance = await WorkflowCapability.ActivityInstance.FindUniqueAsync(findUnique, cancellationToken);
            if (activityInstance == null)
            {
                var createItem = new ActivityInstanceCreate()
                {
                    WorkflowInstanceId = findUnique.WorkflowInstanceId,
                    ActivityVersionId = findUnique.ActivityVersionId,
                    ParentActivityInstanceId = findUnique.ParentActivityInstanceId,
                    ParentIteration = findUnique.ParentIteration
                };
                try
                {
                    var id = await WorkflowCapability.ActivityInstance.CreateAsync(createItem, cancellationToken);
                    return id;
                }
                catch (FulcrumConflictException)
                {
                    // This is OK. Another thread has created the same Id after we did the read above.
                    activityInstance = await WorkflowCapability.ActivityInstance.FindUniqueAsync(findUnique, cancellationToken);
                    FulcrumAssert.IsNotNull(activityInstance, CodeLocation.AsString());
                    return activityInstance.Id;
                }
            }

            Result.ExceptionName = activityInstance.ExceptionType;
            Result.ExceptionMessage = activityInstance.ExceptionMessage;
            Result.Json = activityInstance.ResultAsJson;
            HasCompleted = activityInstance.HasCompleted;
            AsyncRequestId = activityInstance.AsyncRequestId;

            return activityInstance.Id;
        }

        private async Task<string> PersistActivityVersion(CancellationToken cancellationToken)
        {
            var workflowVersionId = _workflowInformation.VersionId;
            var activityVersion =
                await WorkflowCapability.ActivityVersion.FindUniqueByWorkflowVersionActivityAsync(workflowVersionId, FormId,
                    cancellationToken);
            if (activityVersion == null)
            {
                var createItem = new ActivityVersionCreate()
                {
                    WorkflowVersionId = workflowVersionId,
                    ActivityFormId = FormId,
                    ParentActivityVersionId = ParentActivity?.VersionId,
                    Position = Position
                };
                try
                {
                    var id = await WorkflowCapability.ActivityVersion.CreateChildAsync(workflowVersionId, createItem, cancellationToken);
                    return id;
                }
                catch (FulcrumConflictException)
                {
                    // This is OK. Another thread has created the same Id after we did the read above.
                    activityVersion =
                        await WorkflowCapability.ActivityVersion.FindUniqueByWorkflowVersionActivityAsync(workflowVersionId, FormId,
                            cancellationToken);
                    return activityVersion.Id;
                }
            }

            // TODO: Error if tried to change ActivityType?
            if (activityVersion.Position != Position)
            {
                activityVersion.Position = Position;
                try
                {
                    await WorkflowCapability.ActivityVersion.UpdateAsync(activityVersion.Id, activityVersion, cancellationToken);
                }
                catch (FulcrumConflictException)
                {
                    // This is OK. Another thread has update the same Id after we did the read above.
                }
            }

            return activityVersion.Id;
        }

        private async Task PersistActivityFormAsync(CancellationToken cancellationToken)
        {
            var activityForm = await WorkflowCapability.ActivityForm.ReadAsync(FormId, cancellationToken);
            if (activityForm == null)
            {
                var workflowFormId = _workflowInformation.FormId;
                var createItem = new ActivityFormCreate()
                {
                    WorkflowFormId = workflowFormId,
                    Type = ActivityType.ToString(),
                    Title = FormTitle
                };
                try
                {
                    await WorkflowCapability.ActivityForm.CreateChildWithSpecifiedIdAsync(workflowFormId, FormId, createItem,
                        cancellationToken);
                }
                catch (FulcrumConflictException)
                {
                    // Conflict due to other thread created first.
                }
            }
            else
            {
                // TODO: Error if tried to change ActivityType?
                if (activityForm.Title != FormTitle)
                {
                    activityForm.Title = FormTitle;
                    try
                    {
                        await WorkflowCapability.ActivityForm.UpdateAsync(FormId, activityForm, cancellationToken);
                    }
                    catch (FulcrumConflictException)
                    {
                        // This is OK. Another thread has update the same Id after we did the read above.
                    }
                }
            }
        }

        public async Task UpdateInstanceWithResultAsync(CancellationToken cancellationToken)
        {
            while (true)
            {
                HasCompleted = true;
                var item = await WorkflowCapability.ActivityInstance.ReadAsync(InstanceId, cancellationToken);
                item.ResultAsJson = Result.Json;
                item.ExceptionType = Result.ExceptionName;
                item.ExceptionMessage = Result.ExceptionMessage;
                item.HasCompleted = HasCompleted;
                item.FinishedAt = DateTimeOffset.UtcNow; try
                {
                    await WorkflowCapability.ActivityInstance.UpdateAsync(InstanceId, item, cancellationToken);
                    return;
                }
                catch (FulcrumConflictException)
                {
                    // This is OK. Another thread is competing with us on this resource. We will try again.
                }
            }
        }

        public async Task UpdateInstanceWithRequestIdAsync(CancellationToken cancellationToken)
        {
            while (true)
            {
                var item = await WorkflowCapability.ActivityInstance.ReadAsync(InstanceId, cancellationToken);
                if (item.AsyncRequestId != AsyncRequestId)
                {
                    item.AsyncRequestId = AsyncRequestId;
                    try
                    {
                        await WorkflowCapability.ActivityInstance.UpdateAsync(InstanceId, item, cancellationToken);
                        return;
                    }
                    catch (FulcrumConflictException)
                    {
                        // This is OK. Another thread is competing with us on this resource. We will try again.
                    }
                }
            }
        }
    }
}