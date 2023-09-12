using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Web.Error.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Exceptions;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Exceptions;

namespace Nexus.Link.WorkflowEngine.Sdk.Support;

/// <summary>
/// Methods to help workflow implementations
/// </summary>
public class WorkflowHelper
{
    /// <summary>
    /// Similar to <see cref="Task.WhenAll(IEnumerable{Task})"/>, but will aggregate <see cref="RequestPostponedException"/>
    /// </summary>
    /// <param name="tasks"></param>
    /// <exception cref="RequestPostponedException"></exception>
    public static async Task WhenAllActivities(IEnumerable<Task> tasks)
    {
        var exceptionTasks = new List<Task>();
        RequestPostponedException aggregatedException = null;
        var hasOuterException = false;
        foreach (var task in tasks)
        {
            try
            {
                await task;
            }
#pragma warning disable CS0618
            catch (IgnoreAndExitToParentException)
            {
                // Ignore the exception by not adding it to the list exceptionTasksFulcrumAssert.IsNotNull(innerException.ActivityFailedException, CodeLocation.AsString());
            }
#pragma warning restore CS0618
            catch (RequestPostponedException e)
            {
                AggregateException(e);
            }
            catch (WorkflowImplementationShouldNotCatchThisException outerException)
            {
#pragma warning disable CS0618
                if (outerException.InnerException is IgnoreAndExitToParentException)
                {
                    // Ignore the exception by not adding it to the list exceptionTasksFulcrumAssert.IsNotNull(innerException.ActivityFailedException, CodeLocation.AsString());
                }
#pragma warning restore CS0618
                else if (outerException.InnerException is RequestPostponedException rpe)
                {
                    hasOuterException = true;
                    AggregateException(rpe);
                }
                else
                {
                    exceptionTasks.Add(task);
                }
            }
            catch (Exception)
            {
                exceptionTasks.Add(task);
            }
        }

        if (aggregatedException != null)
        {
            if (hasOuterException) throw new WorkflowImplementationShouldNotCatchThisException(aggregatedException);
            throw aggregatedException;
        }
        await Task.WhenAll(exceptionTasks);

        void AggregateException(RequestPostponedException e)
        {
            if (aggregatedException == null)
            {
                aggregatedException = e;
                return;
            }

            aggregatedException.AddWaitingForIds(e.WaitingForRequestIds);
            if (!e.TryAgainAfterMinimumTimeSpan.HasValue) return;
            if (!aggregatedException.TryAgainAfterMinimumTimeSpan.HasValue
                || aggregatedException.TryAgainAfterMinimumTimeSpan.Value > e.TryAgainAfterMinimumTimeSpan.Value)
            {
                aggregatedException.TryAgainAfterMinimumTimeSpan = e.TryAgainAfterMinimumTimeSpan.Value;
            }
        }
    }

    /// <summary>
    /// Similar to <see cref="Task.WhenAll(Task[])"/>, but will aggregate <see cref="RequestPostponedException"/>
    /// </summary>
    /// <param name="tasks"></param>
    /// <exception cref="RequestPostponedException"></exception>
    public static Task WhenAllActivities(params Task[] tasks)
    {
        return WhenAllActivities(tasks.ToList());
    }

    /// <summary>
    /// Performs a retry operation for given function.
    /// </summary>
    /// <param name="actionToExecute">Function to execute.</param>
    /// <param name="maxRetries">Maximum number of retries.</param>
    /// <param name="delayBetweenRetries">Deley between retries.</param>
    /// <returns></returns>
    public static async Task RetryAsync(
        Func<Task> actionToExecute,
        int maxRetries,
        TimeSpan delayBetweenRetries)
    {
        int retryCount = 0;
        while (true)
        {
            try
            {
                await actionToExecute();
                return;
            }
            catch (Exception)
            {
                if (retryCount >= maxRetries)
                {
                    throw;
                }

                retryCount++;
                // Exponential back-off i.e. the duration to wait between retries based on the current retry attempt.
                await Task.Delay(delayBetweenRetries * retryCount);
            }
        }
    }

}