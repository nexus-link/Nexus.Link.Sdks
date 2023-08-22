using System.Collections.Generic;
using System.Linq;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Configuration.Entities;

namespace Nexus.Link.WorkflowEngine.Sdk.Abstract.State.Entities;

/// <summary>
/// This is a collection af all the information about a specific workflow instance
/// </summary>
public class WorkflowSummary : IValidatable
{
    private IReadOnlyList<ActivitySummary> _activityTree;
    private IReadOnlyList<ActivitySummary> _referredActivities;
    private object _lock = new object();

    /// <summary>
    /// The workflow form
    /// </summary>
    public WorkflowForm Form { get; set; }

    /// <summary>
    /// The workflow version
    /// </summary>
    public WorkflowVersion Version { get; set; }

    /// <summary>
    /// The workflow instance
    /// </summary>
    public WorkflowInstance Instance { get; set; }

    /// <summary>
    /// All known activity forms"/>.
    /// </summary>
    public IDictionary<string, ActivityForm> ActivityForms{ get; set; }
    /// <summary>
    /// All known activity versions"/>.
    /// </summary>
    public IDictionary<string, ActivityVersion> ActivityVersions { get; set; }
    /// <summary>
    /// All known activity instances
    /// </summary>
    public IDictionary<string, ActivityInstance> ActivityInstances{ get; set; }

    /// <summary>
    /// Sorted top level activities in position order
    /// </summary>
    public IReadOnlyList<ActivitySummary> ActivityTree
    {
        get
        {
            lock (_lock)
            {
                if (_activityTree != null) return _activityTree;
                var activityTree = BuildActivityTree(null);
                activityTree.Sort(PositionSort);
                _activityTree = activityTree;
                return _activityTree;
            }
        }
    }

    /// <summary>
    /// Sorted top level activities in position order
    /// </summary>
    public IReadOnlyList<ActivitySummary> ReferredActivities
    {
        get
        {
            lock (_lock)
            {
                if (_referredActivities != null) return _referredActivities;
                _referredActivities = BuildReferred();
                return _referredActivities;
            }
        }
    }

    /// <inheritdoc />
    public override string ToString() => $"{Form} {Version} {Instance}";

    /// <inheritdoc />
    public void Validate(string errorLocation, string propertyPath = "")
    {
        if (Instance != null)
        {
            FulcrumValidate.IsNotNull(Version, nameof(Version), errorLocation);
            FulcrumValidate.AreEqual(Version.Id, Instance.WorkflowVersionId,
                $"{nameof(Instance)}.{nameof(Instance.WorkflowVersionId)}", errorLocation);
        }
        if (Version != null)
        {
            FulcrumValidate.IsNotNull(Form, nameof(Form), errorLocation);
            FulcrumValidate.AreEqual(Form.Id, Version.WorkflowFormId,
                $"{nameof(Version)}.{nameof(Version.WorkflowFormId)}", errorLocation);
        }
            
        FulcrumValidate.IsNotNull(ActivityTree, nameof(ActivityTree), errorLocation);
        FulcrumValidate.IsNotNull(ReferredActivities, nameof(ReferredActivities), errorLocation);
        FulcrumValidate.IsNotNull(ActivityForms, nameof(ActivityForms), errorLocation);
        FulcrumValidate.IsNotNull(ActivityVersions, nameof(ActivityVersions), errorLocation);
        FulcrumValidate.IsNotNull(ActivityInstances, nameof(ActivityInstances), errorLocation);
    }

    private static int PositionSort(ActivitySummary x, ActivitySummary y)
    {
        return x.Version.Position.CompareTo(y.Version.Position);
    }

    private List<ActivitySummary> BuildReferred()
    {
        var activities = new List<ActivitySummary>();
        foreach (var entry in ActivityInstances)
        {
            var instance = entry.Value;
            var version = ActivityVersions[instance.ActivityVersionId];
            var form = ActivityForms[version.ActivityFormId];
            var activity = new ActivitySummary
            {
                Instance = instance,
                Version = version,
                Form = form
            };
            activity.Children = BuildActivityTree(activity);
            activities.Add(activity);
        }

        return activities;
    }

    private List<ActivitySummary> BuildActivityTree(ActivitySummary parent)
    {
        var activities = new List<ActivitySummary>();
        foreach (var entry in ActivityInstances.Where(x =>
                     x.Value.ParentActivityInstanceId == parent?.Instance.Id))
        {
            var instance = entry.Value;
            var version = ActivityVersions[instance.ActivityVersionId];
            var form = ActivityForms[version.ActivityFormId];
            var activity = new ActivitySummary
            {
                Instance = instance,
                Version = version,
                Form = form
            };
            activity.Children = BuildActivityTree(activity);
            activities.Add(activity);
        }

        return activities;
    }
}