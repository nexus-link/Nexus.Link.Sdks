using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Error.Logic;
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
    public static async Task<IBackgroundActivity> BeginAsync(this IExecutableActivity activity, CancellationToken cancellationToken = default)
    {
        if (activity is not IBackgroundActivity backgroundActivity)
        {
            throw new FulcrumContractException($"The class {activity.GetType().Name} must implement {nameof(IBackgroundActivity)}.");
        }

        try
        {
            await activity.ExecuteAsync(cancellationToken);
        }
        catch (WorkflowImplementationShouldNotCatchThisException outerException)
        {
            if (outerException.InnerException is not RequestPostponedException) throw;
            // Ignore postpone exceptions, this will be taken care of in EndAsync();
        }
        return backgroundActivity;
    }

    /// <summary>
    /// Start the activity, but don't wait for the activity to complete.
    /// Use <see cref="EndAsync{T}"/> to wait for the activity to complete.
    /// </summary>
    public static async Task<IBackgroundActivity<T>> BeginAsync<T>(this IExecutableActivity<T> activity, CancellationToken cancellationToken = default)
    {
        if (activity is not IBackgroundActivity<T> backgroundActivity)
        {
            throw new FulcrumContractException($"The class {activity.GetType().Name} must implement {nameof(IBackgroundActivity)}.");
        }

        try
        {
            await activity.ExecuteAsync(cancellationToken);
        }
        catch (WorkflowImplementationShouldNotCatchThisException outerException)
        {
            if (outerException.InnerException is not RequestPostponedException) throw;
            // Ignore postpone exceptions, this will be taken care of in EndAsync();
        }
        return backgroundActivity;
    }

    /// <summary>
    /// Wait for the activity to complete.
    /// </summary>
    public static async Task EndAsync(this IBackgroundActivity activity, CancellationToken cancellationToken = default)
    {
        if (activity is not IExecutableActivity executableActivity)
        {
            throw new FulcrumContractException($"The class {activity.GetType().Name} must implement {nameof(IExecutableActivity)}.");
        }

        await executableActivity.ExecuteAsync(cancellationToken);
    }

    /// <summary>
    /// Wait for the activity to complete.
    /// </summary>
    public static async Task<T> EndAsync<T>(this IBackgroundActivity<T> activity, CancellationToken cancellationToken = default)
    {
        if (activity is not IExecutableActivity<T> executableActivity)
        {
            throw new FulcrumContractException($"The class {activity.GetType().Name} must implement {nameof(IExecutableActivity)}.");
        }

        var result = await executableActivity.ExecuteAsync(cancellationToken);
        return result;
    }
}