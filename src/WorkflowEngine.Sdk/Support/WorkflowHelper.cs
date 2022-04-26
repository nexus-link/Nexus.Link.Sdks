using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Web.Error.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Exceptions;

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
        RequestPostponedException outException = null;
        foreach (var task in tasks)
        {
            try
            {
                await task;
            }
            catch (WorkflowImplementationShouldNotCatchThisException et)
            {
                if (et.InnerException is RequestPostponedException rpe)
                {
                    outException ??= new RequestPostponedException();
                    outException.AddWaitingForIds(rpe.WaitingForRequestIds);
                    if (!outException.TryAgain)
                    {
                        outException.TryAgain = rpe.TryAgain;
                        var replaceCurrentValue = !outException.TryAgainAfterMinimumTimeSpan.HasValue
                                                  || rpe.TryAgainAfterMinimumTimeSpan.HasValue &&
                                                  rpe.TryAgainAfterMinimumTimeSpan <
                                                  outException.TryAgainAfterMinimumTimeSpan;
                        if (replaceCurrentValue)
                        {
                            outException.TryAgainAfterMinimumTimeSpan = rpe.TryAgainAfterMinimumTimeSpan;
                        }
                    }
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

        if (outException != null) throw outException;
        await Task.WhenAll(exceptionTasks);
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

}