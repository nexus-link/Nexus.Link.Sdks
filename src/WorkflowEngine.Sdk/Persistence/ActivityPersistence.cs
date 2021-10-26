﻿using System;
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

        public Activity Activity { get; }
        private readonly Activity _storedActivity;

        public MethodHandler MethodHandler { get; }
        public string PreviousActivityId { get; }
        public string NestedPosition { get; }
        public string NestedPositionAndTitle => $"{NestedPosition} {Activity?.Form?.Title}";

        public ActivityTypeEnum? ActivityType => Activity?.Form?.Type;

        public bool HasCompleted =>
            Activity?.Instance?.State == ActivityStateEnum.Success || Activity?.Instance?.State == ActivityStateEnum.Failed;

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

            var activity =
                WorkflowPersistence.StoredWorkflow?.WorkflowHierarchy?.Activities?.FirstOrDefault(a =>
                    a.Form.Id.ToLowerInvariant() == activityFormId.ToLowerInvariant()
                    && a.Instance.ParentActivityInstanceId == parentActivityInstanceId
                    && a.Instance.ParentIteration == parentIteration);
            activity ??=
                WorkflowPersistence.StoredWorkflow?.WorkflowHierarchy?.NotReferredActivities?.FirstOrDefault(a =>
                    a.Form.Id.ToLowerInvariant() == activityFormId.ToLowerInvariant())
                    .AsCopy(); // We need a copy, because this activity can be used for several different instances, e.g. in a loop
            activity ??= new Activity();
            activity.Form ??= new ActivityForm
            {
                Id = activityFormId,
                WorkflowFormId = WorkflowPersistence.FormId,
                Title = formTitle,
                Type = activityType
            };
            activity.Version ??= new ActivityVersion
            {
                WorkflowVersionId = WorkflowPersistence.VersionId,
                ActivityFormId = activityFormId,
                ParentActivityVersionId = parentActivityPersistence?.Activity?.Version.Id,
                FailUrgency = ActivityFailUrgencyEnum.Stopping,
                Position = position
            };
            activity.Instance ??= new ActivityInstance
            {
                WorkflowInstanceId = WorkflowPersistence.InstanceId,
                ParentActivityInstanceId = parentActivityInstanceId,
                ActivityVersionId = activity.Version.Id,
                ParentIteration = parentIteration,
                State = ActivityStateEnum.Started,
                StartedAt = DateTimeOffset.UtcNow
            };
            Activity = activity;
            _storedActivity = Activity.AsCopy();
        }

        private bool SameSerialization(object a, object b)
        {
            if (a == null || b == null) return (a == null && b == null);
            return JToken.DeepEquals(JToken.FromObject(a), JToken.FromObject(b));
        }

        /// <inheritdoc />
        public override string ToString() => $"{WorkflowPersistence.VersionTitle}: {ActivityType} {NestedPositionAndTitle} ({Activity?.Form?.Id})";

        protected internal async Task PersistAsync(CancellationToken cancellationToken)
        {
            var performance = new Performance(GetType().Name, $"{Activity.Form.Title}");
            await performance.MeasureMethodAsync(async () =>
            {
                await performance.MeasureAsync(() => PersistActivityFormAsync(cancellationToken));
                await performance.MeasureAsync(() => PersistActivityVersion(cancellationToken));
                await performance.MeasureAsync(() => PersistActivityInstance(cancellationToken));
                await performance.MeasureAsync(() => PersistTransitionAsync(cancellationToken));
                WorkflowPersistence.LatestActivityInstanceId = Activity.Instance.Id;
                await performance.MeasureAsync(() => MethodHandler.PersistActivityParametersAsync(WorkflowCapability, Activity.Version.Id,
                    cancellationToken));
            });
        }

        private async Task PersistActivityFormAsync(CancellationToken cancellationToken)
        {
            var form = Activity.Form;
            var p = new Performance(this.GetType().Name, $"{form.Title}");
            await p.MeasureMethodAsync(async () =>
            {
                if (form.Etag == null)
                {
                    try
                    {
                        form.Etag = "ignore";
                        await p.MeasureAsync(() => WorkflowCapability.ActivityForm.CreateWithSpecifiedIdAsync(form.Id,
                            form, cancellationToken));
                    }
                    catch (FulcrumConflictException)
                    {
                        // Conflict due to other thread created first.
                    }
                }
                else
                {
                    if (SameSerialization(_storedActivity.Form, form)) return;
                    // TODO: Error if tried to change ActivityType?
                    try
                    {
                        await p.MeasureAsync(() => WorkflowCapability.ActivityForm.UpdateAsync(form.Id, form,
                            cancellationToken));
                    }
                    catch (FulcrumConflictException)
                    {
                        // This is OK. Another thread has update the same Id after we did the read above.
                    }
                }

                var latestForm = await p.MeasureAsync(() => WorkflowCapability.ActivityForm.ReadAsync(form.Id, cancellationToken));
                FulcrumAssert.IsNotNull(latestForm, CodeLocation.AsString());
                Activity.Form = latestForm;
                _storedActivity.Form = latestForm.AsCopy();
            });
        }

        private async Task PersistActivityVersion(CancellationToken cancellationToken)
        {
            var version = Activity.Version;
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
                if (SameSerialization(_storedActivity.Version, version)) return;
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
            var latestVersion = await WorkflowCapability.ActivityVersion.FindUniqueAsync(WorkflowPersistence.VersionId, Activity.Form.Id, cancellationToken);
            FulcrumAssert.IsNotNull(latestVersion, CodeLocation.AsString());
            Activity.Version = latestVersion;
            Activity.Instance.ActivityVersionId = latestVersion.Id;
            _storedActivity.Version = version.AsCopy();
        }

        private async Task PersistActivityInstance(CancellationToken cancellationToken)
        {
            var p = new Performance(this.GetType().Name, Activity.Form.Title);
            await p.MeasureMethodAsync(async () =>
            {
                var instance = Activity.Instance;
                if (instance.Etag == null)
                {
                    try
                    {
                        instance.Id = "ignore";
                        instance.Etag = "ignore";
                        var activityInstanceId =
                            await p.MeasureAsync(() => WorkflowCapability.ActivityInstance.CreateAsync(instance, cancellationToken));
                    }
                    catch (FulcrumConflictException)
                    {
                        // This is OK. Another thread has created the same Id after we did the read above.
                    }
                }
                else
                {
                    if (SameSerialization(_storedActivity.Instance, instance)) return;
                    FulcrumAssert.IsTrue(!_storedActivity.Instance.HasCompleted, CodeLocation.AsString());
                    try
                    {
                        await p.MeasureAsync(() => WorkflowCapability.ActivityInstance.UpdateAsync(instance.Id, instance,
                            cancellationToken));
                    }
                    catch (FulcrumConflictException)
                    {
                        // This is OK. Another thread has update the same Id after we did the read above.
                    }
                }

                var findUnique = new ActivityInstanceUnique
                {
                    WorkflowInstanceId = instance.WorkflowInstanceId,
                    ActivityVersionId = instance.ActivityVersionId,
                    ParentActivityInstanceId = instance.ParentActivityInstanceId,
                    ParentIteration = instance.ParentIteration
                };

                var latestInstance =
                    await p.MeasureAsync(() => WorkflowCapability.ActivityInstance.FindUniqueAsync(findUnique, cancellationToken));
                FulcrumAssert.IsNotNull(latestInstance, CodeLocation.AsString());
                Activity.Instance = latestInstance;
                _storedActivity.Instance = instance.AsCopy();
            });
        }

        private async Task PersistTransitionAsync(CancellationToken cancellationToken)
        {
            var previousActivityInformation = WorkflowPersistence.GetActivityInformation(PreviousActivityId);
            var searchItem = new TransitionUnique
            {
                WorkflowVersionId = WorkflowPersistence.VersionId,
                FromActivityVersionId = previousActivityInformation?.Activity.Version.Id,
                ToActivityVersionId = Activity.Version.Id
            };
            var transition = await WorkflowCapability.Transition.FindUniqueAsync(WorkflowPersistence.VersionId, searchItem, cancellationToken);
            if (transition == null)
            {
                var createItem = new TransitionCreate
                {
                    WorkflowVersionId = WorkflowPersistence.VersionId,
                    FromActivityVersionId = previousActivityInformation?.Activity.Version.Id,
                    ToActivityVersionId = Activity.Version.Id
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