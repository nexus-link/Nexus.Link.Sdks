using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Support;

namespace Nexus.Link.WorkflowEngine.Sdk;

/// <summary>
/// Holds information about an activity that is running in the background.
/// </summary>
public class BackgroundActivity
{
    /// <summary>
    /// The id of the activity that is running in the background
    /// </summary>
    public string ActivityInstanceId { get; set; }

    internal IExecutableActivity GetActivity()
    {
        var workflowExecutor = WorkflowStatic.Context.CurrentWorkflowExecutor;
        FulcrumAssert.IsNotNull(workflowExecutor, CodeLocation.AsString());
        var success = workflowExecutor.WorkflowInformation.TryGetActivity(ActivityInstanceId, out var activity);
        if (!success) return null;
        var executableActivity = activity as IExecutableActivity;
        FulcrumAssert.IsNotNull(executableActivity, CodeLocation.AsString());
        return executableActivity;
    }

    /// <summary>
    /// True if the background activity has completed
    /// </summary>
    public bool HasCompleted
    {
        get
        {
            var activity = GetActivity();
            return activity is { HasCompleted: true };
        }
    }
}

/// <summary>
/// Holds information about an activity that is running in the background.
/// </summary>
public class BackgroundActivity<TActivityReturns>
{
    /// <summary>
    /// The id of the activity that is running in the background
    /// </summary>
    public string ActivityInstanceId { get; set; }

    internal IExecutableActivity<TActivityReturns> GetActivity()
    {
        var workflowExecutor = WorkflowStatic.Context.CurrentWorkflowExecutor;
        FulcrumAssert.IsNotNull(workflowExecutor, CodeLocation.AsString());
        var success = workflowExecutor.WorkflowInformation.TryGetActivity(ActivityInstanceId, out var activity);
        if (!success) return null;
        var executableActivity = activity as IExecutableActivity<TActivityReturns>;
        FulcrumAssert.IsNotNull(executableActivity, CodeLocation.AsString());
        return executableActivity;
    }

    /// <summary>
    /// True if the background activity has completed
    /// </summary>
    public bool HasCompleted
    {
        get
        {
            var activity = GetActivity();
            return activity is { HasCompleted: true };
        }
    }

    /// <summary>
    /// Get the result value from the activity.
    /// </summary>
    /// <returns></returns>
    public TActivityReturns GetResult()
    {
        var workflowExecutor = WorkflowStatic.Context.CurrentWorkflowExecutor;
        FulcrumAssert.IsNotNull(workflowExecutor, CodeLocation.AsString());
        var result = workflowExecutor.WorkflowInformation.GetActivityResult<TActivityReturns>(ActivityInstanceId);
        return result;
    }
}