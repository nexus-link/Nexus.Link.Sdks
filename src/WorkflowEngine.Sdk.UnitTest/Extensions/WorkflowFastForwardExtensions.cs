﻿using System;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Activities;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Logic;

namespace Nexus.Link.WorkflowEngine.Sdk.UnitTest.Extensions;

/// <summary>
/// Extensions for fast forwarding workflows when testing them.
/// </summary>
public static class WorkflowFastForwardExtensions
{
    /// <summary>
    /// Mark the <paramref name="activity"/> as completed and successful.
    /// </summary>
    [Obsolete("Please use the new activity declaration style and use ExecutionAsync() with no method parameter instead of this method. Obsolete since 2022-05-02.")]
    public static void Success(this IActivity activity)
    {
        var activityImplementation = activity as Activity;
        FulcrumAssert.IsNotNull(activityImplementation, CodeLocation.AsString());
        var executor = activityImplementation!.ActivityInformation.Workflow.GetActivityExecutor(activityImplementation);
        FulcrumAssert.IsNotNull(executor, CodeLocation.AsString());
        executor!.ExecuteWithoutReturnValueAsync(_ => Task.CompletedTask).Wait();
    }

    /// <summary>
    /// Mark the <paramref name="activity"/> as completed, successful and with result <paramref name="value"/>.
    /// </summary>
    [Obsolete("Please use the new activity declaration style and use ExecutionAsync() with no method parameter instead of this method. Obsolete since 2022-05-02.")]
    public static T Success<T>(this IActivity activity, T value)
    {
        var activityImplementation = activity as Activity;
        FulcrumAssert.IsNotNull(activityImplementation, CodeLocation.AsString());
        var executor = activityImplementation!.ActivityInformation.Workflow.GetActivityExecutor(activityImplementation);
        FulcrumAssert.IsNotNull(executor, CodeLocation.AsString());
        return executor!.ExecuteWithReturnValueAsync(_ => Task.FromResult(value), null).Result;
    }
}