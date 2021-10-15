using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities.Runtime;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Crud.Helpers;
using Nexus.Link.Libraries.Crud.Model;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Support;

namespace Nexus.Link.WorkflowEngine.Sdk.Services;

public class WorkflowService : IWorkflowService
{
    private readonly IConfigurationTables _configurationTables;
    private readonly IRuntimeTables _runtimeTables;

    public WorkflowService(IConfigurationTables configurationTables, IRuntimeTables runtimeTables)
    {
        _configurationTables = configurationTables;
        _runtimeTables = runtimeTables;
    }

    public async Task<Workflow> ReadAsync(string id, CancellationToken cancellationToken = default)
    {
        InternalContract.RequireNotNullOrWhiteSpace(id, nameof(id));

        var idAsGuid = MapperHelper.MapToType<Guid, string>(id);
        var instance = await _runtimeTables.WorkflowInstance.ReadAsync(idAsGuid, cancellationToken);
        var version = await _configurationTables.WorkflowVersion.ReadAsync(instance.WorkflowVersionId, cancellationToken);
        var form = await _configurationTables.WorkflowForm.ReadAsync(version.WorkflowFormId, cancellationToken);

        var workflow = new Workflow
        {
            Instance = new WorkflowInstance().From(instance),
            Form = new WorkflowForm().From(form),
            Version = new WorkflowVersion().From(version)
        };

        var (activityForms, activityVersions, activityInstances) = await ReadAllActivities(form.Id, version.Id, instance.Id, cancellationToken);
        workflow.Activities = BuildActivityTree(null, activityForms, activityVersions, activityInstances);

        return workflow;
    }

    private List<Activity> BuildActivityTree(Activity parent, Dictionary<string, ActivityFormRecord> activityForms, Dictionary<string, ActivityVersionRecord> activityVersions, Dictionary<string, ActivityInstanceRecord> activityInstances)
    {
        var activities = new List<Activity>();
        foreach (var entry in activityInstances.Where(x => x.Value.ParentActivityInstanceId?.ToString() == parent?.Instance.Id))
        {
            var version = activityVersions[entry.Value.ActivityVersionId.ToString()];
            var form = activityForms[version.ActivityFormId.ToString()];
            var activity = new Activity
            {
                Instance = new ActivityInstance().From(entry.Value),
                Version = new ActivityVersion().From(version),
                Form = new ActivityForm().From(form)
            };
            activity.Children = BuildActivityTree(activity, activityForms, activityVersions, activityInstances);
            activities.Add(activity);
        }
        return activities;
    }

    private async Task<(Dictionary<string, ActivityFormRecord> activityForms, Dictionary<string, ActivityVersionRecord> activityVersions, Dictionary<string, ActivityInstanceRecord> activityInstances)>
        ReadAllActivities(Guid formId, Guid versionId, Guid instanceId, CancellationToken cancellationToken)
    {
        var activityFormsList = await _configurationTables.ActivityForm.SearchAsync(new SearchDetails<ActivityFormRecord>(new ActivityFormRecordSearch { WorkflowFormId = formId }), 0, int.MaxValue, cancellationToken);
        var activityForms = activityFormsList.Data.ToDictionary(x => x.Id.ToString(), x => x);

        var activityVersionsList = await _configurationTables.ActivityVersion.SearchAsync(new SearchDetails<ActivityVersionRecord>(new ActivityVersionRecordSearch { WorkflowVersionId = versionId }), 0, int.MaxValue, cancellationToken);
        var activityVersions = activityVersionsList.Data.ToDictionary(x => x.Id.ToString(), x => x);

        var activityInstancesList = await _runtimeTables.ActivityInstance.SearchAsync(new SearchDetails<ActivityInstanceRecord>(new ActivityInstanceRecordSearch { WorkflowInstanceId = instanceId }), 0, int.MaxValue, cancellationToken);
        var activityInstances = activityInstancesList.Data.ToDictionary(x => x.Id.ToString(), x => x);

        return (activityForms, activityVersions, activityInstances);
    }
}