using System;
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
    [Obsolete("This will not be supported. Please use Action+Catch. Obsolete since 2022-06-15.")]
    public static async Task CatchExitExceptionAsync(this Task task, LoopActivity activity, CancellationToken cancellationToken)
    {
        try
        {
            await task;
        }
        catch (WorkflowImplementationShouldNotCatchThisException outerException)
        {
#pragma warning disable CS0618
            if (outerException.InnerException is not IgnoreAndExitToParentException innerException) throw;
#pragma warning restore CS0618
            FulcrumAssert.IsNotNull(innerException.ActivityFailedException, CodeLocation.AsString());
            var e = innerException.ActivityFailedException;
            await activity.LogInformationAsync($"Ignoring exception in iteration {activity.LoopIteration}: {e.TechnicalMessage}", e, cancellationToken);
        }
    }

    [Obsolete("This will not be supported. Please use Action+Catch. Obsolete since 2022-06-15.")]
    public static async Task<TActivityReturns> CatchExitExceptionAsync<TActivityReturns>(this Task<TActivityReturns> task, LoopActivity<TActivityReturns> activity, CancellationToken cancellationToken)
    {
        try
        {
            return await task;
        }
        catch (WorkflowImplementationShouldNotCatchThisException outerException)
        {
#pragma warning disable CS0618
            if (outerException.InnerException is not IgnoreAndExitToParentException innerException) throw;
#pragma warning restore CS0618
            FulcrumAssert.IsNotNull(innerException.ActivityFailedException, CodeLocation.AsString());
            var e = innerException.ActivityFailedException;
            var result = await activity.DefaultValueMethodAsync(cancellationToken);
            await activity.LogInformationAsync($"Ignoring exception in iteration {activity.LoopIteration}, using the default value.", new { Exception = e, Result = result }, cancellationToken);
            return result;
        }
    }
}