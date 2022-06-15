using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Web.Error.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Exceptions;
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
        RequestPostponedException outException = null;
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
                outException ??= new RequestPostponedException();
                outException.AddWaitingForIds(e.WaitingForRequestIds);
                if (!outException.TryAgain)
                {
                    outException.TryAgain = e.TryAgain;
                    var replaceCurrentValue = !outException.TryAgainAfterMinimumTimeSpan.HasValue
                                              || e.TryAgainAfterMinimumTimeSpan.HasValue &&
                                              e.TryAgainAfterMinimumTimeSpan <
                                              outException.TryAgainAfterMinimumTimeSpan;
                    if (replaceCurrentValue)
                    {
                        outException.TryAgainAfterMinimumTimeSpan = e.TryAgainAfterMinimumTimeSpan;
                    }
                }
            }
            // TODO: Remove?
            //catch (WorkflowImplementationShouldNotCatchThisException outerException)
            //{
            //    if (outerException.InnerException is IgnoreAndExitToParentException)
            //    {
            //        // Ignore the exception by not adding it to the list exceptionTasksFulcrumAssert.IsNotNull(innerException.ActivityFailedException, CodeLocation.AsString());
            //    }
            //    else if (outerException.InnerException is RequestPostponedException rpe)
            //    {
            //        outException ??= new RequestPostponedException();
            //        outException.AddWaitingForIds(rpe.WaitingForRequestIds);
            //        if (!outException.TryAgain)
            //        {
            //            outException.TryAgain = rpe.TryAgain;
            //            var replaceCurrentValue = !outException.TryAgainAfterMinimumTimeSpan.HasValue
            //                                      || rpe.TryAgainAfterMinimumTimeSpan.HasValue &&
            //                                      rpe.TryAgainAfterMinimumTimeSpan <
            //                                      outException.TryAgainAfterMinimumTimeSpan;
            //            if (replaceCurrentValue)
            //            {
            //                outException.TryAgainAfterMinimumTimeSpan = rpe.TryAgainAfterMinimumTimeSpan;
            //            }
            //        }
            //    }
            //    else
            //    {
            //        exceptionTasks.Add(task);
            //    }
            //}
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