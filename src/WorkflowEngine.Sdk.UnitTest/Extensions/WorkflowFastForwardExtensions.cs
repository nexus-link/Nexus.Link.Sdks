using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Logic;

namespace Nexus.Link.WorkflowEngine.Sdk.UnitTest.Extensions;

public static class WorkflowFastForwardExtensions
{
    public static void Success(this IActivity activity)
    {
        var activityImplementation = activity as Activity;
        FulcrumAssert.IsNotNull(activityImplementation, CodeLocation.AsString());
        activityImplementation!.InternalExecuteAsync((_, _) => Task.CompletedTask).Wait();
    }

    public static T Success<T>(this IActivity<T> activity, T value)
    {
        var activityImplementation = activity as Activity<T>;
        FulcrumAssert.IsNotNull(activityImplementation, CodeLocation.AsString());
        return activityImplementation!.InternalExecuteAsync((_, _) => Task.FromResult(value), null).Result;
    }
}