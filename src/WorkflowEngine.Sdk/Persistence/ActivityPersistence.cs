using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities.Runtime;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Support;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Json;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.WorkflowEngine.Sdk.MethodSupport;
using Nexus.Link.WorkflowEngine.Sdk.Support;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence
{
    public class ActivityPersistence
    {
        public IWorkflowCapability WorkflowCapability { get; }
        public WorkflowPersistence WorkflowPersistence { get; }

        public ActivitySummary ActivitySummary { get; }
        private readonly ActivitySummary _storedActivitySummary;

        public MethodHandler MethodHandler { get; }
        public string PreviousActivityId { get; }
        public string NestedPosition { get; }
        public string NestedPositionAndTitle => $"{NestedPosition} {ActivitySummary?.Form?.Title}";

        public ActivityTypeEnum? ActivityType => ActivitySummary?.Form?.Type;

        public bool HasCompleted =>
            ActivitySummary?.Instance?.State == ActivityStateEnum.Success || ActivitySummary?.Instance?.State == ActivityStateEnum.Failed;

        public ActivityPersistence(WorkflowPersistence workflowPersistence,
            MethodHandler methodHandler, string formTitle, int position, string activityFormId,
            ActivityTypeEnum activityType)
        {
            InternalContract.RequireNotNull(workflowPersistence, nameof(workflowPersistence));
            InternalContract.RequireNotNullOrWhiteSpace(activityFormId, nameof(activityFormId));

            WorkflowPersistence = workflowPersistence!;
            WorkflowCapability = workflowPersistence.WorkflowCapability;
            MethodHandler = methodHandler;

            PreviousActivityId = AsyncWorkflowStatic.Context.LatestActivityInstanceId;

            var parentActivityInstanceId = AsyncWorkflowStatic.Context.ParentActivityInstanceId;
            int? parentIteration = null;
            ActivityPersistence parentActivityPersistence = null;
            if (parentActivityInstanceId != null)
            {
                var parentActivity = WorkflowPersistence.GetActivity(parentActivityInstanceId);
                parentActivityPersistence = WorkflowPersistence?.GetActivityInformation(parentActivityInstanceId);
                FulcrumAssert.IsNotNull(parentActivity, CodeLocation.AsString());
                parentIteration = parentActivity?.Iteration;
            }

            var activitySummary =
                WorkflowPersistence.StoredWorkflow?.WorkflowSummary?.ReferredActivities?.FirstOrDefault(a =>
                    a.Form.Id.ToLowerInvariant() == activityFormId.ToLowerInvariant()
                    && a.Instance.ParentActivityInstanceId == parentActivityInstanceId
                    && a.Instance.ParentIteration == parentIteration);
            if (activitySummary == null)
            {
                activitySummary ??=
                    WorkflowPersistence.StoredWorkflow?.WorkflowSummary?.NotReferredActivities?.FirstOrDefault(a =>
                            a.Form.Id.ToLowerInvariant() == activityFormId.ToLowerInvariant())
                        .AsCopy(); // We need a copy, because this activity can be used for several different instances, e.g. in a loop
            }

            activitySummary ??= new ActivitySummary();
            activitySummary.Form ??= new ActivityForm
            {
                Id = activityFormId,
                WorkflowFormId = WorkflowPersistence.FormId,
                Title = formTitle,
                Type = activityType
            };
            activitySummary.Version ??= new ActivityVersion
            {
                WorkflowVersionId = WorkflowPersistence.VersionId,
                ActivityFormId = activityFormId,
                ParentActivityVersionId = parentActivityPersistence?.ActivitySummary?.Version.Id,
                FailUrgency = ActivityFailUrgencyEnum.Stopping,
                Position = position
            };
            activitySummary.Instance ??= new ActivityInstance
            {
                WorkflowInstanceId = WorkflowPersistence.InstanceId,
                ParentActivityInstanceId = parentActivityInstanceId,
                ActivityVersionId = activitySummary.Version.Id,
                ParentIteration = parentIteration,
                State = ActivityStateEnum.Started,
                StartedAt = DateTimeOffset.UtcNow
            };
            ActivitySummary = activitySummary;
            _storedActivitySummary = ActivitySummary.AsCopy();
        }

        private bool SameSerialization(object a, object b)
        {
            if (a == null || b == null) return (a == null && b == null);
            return JToken.DeepEquals(JToken.FromObject(a), JToken.FromObject(b));
        }

        /// <inheritdoc />
        public override string ToString() => $"{WorkflowPersistence.VersionTitle}: {ActivityType} {NestedPositionAndTitle} ({ActivitySummary?.Form?.Id})";

        protected internal async Task PersistAsync(CancellationToken cancellationToken)
        {
                var hasCompleted = _storedActivitySummary.Instance.HasCompleted;
                if (!hasCompleted)
                {
                    await PersistActivityFormAsync(cancellationToken);
                    await PersistActivityVersion(cancellationToken);
                    await PersistActivityInstance(cancellationToken);
                    await PersistTransitionAsync(cancellationToken);
                }
                WorkflowPersistence.LatestActivityInstanceId = ActivitySummary.Instance.Id;
                if (!hasCompleted)
                {
                    await MethodHandler.PersistActivityParametersAsync(
                        WorkflowCapability, ActivitySummary.Version.Id,
                        cancellationToken);
                }
        }

        private async Task PersistActivityFormAsync(CancellationToken cancellationToken)
        {
            var form = ActivitySummary.Form;
            if (form.Etag == null)
            {
                try
                {
                    form.Etag = "ignore";
                    await WorkflowCapability.ActivityForm.CreateWithSpecifiedIdAsync(form.Id,
                        form, cancellationToken);
                }
                catch (FulcrumConflictException)
                {
                    // Conflict due to other thread created first.
                }
            }
            else
            {
                if (SameSerialization(_storedActivitySummary.Form, form)) return;
                // TODO: Error if tried to change ActivityType?
                try
                {
                    await WorkflowCapability.ActivityForm.UpdateAsync(form.Id, form,
                        cancellationToken);
                }
                catch (FulcrumConflictException)
                {
                    // This is OK. Another thread has update the same Id after we did the read above.
                }
            }

            var latestForm = await WorkflowCapability.ActivityForm.ReadAsync(form.Id, cancellationToken);
            FulcrumAssert.IsNotNull(latestForm, CodeLocation.AsString());
            ActivitySummary.Form = latestForm;
            _storedActivitySummary.Form = latestForm.AsCopy();
        }

        private async Task PersistActivityVersion(CancellationToken cancellationToken)
        {
            var version = ActivitySummary.Version;
            if (version.Etag == null)
            {
                try
                {
                    version.Id = "ignore";
                    version.Etag = "ignore";
                    await WorkflowCapability.ActivityVersion.CreateAsync(version, cancellationToken);
                }
                catch (FulcrumConflictException)
                {
                    // This is OK. Another thread has created the same Id after we did the read above.
                }
            }
            else
            {
                if (SameSerialization(_storedActivitySummary.Version, version)) return;
                try
                {
                    await WorkflowCapability.ActivityVersion.UpdateAsync(version.Id, version,
                        cancellationToken);
                }
                catch (FulcrumConflictException)
                {
                    // This is OK. Another thread has update the same Id after we did the read above.
                }
            }
            var latestVersion = await WorkflowCapability.ActivityVersion.FindUniqueAsync(WorkflowPersistence.VersionId, ActivitySummary.Form.Id, cancellationToken);
            FulcrumAssert.IsNotNull(latestVersion, CodeLocation.AsString());
            ActivitySummary.Version = latestVersion;
            ActivitySummary.Instance.ActivityVersionId = latestVersion.Id;
            _storedActivitySummary.Version = latestVersion.AsCopy();
        }

        private async Task PersistActivityInstance(CancellationToken cancellationToken)
        {
            var instance = ActivitySummary.Instance;
            ActivityInstance updatedActivityInstance = null;
            if (instance.Id == null)
            {
                try
                {
                    instance.Id = "ignore";
                    instance.Etag = "ignore";
                    updatedActivityInstance = await WorkflowCapability.ActivityInstance.CreateAndReturnAsync(instance, cancellationToken);
                }
                catch (FulcrumConflictException)
                {
                    // This is OK. Another thread has created the same Id after we did the read above.
                }
            }
            else
            {
                if (SameSerialization(_storedActivitySummary.Instance, instance)) return;
                FulcrumAssert.IsTrue(!_storedActivitySummary.Instance.HasCompleted, CodeLocation.AsString());
                try
                {
                    updatedActivityInstance = await WorkflowCapability.ActivityInstance.UpdateAndReturnAsync(instance.Id, instance,
                        cancellationToken);
                }
                catch (FulcrumConflictException)
                {
                    // This is OK. Another thread has update the same Id after we did the read above.
                }
            }

            if (updatedActivityInstance == null)
            {
                var findUnique = new ActivityInstanceUnique
                {
                    WorkflowInstanceId = instance.WorkflowInstanceId,
                    ActivityVersionId = instance.ActivityVersionId,
                    ParentActivityInstanceId = instance.ParentActivityInstanceId,
                    ParentIteration = instance.ParentIteration
                };

                updatedActivityInstance =
                    await WorkflowCapability.ActivityInstance.FindUniqueAsync(findUnique, cancellationToken);
                FulcrumAssert.IsNotNull(updatedActivityInstance, CodeLocation.AsString());
            }

            ActivitySummary.Instance = updatedActivityInstance;
            _storedActivitySummary.Instance = updatedActivityInstance.AsCopy();
        }

        private async Task PersistTransitionAsync(CancellationToken cancellationToken)
        {
            var previousActivityInformation = WorkflowPersistence.GetActivityInformation(PreviousActivityId);
            var searchItem = new TransitionUnique
            {
                WorkflowVersionId = WorkflowPersistence.VersionId,
                FromActivityVersionId = previousActivityInformation?.ActivitySummary.Version.Id,
                ToActivityVersionId = ActivitySummary.Version.Id
            };
            var transition = await WorkflowCapability.Transition.FindUniqueAsync(WorkflowPersistence.VersionId, searchItem, cancellationToken);
            if (transition == null)
            {
                var createItem = new TransitionCreate
                {
                    WorkflowVersionId = WorkflowPersistence.VersionId,
                    FromActivityVersionId = previousActivityInformation?.ActivitySummary.Version.Id,
                    ToActivityVersionId = ActivitySummary.Version.Id
                };
                try
                {
                    await WorkflowCapability.Transition.CreateChildAsync(WorkflowPersistence.VersionId, createItem, cancellationToken);
                }
                catch (FulcrumConflictException)
                {
                    // This is OK. Most likely another thread has created the same item after we did the uniqueness test above.
                }
            }
        }
    }
}