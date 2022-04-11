using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract;
using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract.Entities;
using Nexus.Link.Capabilities.WorkflowState.Abstract.Entities;
using Nexus.Link.Capabilities.WorkflowState.Abstract.Services;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Extensions.Configuration;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Extensions.State;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;

namespace Nexus.Link.WorkflowEngine.Sdk.Services.State
{

    public class WorkflowSummaryService : IWorkflowSummaryService
    {
        private readonly IConfigurationTables _configurationTables;
        private readonly IRuntimeTables _runtimeTables;
        private readonly IAsyncRequestMgmtCapability _asyncRequestMgmtCapability;

        public WorkflowSummaryService(IConfigurationTables configurationTables, IRuntimeTables runtimeTables,
            IAsyncRequestMgmtCapability asyncRequestMgmtCapability)
        {
            _configurationTables = configurationTables;
            _runtimeTables = runtimeTables;
            _asyncRequestMgmtCapability = asyncRequestMgmtCapability;
        }

        public async Task<WorkflowSummary> GetSummaryAsync(string instanceId,
            CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNullOrWhiteSpace(instanceId, nameof(instanceId));

            var instanceIdAsGuid = instanceId.ToGuid();
            var instance = await _runtimeTables.WorkflowInstance.ReadAsync(instanceIdAsGuid, cancellationToken);
            if (instance == null) return null;

            var version = await _configurationTables.WorkflowVersion.ReadAsync(instance.WorkflowVersionId, cancellationToken);
            FulcrumAssert.IsNotNull(version, CodeLocation.AsString());
            FulcrumAssert.AreEqual(version.Id, instance.WorkflowVersionId, CodeLocation.AsString());
            var form = await _configurationTables.WorkflowForm.ReadAsync(version.WorkflowFormId, cancellationToken);
            FulcrumAssert.IsNotNull(form, CodeLocation.AsString());
            FulcrumAssert.AreEqual(form.Id, version.WorkflowFormId, CodeLocation.AsString());

            return await GetSummaryAsync(form, version, instance, cancellationToken);
        }

        public async Task<WorkflowSummary> GetSummaryAsync(string formId, int majorVersion, string instanceId,
            CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNullOrWhiteSpace(formId, nameof(formId));
            InternalContract.RequireGreaterThanOrEqualTo(0, majorVersion, nameof(majorVersion));
            InternalContract.RequireNotNullOrWhiteSpace(instanceId, nameof(instanceId));

            // Form
            var formIdAsGuid = formId.ToGuid();
            var form = await _configurationTables.WorkflowForm.ReadAsync(formIdAsGuid, cancellationToken);

            // Version
            WorkflowVersionRecord version = null;
            if (form != null)
            {
                version = await _configurationTables.WorkflowVersion.FindByFormAndMajorAsync(formIdAsGuid, majorVersion,
                    cancellationToken);
            }

            // Instance
            var instanceIdAsGuid = instanceId.ToGuid();
            var instance = await _runtimeTables.WorkflowInstance.ReadAsync(instanceIdAsGuid, cancellationToken);
            if (instance != null)
            {
                FulcrumAssert.IsNotNull(version, CodeLocation.AsString());
                FulcrumAssert.AreEqual(version!.Id, instance.WorkflowVersionId, CodeLocation.AsString());
                FulcrumAssert.IsNotNull(form, CodeLocation.AsString());
                FulcrumAssert.AreEqual(form!.Id, version.WorkflowFormId, CodeLocation.AsString());
            }

            return await GetSummaryAsync(form, version, instance, cancellationToken);
        }

        private async Task<WorkflowSummary> GetSummaryAsync(WorkflowFormRecord form, WorkflowVersionRecord version, WorkflowInstanceRecord instance,
            CancellationToken cancellationToken = default)
        {
            // Activities
            var (activityForms, activityVersions, activityInstances) =
                await ReadAllActivitiesAndUpdateResponsesAsync(form?.Id, version?.Id, instance?.Id, cancellationToken);

            var activityTree = BuildActivityTree(null, activityForms, activityVersions, activityInstances);
            activityTree.Sort(PositionSort);

            var workflowSummary = new WorkflowSummary
            {
                Instance = instance == null ? null : new WorkflowInstance().From(instance),
                Form = form == null ? null : new WorkflowForm().From(form),
                Version = version == null ? null : new WorkflowVersion().From(version),
                ActivityForms = activityForms,
                ActivityVersions = activityVersions,
                ActivityInstances = activityInstances,
                ReferredActivities = BuildReferred(activityForms, activityVersions, activityInstances),
                ActivityTree = activityTree
            };

            FulcrumAssert.IsValidated(workflowSummary);
            return workflowSummary;
        }

        private static int PositionSort(ActivitySummary x, ActivitySummary y)
        {
            return x.Version.Position.CompareTo(y.Version.Position);
        }

        private List<ActivitySummary> BuildReferred(Dictionary<string, ActivityForm> activityForms,
            Dictionary<string, ActivityVersion> activityVersions,
            Dictionary<string, ActivityInstance> activityInstances)
        {
            var activities = new List<ActivitySummary>();
            foreach (var entry in activityInstances)
            {
                var instance = entry.Value;
                var version = activityVersions[instance.ActivityVersionId];
                var form = activityForms[version.ActivityFormId];
                var activity = new ActivitySummary
                {
                    Instance = instance,
                    Version = version,
                    Form = form
                };
                activity.Children = BuildActivityTree(activity, activityForms, activityVersions, activityInstances);
                activities.Add(activity);
            }

            return activities;
        }

        private List<ActivitySummary> BuildActivityTree(ActivitySummary parent, Dictionary<string, ActivityForm> activityForms,
            Dictionary<string, ActivityVersion> activityVersions,
            Dictionary<string, ActivityInstance> activityInstances)
        {
            var activities = new List<ActivitySummary>();
            foreach (var entry in activityInstances.Where(x =>
                x.Value.ParentActivityInstanceId == parent?.Instance.Id))
            {
                var instance = entry.Value;
                var version = activityVersions[instance.ActivityVersionId];
                var form = activityForms[version.ActivityFormId];
                var activity = new ActivitySummary
                {
                    Instance = instance,
                    Version = version,
                    Form = form
                };
                activity.Children = BuildActivityTree(activity, activityForms, activityVersions, activityInstances);
                activities.Add(activity);
            }

            return activities;
        }

        private async Task<(Dictionary<string, ActivityForm> activityForms,
                Dictionary<string, ActivityVersion> activityVersions, Dictionary<string, ActivityInstance>
                activityInstances)>
            ReadAllActivitiesAndUpdateResponsesAsync(Guid? formId, Guid? versionId, Guid? instanceId, CancellationToken cancellationToken)
        {
            // ActivityForms
            Dictionary<string, ActivityForm> activityForms;
            if (formId == null)
            {
                activityForms = new Dictionary<string, ActivityForm>();
            }
            else
            {
                var activityFormsList =
                    await _configurationTables.ActivityForm.SearchByWorkflowFormIdAsync(formId.Value, int.MaxValue, cancellationToken);
                activityForms = activityFormsList.ToDictionary(x => x.Id.ToGuidString(), x => new ActivityForm().From(x));
            }
            // ActivityVersions
            Dictionary<string, ActivityVersion> activityVersions;
            if (versionId == null)
            {
                activityVersions = new Dictionary<string, ActivityVersion>();
            }
            else
            {
                var activityVersionsList = await _configurationTables.ActivityVersion.SearchByWorkflowVersionIdAsync(versionId.Value, int.MaxValue, cancellationToken);
                activityVersions = activityVersionsList.ToDictionary(x => x.Id.ToGuidString(), x => new ActivityVersion().From(x));
            }

            // ActivityInstances
            Dictionary<string, ActivityInstance> activityInstances;
            if (instanceId == null)
            {
                activityInstances = new Dictionary<string, ActivityInstance>();
            }
            else
            {

                var activityInstancesList = await _runtimeTables.ActivityInstance.SearchByWorkflowInstanceIdAsync(instanceId.Value, int.MaxValue, cancellationToken);

                activityInstances = activityInstancesList.ToDictionary(x => x.Id.ToGuidString(), x => new ActivityInstance().From(x));
            }

            return (activityForms, activityVersions, activityInstances);
        }

        private static bool HasCompleted(ActivityInstanceRecord activityInstanceRecord)
        {
            return activityInstanceRecord.FinishedAt.HasValue;
        }
    }
}
