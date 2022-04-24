﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract.Entities;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Support;

namespace Nexus.Link.WorkflowEngine.Sdk.Internal.Logic
{
    internal class ActivityCondition<TActivityReturns> : Activity<TActivityReturns>, IActivityCondition<TActivityReturns>
    {
        private readonly Func<CancellationToken, Task<TActivityReturns>> _getDefaultValueMethodAsync;

        public ActivityCondition(IActivityInformation activityInformation, Func<CancellationToken, Task<TActivityReturns>> getDefaultValueMethodAsync)
            : base(activityInformation)
        {
            _getDefaultValueMethodAsync = getDefaultValueMethodAsync;
        }

        public Task<TActivityReturns> ExecuteAsync(
            Func<IActivityCondition<TActivityReturns>, CancellationToken, Task<TActivityReturns>> conditionMethodAsync,
            CancellationToken cancellationToken = default)
        {
            return InternalExecuteAsync((instance, ct) => MapMethodAsync(conditionMethodAsync, instance, ct), _getDefaultValueMethodAsync, cancellationToken);
        }

        public Task<TActivityReturns> ExecuteAsync(Func<IActivityCondition<TActivityReturns>, TActivityReturns> conditionMethod,
            CancellationToken cancellationToken = default)
        {
            return InternalExecuteAsync((instance, _) => MapMethodAsync(conditionMethod, instance), _getDefaultValueMethodAsync, cancellationToken);
        }

        private static Task<TActivityReturns> MapMethodAsync(
            Func<IActivityCondition<TActivityReturns>, CancellationToken, Task<TActivityReturns>> method,
            IActivity instance, CancellationToken cancellationToken)
        {
            var condition = instance as IActivityCondition<TActivityReturns>;
            FulcrumAssert.IsNotNull(condition, CodeLocation.AsString());
            return method(condition, cancellationToken);
        }

        private static Task<TActivityReturns> MapMethodAsync(
            Func<IActivityCondition<TActivityReturns>, TActivityReturns> method,
            IActivity instance)
        {
            var condition = instance as IActivityCondition<TActivityReturns>;
            FulcrumAssert.IsNotNull(condition, CodeLocation.AsString());
            return Task.FromResult(method(condition));
        }
    }
}