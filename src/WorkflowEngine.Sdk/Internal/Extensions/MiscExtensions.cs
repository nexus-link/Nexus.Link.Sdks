using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.WorkflowEngine.Sdk.Exceptions;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Exceptions;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Extensions.State;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Logic;

namespace Nexus.Link.WorkflowEngine.Sdk.Internal.Extensions;

internal static class MiscExtensions
{
    public static async Task CatchExitExceptionAsync(this Task task, LoopActivity activity, CancellationToken cancellationToken)
    {
        try
        {
            await task;
        }
        catch (WorkflowImplementationShouldNotCatchThisException outerException)
        {
            if (outerException.InnerException is not IgnoreAndExitToParentException innerException) throw;
            FulcrumAssert.IsNotNull(innerException.ActivityFailedException, CodeLocation.AsString());
            var e = innerException.ActivityFailedException;
            await activity.LogInformationAsync($"Ignoring exception in iteration {activity.LoopIteration}: {e.TechnicalMessage}", e, cancellationToken);
        }
    }

    public static async Task<TActivityReturns> CatchExitExceptionAsync<TActivityReturns>(this Task<TActivityReturns> task, LoopActivity<TActivityReturns> activity, CancellationToken cancellationToken)
    {
        try
        {
            return await task;
        }
        catch (WorkflowImplementationShouldNotCatchThisException outerException)
        {
            if (outerException.InnerException is not IgnoreAndExitToParentException innerException) throw;
            FulcrumAssert.IsNotNull(innerException.ActivityFailedException, CodeLocation.AsString());
            var e = innerException.ActivityFailedException;
            var result = await activity.DefaultValueMethodAsync(cancellationToken);
            await activity.LogInformationAsync($"Ignoring exception in iteration {activity.LoopIteration}, using the default value.", new { Exception = e, Result = result }, cancellationToken);
            return result;
        }
    }

    public static async Task<TMethodReturns>  CatchExitExceptionAsync<TActivityReturns, TMethodReturns>(this Task<TMethodReturns> task, LoopActivity<TActivityReturns, TMethodReturns> activity, CancellationToken cancellationToken)
    {
        try
        {
            return await task;
        }
        catch (WorkflowImplementationShouldNotCatchThisException outerException)
        {
            if (outerException.InnerException is not IgnoreAndExitToParentException innerException) throw;
            FulcrumAssert.IsNotNull(innerException.ActivityFailedException, CodeLocation.AsString());
            var e = innerException.ActivityFailedException;
            var result = await activity.DefaultValueMethodAsync(cancellationToken);
            await activity.LogInformationAsync($"Ignoring exception in iteration {activity.LoopIteration}, using the default value.", new { Exception = e, Result = result }, cancellationToken);
            return result;
        }
    }
}