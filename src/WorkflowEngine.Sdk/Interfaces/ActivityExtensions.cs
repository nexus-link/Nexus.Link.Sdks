using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowState.Abstract.Entities;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Web.Error.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Exceptions;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Interfaces;

namespace Nexus.Link.WorkflowEngine.Sdk.Interfaces;

/// <summary>
/// Extensions for <see cref="IActivity"/> related interfaces.
/// </summary>
public static class ActivityExtensions
{
    /// <summary>
    /// Start the activity, but don't wait for the activity to complete.
    /// Use <see cref="EndAsync{T}"/> to wait for the activity to complete.
    /// </summary>
    public static async Task<BackgroundActivity> BeginAsync(this IExecutableActivity activity, CancellationToken cancellationToken = default)
    {
        InternalContract.RequireNotNull(activity, nameof(activity));
        try
        {
            await activity.ExecuteAsync(cancellationToken);
        }
        catch (WorkflowImplementationShouldNotCatchThisException outerException)
        {
            if (outerException.InnerException is not RequestPostponedException) throw;
            // Ignore postpone exceptions, this will be taken care of in EndAsync();
        }
        FulcrumAssert.IsNotNull(activity.ActivityInstanceId, CodeLocation.AsString());
        return new BackgroundActivity { ActivityInstanceId = activity.ActivityInstanceId };
    }

    /// <summary>
    /// Start the activity, but don't wait for the activity to complete.
    /// Use <see cref="EndAsync{T}"/> to wait for the activity to complete.
    /// </summary>
    public static async Task<BackgroundActivity<T>> BeginAsync<T>(this IExecutableActivity<T> activity, CancellationToken cancellationToken = default)
    {
        InternalContract.RequireNotNull(activity, nameof(activity));
        try
        {
            await activity.ExecuteAsync(cancellationToken);
        }
        catch (WorkflowImplementationShouldNotCatchThisException outerException)
        {
            if (outerException.InnerException is not RequestPostponedException) throw;
            // Ignore postpone exceptions, this will be taken care of in EndAsync();
        }
        FulcrumAssert.IsNotNull(activity.ActivityInstanceId, CodeLocation.AsString());
        return new BackgroundActivity<T> { ActivityInstanceId = activity.ActivityInstanceId };
    }

    /// <summary>
    /// Wait for the activity to complete.
    /// </summary>
    public static async Task EndAsync(this BackgroundActivity backgroundActivity, CancellationToken cancellationToken = default)
    {
        var activity = backgroundActivity.GetActivity();
        if (activity == null) return;
        await activity.ExecuteAsync(cancellationToken);
    }

    /// <summary>
    /// Wait for the activity to complete.
    /// </summary>
    public static async Task<T> EndAsync<T>(this BackgroundActivity<T> backgroundActivity, CancellationToken cancellationToken = default)
    {
        var activity = backgroundActivity.GetActivity();
        if (activity != null)
        {
            return await activity.ExecuteAsync(cancellationToken);
        }

        return backgroundActivity.GetResult();
    }
}