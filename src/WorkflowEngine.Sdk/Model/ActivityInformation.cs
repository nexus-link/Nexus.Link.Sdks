using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.WorkflowEngine.Sdk.MethodSupport;

namespace Nexus.Link.WorkflowEngine.Sdk.Model
{
    public enum WorkflowActivityTypeEnum
    {
        Action, Condition, LoopUntilTrue,
        ForEachParallel,
        ForEachSequential
    }

    public class ActivityInformation
    {
        public IWorkflowCapability WorkflowCapability { get; }
        public WorkflowInformation WorkflowInformation { get; }
        public MethodHandler MethodHandler { get; }
        public WorkflowActivityTypeEnum ActivityType { get; }
        public ActivityInformation PreviousActivity { get; }
        public ActivityInformation ParentActivity { get; }
        public int? ParentIteration { get; set; }
        public string FormId { get; set; }
        public string VersionId { get; set; }
        public string InstanceId { get; set; }
        public int Position { get; set; }
        public string FormTitle { get; set; }
        public string NestedPosition { get; set; }
        public string NestedPositionAndTitle => $"{NestedPosition} {FormTitle}";
        public ActivityResult Result { get; set; } = new();
        public string AsyncRequestId { get; set; }
        public ActivityStateEnum State { get; set; }
        public ActivityFailUrgencyEnum FailUrgency { get; set; }

        public bool HasCompleted =>
            State == ActivityStateEnum.Success || State == ActivityStateEnum.Failed;

        public ActivityInformation(WorkflowInformation workflowInformation,
            MethodHandler methodHandler, int position, WorkflowActivityTypeEnum activityType,
            ActivityInformation previousActivity,
            ActivityInformation parentActivity, int? parentIteration)
        {
            WorkflowInformation = workflowInformation;
            WorkflowCapability = workflowInformation.WorkflowCapability;
            MethodHandler = methodHandler;
            ActivityType = activityType;
            PreviousActivity = previousActivity;
            Position = position;
            ParentActivity = parentActivity;
            ParentIteration = parentIteration;
            NestedPosition = parentActivity == null ? Position.ToString() : $"{parentActivity.NestedPosition}.{Position}";
            State = ActivityStateEnum.Started;
            FailUrgency = ActivityFailUrgencyEnum.Stopping;
        }

        /// <inheritdoc />
        public override string ToString() => $"{WorkflowInformation.VersionTitle}: {ActivityType} {NestedPositionAndTitle} ({FormId})";

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
                WorkflowVersionId = WorkflowInformation.VersionId,
                FromActivityVersionId = PreviousActivity?.VersionId,
                ToActivityVersionId = VersionId
            };
            var transition = await WorkflowCapability.Transition.FindUniqueAsync(WorkflowInformation.VersionId, searchItem, cancellationToken);
            if (transition == null)
            {
                var createItem = new TransitionCreate
                {
                    WorkflowVersionId = WorkflowInformation.VersionId,
                    FromActivityVersionId = PreviousActivity?.VersionId,
                    ToActivityVersionId = VersionId
                };
                try
                {
                    await WorkflowCapability.Transition.CreateChildAsync(WorkflowInformation.VersionId, createItem, cancellationToken);
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
                WorkflowInstanceId = WorkflowInformation.InstanceId,
                ActivityVersionId = VersionId,
                ParentActivityInstanceId = ParentActivity?.InstanceId,
                ParentIteration = ParentIteration
            };
            if (ParentActivity != null)
            {
                switch (ParentActivity.ActivityType)
                {
                    case WorkflowActivityTypeEnum.Action:
                    case WorkflowActivityTypeEnum.Condition:
                        break;
                    case WorkflowActivityTypeEnum.LoopUntilTrue:
                    case WorkflowActivityTypeEnum.ForEachParallel:
                    case WorkflowActivityTypeEnum.ForEachSequential:
                        FulcrumAssert.IsNotNull(ParentIteration, CodeLocation.AsString());
                        break;
                    default:
                        FulcrumAssert.Fail(CodeLocation.AsString(), $"Unknown activity type: {ParentActivity.ActivityType}.");
                        throw new ArgumentOutOfRangeException();
                }
            }

            var activityInstance = await WorkflowCapability.ActivityInstance.FindUniqueAsync(findUnique, cancellationToken);
            if (activityInstance == null)
            {
                var createItem = new ActivityInstanceCreate()
                {
                    State = State,
                    FailUrgency = FailUrgency,
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
            
            State = activityInstance.State;
            
            Result.ExceptionAlertHandled = activityInstance.ExceptionAlertHandled;
            FailUrgency = activityInstance.FailUrgency;
            Result.ExceptionCategory = activityInstance.ExceptionCategory;
            Result.ExceptionFriendlyMessage = activityInstance.ExceptionFriendlyMessage;
            Result.ExceptionTechnicalMessage = activityInstance.ExceptionTechnicalMessage;
            Result.Json = activityInstance.ResultAsJson;
            AsyncRequestId = activityInstance.AsyncRequestId;

            return activityInstance.Id;
        }

        private async Task<string> PersistActivityVersion(CancellationToken cancellationToken)
        {
            var workflowVersionId = WorkflowInformation.VersionId;
            var activityVersion =
                await WorkflowCapability.ActivityVersion.FindUniqueAsync(workflowVersionId, FormId, cancellationToken);
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
                    var id = await WorkflowCapability.ActivityVersion.CreateAsync(createItem, cancellationToken);
                    return id;
                }
                catch (FulcrumConflictException)
                {
                    // This is OK. Another thread has created the same Id after we did the read above.
                    activityVersion = await WorkflowCapability.ActivityVersion.FindUniqueAsync(workflowVersionId, FormId, cancellationToken);
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
                var workflowFormId = WorkflowInformation.FormId;
                var createItem = new ActivityFormCreate()
                {
                    WorkflowFormId = workflowFormId,
                    Type = ActivityType.ToString(),
                    Title = FormTitle
                };
                try
                {
                    await WorkflowCapability.ActivityForm.CreateWithSpecifiedIdAsync(FormId, createItem, cancellationToken);
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
                var item = await WorkflowCapability.ActivityInstance.ReadAsync(InstanceId, cancellationToken);
                item.State = State;
                item.FailUrgency = FailUrgency;
                item.AsyncRequestId = AsyncRequestId;
                item.ResultAsJson = Result.Json;
                item.ExceptionCategory = Result.ExceptionCategory;
                item.ExceptionFriendlyMessage = Result.ExceptionFriendlyMessage;
                item.ExceptionTechnicalMessage = Result.ExceptionTechnicalMessage;
                item.FinishedAt = DateTimeOffset.UtcNow;
                item.ExceptionAlertHandled = Result.ExceptionAlertHandled;
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