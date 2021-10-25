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

namespace Nexus.Link.WorkflowEngine.Sdk.Model
{
    public class ActivityInformation
    {
        public IWorkflowCapability WorkflowCapability { get; }
        public WorkflowInformation WorkflowInformation { get; }

        public Activity Activity { get; }
        private readonly Activity _storedActivity;

        public MethodHandler MethodHandler { get; }
        public string PreviousActivityId { get; }
        public string NestedPosition { get; }
        public string NestedPositionAndTitle => $"{NestedPosition} {Activity?.Form?.Title}";

        public ActivityTypeEnum? ActivityType => Activity?.Form?.Type;

        public bool HasCompleted =>
            Activity?.Instance?.State == ActivityStateEnum.Success || Activity?.Instance?.State == ActivityStateEnum.Failed;

        public ActivityInformation(WorkflowInformation workflowInformation,
            MethodHandler methodHandler, string formTitle, int position, string activityFormId,
            ActivityTypeEnum activityType)
        {
            InternalContract.RequireNotNull(workflowInformation, nameof(workflowInformation));
            InternalContract.RequireNotNullOrWhiteSpace(activityFormId, nameof(activityFormId));

            WorkflowInformation = workflowInformation!;
            WorkflowCapability = workflowInformation.WorkflowCapability;
            MethodHandler = methodHandler;

            PreviousActivityId = AsyncWorkflowStatic.Context.LatestActivityInstanceId;

            var parentActivityInstanceId = AsyncWorkflowStatic.Context.ParentActivityInstanceId;
            int? parentIteration = null;
            ActivityInformation parentActivityInformation = null;
            if (parentActivityInstanceId != null)
            {
                var parentActivity = WorkflowInformation.GetActivity(parentActivityInstanceId);
                parentActivityInformation = WorkflowInformation?.GetActivityInformation(parentActivityInstanceId);
                FulcrumAssert.IsNotNull(parentActivity, CodeLocation.AsString());
                parentIteration = parentActivity?.Iteration;
            }

            var activity =
                WorkflowInformation.StoredWorkflow?.WorkflowHierarchy?.Activities?.FirstOrDefault(a =>
                    a.Form.Id == activityFormId);
            activity ??= new Activity();
            activity.Form ??= new ActivityForm
            {
                Id = activityFormId,
                WorkflowFormId = WorkflowInformation.FormId,
                Title = formTitle,
                Type = activityType
            };
            activity.Version ??= new ActivityVersion
            {
                WorkflowVersionId = WorkflowInformation.VersionId,
                ActivityFormId = activityFormId,
                ParentActivityVersionId = parentActivityInformation?.Activity?.Version.Id,
                Position = position
            };
            activity.Instance ??= new ActivityInstance
            {
                WorkflowInstanceId = WorkflowInformation.InstanceId,
                ParentActivityInstanceId = parentActivityInstanceId,
                ActivityVersionId = null,
                ParentIteration = parentIteration,
                FailUrgency = ActivityFailUrgencyEnum.Stopping,
                State = ActivityStateEnum.Started,
                StartedAt = DateTimeOffset.UtcNow
            };
            Activity = activity;
            _storedActivity = Activity.AsCopy();
        }

        private bool SameSerialization(object a, object b)
        {
            if (a == null || b == null) return (a == null && b == null);
            return JToken.FromObject(a).Equals(JToken.FromObject(b));
        }

        /// <inheritdoc />
        public override string ToString() => $"{WorkflowInformation.VersionTitle}: {ActivityType} {NestedPositionAndTitle} ({Activity?.Form?.Id})";

        protected internal async Task PersistAsync(CancellationToken cancellationToken)
        {
            await PersistActivityFormAsync(cancellationToken);
            await PersistActivityVersion(cancellationToken);
            await PersistActivityInstance(cancellationToken);
            await PersistTransitionAsync(cancellationToken);
            WorkflowInformation.LatestActivityInstanceId = Activity.Instance.Id;
            await MethodHandler.PersistActivityParametersAsync(WorkflowCapability, Activity.Version.Id, cancellationToken);
        }

        private async Task PersistActivityFormAsync(CancellationToken cancellationToken)
        {
            if (Activity.Form.Etag == null)
            {
                try
                {
                    Activity.Form.Etag = "ignore";
                    await WorkflowCapability.ActivityForm.CreateWithSpecifiedIdAsync(Activity.Form.Id, Activity.Form, cancellationToken);
                }
                catch (FulcrumConflictException)
                {
                    // Conflict due to other thread created first.
                }
            }
            else
            {
                if (SameSerialization(_storedActivity.Form, Activity.Form)) return;
                // TODO: Error if tried to change ActivityType?
                try
                {
                    await WorkflowCapability.ActivityForm.UpdateAsync(Activity.Form.Id, Activity.Form, cancellationToken);
                }
                catch (FulcrumConflictException)
                {
                    // This is OK. Another thread has update the same Id after we did the read above.
                }
            }

            var form = await WorkflowCapability.ActivityForm.ReadAsync(Activity.Form.Id, cancellationToken);
            FulcrumAssert.IsNotNull(form, CodeLocation.AsString());
            Activity.Form = form;
            _storedActivity.Form = Activity.Form.AsCopy();
        }

        private async Task PersistActivityVersion(CancellationToken cancellationToken)
        {
            if (Activity.Version.Etag == null)
            {
                try
                {
                    Activity.Version.Id = "ignore";
                    Activity.Version.Etag = "ignore";
                    await WorkflowCapability.ActivityVersion.CreateAsync(Activity.Version, cancellationToken);
                }
                catch (FulcrumConflictException)
                {
                    // This is OK. Another thread has created the same Id after we did the read above.
                }
            }
            else
            {
                if (SameSerialization(_storedActivity.Version, Activity.Version)) return;
                {
                    try
                    {
                        await WorkflowCapability.ActivityVersion.UpdateAsync(Activity.Version.Id, Activity.Version,
                            cancellationToken);
                    }
                    catch (FulcrumConflictException)
                    {
                        // This is OK. Another thread has update the same Id after we did the read above.
                    }
                }
            }
            var version = await WorkflowCapability.ActivityVersion.FindUniqueAsync(WorkflowInformation.VersionId, Activity.Form.Id, cancellationToken);
            FulcrumAssert.IsNotNull(version, CodeLocation.AsString());
            Activity.Version = version;
            Activity.Instance.ActivityVersionId = version.Id;
            _storedActivity.Version = Activity.Version.AsCopy();
        }

        private async Task PersistActivityInstance(CancellationToken cancellationToken)
        {
            if (Activity.Instance.Etag == null)
            {
                try
                {
                    Activity.Instance.Id = "ignore";
                    Activity.Instance.Etag = "ignore";
                    var activityInstanceId = await WorkflowCapability.ActivityInstance.CreateAsync(Activity.Instance, cancellationToken);
                }
                catch (FulcrumConflictException)
                {
                    // This is OK. Another thread has created the same Id after we did the read above.
                }
            }
            else
            {
                if (SameSerialization(_storedActivity.Instance, Activity.Instance)) return;
                {
                    try
                    {
                        await WorkflowCapability.ActivityInstance.UpdateAsync(Activity.Instance.Id, Activity.Instance,
                            cancellationToken);
                    }
                    catch (FulcrumConflictException)
                    {
                        // This is OK. Another thread has update the same Id after we did the read above.
                    }
                }
            }
            var findUnique = new ActivityInstanceUnique
            {
                WorkflowInstanceId = WorkflowInformation.InstanceId,
                ActivityVersionId = Activity.Instance.ActivityVersionId,
                ParentActivityInstanceId = Activity.Instance.ParentActivityInstanceId,
                ParentIteration = Activity.Instance.ParentIteration
            };

            var activityInstance = await WorkflowCapability.ActivityInstance.FindUniqueAsync(findUnique, cancellationToken);
            FulcrumAssert.IsNotNull(activityInstance, CodeLocation.AsString());
            Activity.Instance = activityInstance;
            _storedActivity.Instance = Activity.Instance.AsCopy();
        }

        private async Task PersistTransitionAsync(CancellationToken cancellationToken)
        {
            var previousActivityInformation = WorkflowInformation.GetActivityInformation(PreviousActivityId);
            var searchItem = new TransitionUnique
            {
                WorkflowVersionId = WorkflowInformation.VersionId,
                FromActivityVersionId = previousActivityInformation?.Activity.Version.Id,
                ToActivityVersionId = Activity.Version.Id
            };
            var transition = await WorkflowCapability.Transition.FindUniqueAsync(WorkflowInformation.VersionId, searchItem, cancellationToken);
            if (transition == null)
            {
                var createItem = new TransitionCreate
                {
                    WorkflowVersionId = WorkflowInformation.VersionId,
                    FromActivityVersionId = previousActivityInformation?.Activity.Version.Id,
                    ToActivityVersionId = Activity.Version.Id
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
    }
}